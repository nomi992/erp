using erp_backend.Auth;
using erp_backend.Data;
using erp_backend.Exceptions;
using erp_backend.Messages;
using erp_backend.Models;
using erp_backend.Roles.Dtos;
using Microsoft.EntityFrameworkCore;
using RightResponse = erp_backend.Rights.Dtos.RightResponse;

namespace erp_backend.Roles;

public class RoleRepository : IRoleRepository
{
    private readonly AppDbContext _context;
    private readonly ICurrentTenantContext _tenantContext;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public RoleRepository(AppDbContext context, ICurrentTenantContext tenantContext, IHttpContextAccessor httpContextAccessor)
    {
        _context = context;
        _tenantContext = tenantContext;
        _httpContextAccessor = httpContextAccessor;
    }

    private bool IsSystemAdmin => _httpContextAccessor.HttpContext?.User.IsInRole(AppRoles.SystemAdmin) == true;

    private IQueryable<Role> RolesQuery() => IsSystemAdmin ? _context.Roles.IgnoreQueryFilters() : _context.Roles;

    public async Task<List<RoleResponse>> GetAllAsync(int? tenantId)
    {
        var query = RolesQuery().Include(r => r.Tenant).Include(r => r.RoleRights).ThenInclude(rr => rr.Right).AsQueryable();

        if (IsSystemAdmin && tenantId is int t)
        {
            query = query.Where(r => r.TenantId == null || r.TenantId == t);
        }

        var roles = await query.OrderBy(r => r.Name).ToListAsync();
        return roles.Select(r => RoleResponse.FromEntity(r, null)).ToList();
    }

    public async Task<RoleResponse> GetByIdAsync(int id)
    {
        var role = await RolesQuery()
            .Include(r => r.Tenant)
            .Include(r => r.RoleRights).ThenInclude(rr => rr.Right)
            .FirstOrDefaultAsync(r => r.Id == id)
            ?? throw new NotFoundException(ResponseMessage.RoleNotFound);

        return RoleResponse.FromEntity(role, null);
    }

    public async Task<RoleResponse> CreateAsync(RoleRequest request)
    {
        var targetTenantId = IsSystemAdmin && request.TenantId is int t ? t : _tenantContext.TenantId;

        var rights = await _context.Rights.Where(r => request.RightIds.Contains(r.Id)).ToListAsync();
        if (rights.Count != request.RightIds.Distinct().Count())
        {
            throw new BadRequestException(ResponseMessage.RightNotFound);
        }

        var nameExists = await _context.Roles.IgnoreQueryFilters()
            .AnyAsync(r => (r.TenantId == null || r.TenantId == targetTenantId) && r.Name == request.Name);
        if (nameExists)
        {
            throw new BadRequestException(ResponseMessage.RoleNameExists);
        }

        var role = new Role
        {
            TenantId = targetTenantId,
            Name = request.Name,
            Description = request.Description,
            IsSystemRole = false,
            RoleRights = rights.Select(r => new RoleRight { RightId = r.Id }).ToList(),
        };

        _context.Roles.Add(role);
        await _context.SaveChangesAsync();

        var tenantName = await _context.Tenants.Where(t2 => t2.Id == targetTenantId).Select(t2 => t2.Name).FirstOrDefaultAsync();

        return BuildResponse(role, rights, tenantName);
    }

    public async Task<RoleResponse> UpdateAsync(int id, RoleRequest request)
    {
        var role = await RolesQuery()
            .Include(r => r.RoleRights)
            .FirstOrDefaultAsync(r => r.Id == id)
            ?? throw new NotFoundException(ResponseMessage.RoleNotFound);

        if (role.IsSystemRole && !string.Equals(request.Name, role.Name, StringComparison.Ordinal))
        {
            throw new BadRequestException(ResponseMessage.RoleNameNotEditableForSystemRole);
        }

        var rights = await _context.Rights.Where(r => request.RightIds.Contains(r.Id)).ToListAsync();
        if (rights.Count != request.RightIds.Distinct().Count())
        {
            throw new BadRequestException(ResponseMessage.RightNotFound);
        }

        if (!role.IsSystemRole)
        {
            var nameExists = await _context.Roles.IgnoreQueryFilters()
                .AnyAsync(r => r.Id != id && (r.TenantId == null || r.TenantId == role.TenantId) && r.Name == request.Name);
            if (nameExists)
            {
                throw new BadRequestException(ResponseMessage.RoleNameExists);
            }

            role.Name = request.Name;
        }

        role.Description = request.Description;

        _context.RoleRights.RemoveRange(role.RoleRights);
        role.RoleRights = rights.Select(r => new RoleRight { RoleId = role.Id, RightId = r.Id }).ToList();

        await _context.SaveChangesAsync();

        var tenantName = role.TenantId is int tenantId
            ? await _context.Tenants.Where(t => t.Id == tenantId).Select(t => t.Name).FirstOrDefaultAsync()
            : null;

        return BuildResponse(role, rights, tenantName);
    }

    public async Task DeleteAsync(int id)
    {
        var role = await RolesQuery().FirstOrDefaultAsync(r => r.Id == id)
            ?? throw new NotFoundException(ResponseMessage.RoleNotFound);

        if (role.IsSystemRole)
        {
            throw new BadRequestException(ResponseMessage.RoleIsSystemRoleCannotDelete);
        }

        var isInUse = await _context.Users.IgnoreQueryFilters().AnyAsync(u => u.RoleId == id);
        if (isInUse)
        {
            throw new BadRequestException(ResponseMessage.RoleInUse);
        }

        _context.Roles.Remove(role);
        await _context.SaveChangesAsync();
    }

    private static RoleResponse BuildResponse(Role role, List<Right> rights, string? tenantName) =>
        new()
        {
            Id = role.Id,
            TenantId = role.TenantId,
            TenantName = tenantName,
            Name = role.Name,
            Description = role.Description,
            IsSystemRole = role.IsSystemRole,
            CreatedAtUtc = role.CreatedAtUtc,
            Rights = rights
                .Select(RightResponse.FromEntity)
                .OrderBy(r => r.Module).ThenBy(r => r.Code)
                .ToList(),
        };
}
