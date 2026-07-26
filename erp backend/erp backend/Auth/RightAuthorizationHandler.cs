using Microsoft.AspNetCore.Authorization;

namespace erp_backend.Auth;

public class RightAuthorizationHandler : AuthorizationHandler<RightRequirement>
{
    protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, RightRequirement requirement)
    {
        if (context.User.IsInRole(AppRoles.SystemAdmin) ||
            context.User.HasClaim("right", requirement.RightCode))
        {
            context.Succeed(requirement);
        }

        return Task.CompletedTask;
    }
}
