using erp_backend.Auth;
using erp_backend.Auth.Dtos;
using erp_backend.Data;
using erp_backend.Exceptions;
using erp_backend.Messages;
using erp_backend.Models;
using erp_backend.Users.Dtos;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace erp_backend.Users;

public class UserRepository : IUserRepository
{
    private readonly AppDbContext _context;
    private readonly ICurrentTenantContext _tenantContext;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly PasswordHasher<User> _passwordHasher = new();

    public UserRepository(AppDbContext context, ICurrentTenantContext tenantContext, IHttpContextAccessor httpContextAccessor)
    {
        _context = context;
        _tenantContext = tenantContext;
        _httpContextAccessor = httpContextAccessor;
    }

    private bool IsSystemAdmin => _httpContextAccessor.HttpContext?.User.IsInRole(AppRoles.SystemAdmin) == true;

    private IQueryable<User> UsersQuery() =>
        (IsSystemAdmin ? _context.Users.IgnoreQueryFilters() : _context.Users).Include(u => u.Role);

    public async Task<List<UserResponse>> GetAllAsync(int? tenantId)
    {
        var query = UsersQuery().Include(u => u.Tenant).AsQueryable();

        if (IsSystemAdmin && tenantId is int t)
        {
            query = query.Where(u => u.TenantId == t);
        }

        var users = await query.OrderBy(u => u.Username).ToListAsync();
        return await ToResponsesAsync(users);
    }

    public async Task<UserResponse> GetByIdAsync(int id)
    {
        var user = await UsersQuery().Include(u => u.Tenant).FirstOrDefaultAsync(u => u.Id == id)
            ?? throw new NotFoundException(ResponseMessage.AppUserNotFound);

        var responses = await ToResponsesAsync([user]);
        return responses[0];
    }

    public async Task<UserResponse> CreateAsync(CreateUserRequest request)
    {
        var targetTenantId = IsSystemAdmin && request.TenantId is int t ? t : _tenantContext.TenantId;

        var role = await _context.Roles.IgnoreQueryFilters().FirstOrDefaultAsync(r => r.Id == request.RoleId)
            ?? throw new NotFoundException(ResponseMessage.RoleNotFound);

        if (role.TenantId is int roleTenantId && roleTenantId != targetTenantId)
        {
            throw new BadRequestException(ResponseMessage.RoleTenantMismatch);
        }

        if (role.IsSystemRole && role.Name == AppRoles.SystemAdmin && !IsSystemAdmin)
        {
            throw new ForbiddenException(ResponseMessage.OnlySystemAdminCanAssignSystemAdmin);
        }

        var tenant = await _context.Tenants.FirstOrDefaultAsync(x => x.Id == targetTenantId)
            ?? throw new NotFoundException(ResponseMessage.TenantNotFound);

        var usernameExists = await _context.Users.IgnoreQueryFilters().AnyAsync(u => u.Username == request.Username);
        if (usernameExists)
        {
            throw new BadRequestException(ResponseMessage.UserUsernameExists);
        }

        var branches = request.BranchIds.Count == 0
            ? []
            : await _context.Branches.Where(b => request.BranchIds.Contains(b.Id)).ToListAsync();

        if (branches.Any(b => b.TenantId != targetTenantId))
        {
            throw new BadRequestException(ResponseMessage.BranchTenantMismatch);
        }

        var user = new User
        {
            TenantId = targetTenantId,
            Username = request.Username,
            RoleId = role.Id,
            Role = role,
        };
        user.PasswordHash = _passwordHasher.HashPassword(user, request.Password);

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        foreach (var branch in branches)
        {
            _context.UserBranches.Add(new UserBranch { UserId = user.Id, BranchId = branch.Id });
        }

        if (branches.Count > 0)
        {
            await _context.SaveChangesAsync();
        }

        var responses = await ToResponsesAsync([user], tenant.Name);
        return responses[0];
    }

