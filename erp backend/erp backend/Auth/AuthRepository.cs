using erp_backend.Auth.Dtos;
using erp_backend.Data;
using erp_backend.Exceptions;
using erp_backend.Messages;
using erp_backend.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace erp_backend.Auth;

public class AuthRepository : IAuthRepository
{
    private readonly AppDbContext _context;
    private readonly IJwtTokenService _jwtTokenService;
    private readonly PasswordHasher<User> _passwordHasher = new();

    public AuthRepository(AppDbContext context, IJwtTokenService jwtTokenService)
    {
        _context = context;
        _jwtTokenService = jwtTokenService;
    }

    public async Task<LoginResponse> LoginAsync(LoginRequest request)
    {
        // Login must resolve a user before any tenant context exists, so it deliberately
        // bypasses the tenant/branch query filters (Username is globally unique).
        var user = await _context.Users
            .IgnoreQueryFilters()
            .Include(u => u.Role)
            .FirstOrDefaultAsync(u => u.Username == request.Username);

        if (user is null ||
            _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, request.Password) == PasswordVerificationResult.Failed)
        {
            throw new UnauthorizedAppException(ResponseMessage.InvalidCredentials);
        }

        if (!user.IsActive)
        {
            throw new UnauthorizedAppException(ResponseMessage.InvalidCredentials);
        }

        var tenant = await _context.Tenants.IgnoreQueryFilters().FirstOrDefaultAsync(t => t.Id == user.TenantId);

        if (tenant is null || !tenant.IsActive)
        {
            throw new UnauthorizedAppException(ResponseMessage.InvalidCredentials);
        }

        var branches = await _context.UserBranches
            .IgnoreQueryFilters()
            .Where(ub => ub.UserId == user.Id)
            .Join(_context.Branches.IgnoreQueryFilters(), ub => ub.BranchId, b => b.Id, (ub, b) => b)
            .Where(b => b.IsActive)
            .Select(b => new BranchSummary { Id = b.Id, Name = b.Name, Code = b.Code })
            .OrderBy(b => b.Name)
            .ToListAsync();

        var rightCodes = await _context.RoleRights
            .Where(rr => rr.RoleId == user.RoleId)
            .Join(_context.Rights, rr => rr.RightId, r => r.Id, (rr, r) => r.Code)
            .ToListAsync();

        var (token, expiresAtUtc) = _jwtTokenService.GenerateToken(user, user.Role!.Name, rightCodes);

        return new LoginResponse
        {
            Token = token,
            Username = user.Username,
            Role = user.Role.Name,
            ExpiresAtUtc = expiresAtUtc,
            TenantId = user.TenantId,
            TenantName = tenant?.Name ?? string.Empty,
            Branches = branches,
            Rights = rightCodes,
        };
    }
}
