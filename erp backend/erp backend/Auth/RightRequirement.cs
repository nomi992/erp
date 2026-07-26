using Microsoft.AspNetCore.Authorization;

namespace erp_backend.Auth;

public class RightRequirement : IAuthorizationRequirement
{
    public string RightCode { get; }

    public RightRequirement(string rightCode)
    {
        RightCode = rightCode;
    }
}
