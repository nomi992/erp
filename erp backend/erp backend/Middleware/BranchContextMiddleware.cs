using System.Security.Claims;
using erp_backend.Auth;
using erp_backend.Data;
using Microsoft.EntityFrameworkCore;

namespace erp_backend.Middleware;

public class BranchContextMiddleware
{
    public const string BranchHeaderName = "X-Branch-Id";

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

            var userIdClaim = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(userIdClaim, out var userId))
            {
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                return;
            }

            var hasGrant = await dbContext.UserBranches
                .Where(ub => ub.UserId == userId && ub.BranchId == branchId)
                .Join(dbContext.Branches, ub => ub.BranchId, b => b.Id, (ub, b) => b)
                .AnyAsync(b => b.TenantId == tenantId && b.IsActive);

            if (!hasGrant)
            {
                context.Response.StatusCode = StatusCodes.Status403Forbidden;
                await context.Response.WriteAsJsonAsync(new { message = "You do not have access to this branch." });
                return;
            }

            tenantContext.SetBranch(branchId);
        }

        await _next(context);
    }
}
