using System.Security.Claims;
using erp_backend.Auth;
using erp_backend.Data;
using Microsoft.EntityFrameworkCore;

namespace erp_backend.Middleware;

public class BranchContextMiddleware
{
    public const string BranchHeaderName = "X-Branch-Id";

    // SystemAdmin-only: lets a cross-tenant admin browse another tenant's branch-scoped
    // data (invoices, vouchers, stock, etc.) by overriding the tenant resolved from the JWT,
    // which for a SystemAdmin always points at the internal PLATFORM tenant, never a real one.
    public const string TenantHeaderName = "X-Tenant-Id";

    private readonly RequestDelegate _next;

    public BranchContextMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context, AppDbContext dbContext, ICurrentTenantContext tenantContext)
    {
        if (context.User.Identity?.IsAuthenticated != true)
        {
            await _next(context);
            return;
        }

        var tenantIdClaim = context.User.FindFirst(TenantClaimTypes.TenantId)?.Value;
        if (!int.TryParse(tenantIdClaim, out var tenantId))
        {
            await _next(context);
            return;
        }

        var isSystemAdmin = context.User.IsInRole(AppRoles.SystemAdmin);

        if (isSystemAdmin && context.Request.Headers.TryGetValue(TenantHeaderName, out var tenantHeaderValues))
        {
            var tenantHeaderValue = tenantHeaderValues.ToString();
            if (!int.TryParse(tenantHeaderValue, out var overrideTenantId))
            {
                context.Response.StatusCode = StatusCodes.Status400BadRequest;
                await context.Response.WriteAsJsonAsync(new { message = $"Invalid {TenantHeaderName} header." });
                return;
            }

            var tenantExists = await dbContext.Tenants.AnyAsync(t => t.Id == overrideTenantId);
            if (!tenantExists)
            {
                context.Response.StatusCode = StatusCodes.Status404NotFound;
                await context.Response.WriteAsJsonAsync(new { message = "Tenant not found." });
                return;
            }

            tenantId = overrideTenantId;
        }

        tenantContext.SetTenant(tenantId);

        if (context.Request.Headers.TryGetValue(BranchHeaderName, out var branchHeaderValues))
        {
            var branchHeaderValue = branchHeaderValues.ToString();

            if (!int.TryParse(branchHeaderValue, out var branchId))
            {
                context.Response.StatusCode = StatusCodes.Status400BadRequest;
                await context.Response.WriteAsJsonAsync(new { message = $"Invalid {BranchHeaderName} header." });
                return;
            }

            // A SystemAdmin has no UserBranch grants of their own (they don't belong to any
            // real tenant) — access to any active branch of the resolved tenant is implicit.
            var hasGrant = isSystemAdmin
                ? await dbContext.Branches.AnyAsync(b => b.Id == branchId && b.TenantId == tenantId && b.IsActive)
                : await HasBranchGrantAsync(context, dbContext, tenantId, branchId);

            if (hasGrant is null)
            {
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                return;
            }

            if (hasGrant == false)
            {
                context.Response.StatusCode = StatusCodes.Status403Forbidden;
                await context.Response.WriteAsJsonAsync(new { message = "You do not have access to this branch." });
                return;
            }

            tenantContext.SetBranch(branchId);
        }

        await _next(context);
    }

    private static async Task<bool?> HasBranchGrantAsync(HttpContext context, AppDbContext dbContext, int tenantId, int branchId)
    {
        var userIdClaim = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!int.TryParse(userIdClaim, out var userId))
        {
            return null;
        }

        return await dbContext.UserBranches
            .Where(ub => ub.UserId == userId && ub.BranchId == branchId)
            .Join(dbContext.Branches, ub => ub.BranchId, b => b.Id, (ub, b) => b)
            .AnyAsync(b => b.TenantId == tenantId && b.IsActive);
    }
}