    public async Task<UserResponse> UpdateAsync(int id, UpdateUserRequest request)
    {
        var user = await UsersQuery().FirstOrDefaultAsync(u => u.Id == id)
            ?? throw new NotFoundException(ResponseMessage.AppUserNotFound);

        var targetRole = await _context.Roles.IgnoreQueryFilters().FirstOrDefaultAsync(r => r.Id == request.RoleId)
            ?? throw new NotFoundException(ResponseMessage.RoleNotFound);

        if (targetRole.TenantId is int roleTenantId && roleTenantId != user.TenantId)
        {
            throw new BadRequestException(ResponseMessage.RoleTenantMismatch);
        }

        var isEscalatingToSystemAdmin =
            targetRole.Name == AppRoles.SystemAdmin &&
            user.Role?.Name != AppRoles.SystemAdmin;

        if (isEscalatingToSystemAdmin && !IsSystemAdmin)
        {
            throw new ForbiddenException(ResponseMessage.OnlySystemAdminCanAssignSystemAdmin);
        }

        user.RoleId = targetRole.Id;
        user.Role = targetRole;
        user.IsActive = request.IsActive;
        await _context.SaveChangesAsync();

        var responses = await ToResponsesAsync([user]);
        return responses[0];
    }

    public Task ActivateAsync(int id) => SetActiveAsync(id, true);

    public Task DeactivateAsync(int id) => SetActiveAsync(id, false);

    public async Task GrantBranchAsync(int userId, int branchId)
    {
        var user = await UsersQuery().FirstOrDefaultAsync(u => u.Id == userId)
            ?? throw new NotFoundException(ResponseMessage.AppUserNotFound);

        var branch = await _context.Branches.FirstOrDefaultAsync(b => b.Id == branchId)
            ?? throw new NotFoundException(ResponseMessage.BranchNotFound);

        if (branch.TenantId != user.TenantId)
        {
            throw new BadRequestException(ResponseMessage.BranchTenantMismatch);
        }

        var alreadyGranted = await _context.UserBranches.AnyAsync(ub => ub.UserId == userId && ub.BranchId == branchId);
        if (!alreadyGranted)
        {
            _context.UserBranches.Add(new UserBranch { UserId = userId, BranchId = branchId });
            await _context.SaveChangesAsync();
        }
    }

    public async Task RevokeBranchAsync(int userId, int branchId)
    {
        var user = await UsersQuery().FirstOrDefaultAsync(u => u.Id == userId)
            ?? throw new NotFoundException(ResponseMessage.AppUserNotFound);

        var grant = await _context.UserBranches.FirstOrDefaultAsync(ub => ub.UserId == user.Id && ub.BranchId == branchId);
        if (grant is not null)
        {
            _context.UserBranches.Remove(grant);
            await _context.SaveChangesAsync();
        }
    }

    private async Task SetActiveAsync(int id, bool isActive)
    {
        var user = await UsersQuery().FirstOrDefaultAsync(u => u.Id == id)
            ?? throw new NotFoundException(ResponseMessage.AppUserNotFound);

        user.IsActive = isActive;
        await _context.SaveChangesAsync();
    }

    private async Task<List<UserResponse>> ToResponsesAsync(List<User> users, string? sharedTenantName = null)
    {
        var userIds = users.Select(u => u.Id).ToList();
        var tenantIds = users.Select(u => u.TenantId).Distinct().ToList();

        var tenantNames = sharedTenantName is not null
            ? tenantIds.ToDictionary(t => t, _ => sharedTenantName)
            : await _context.Tenants.Where(t => tenantIds.Contains(t.Id)).ToDictionaryAsync(t => t.Id, t => t.Name);

        var branchesByUser = await _context.UserBranches
            .Where(ub => userIds.Contains(ub.UserId))
            .Join(_context.Branches, ub => ub.BranchId, b => b.Id, (ub, b) => new { ub.UserId, Branch = b })
            .ToListAsync();

        return users.Select(u => UserResponse.FromEntity(
            u,
            tenantNames.GetValueOrDefault(u.TenantId),
            branchesByUser
                .Where(x => x.UserId == u.Id)
                .Select(x => new BranchSummary { Id = x.Branch.Id, Name = x.Branch.Name, Code = x.Branch.Code })
                .OrderBy(b => b.Name)
                .ToList())).ToList();
    }
}
