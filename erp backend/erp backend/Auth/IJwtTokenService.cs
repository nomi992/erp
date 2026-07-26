using erp_backend.Models;

namespace erp_backend.Auth;

public interface IJwtTokenService
{
    (string Token, DateTime ExpiresAtUtc) GenerateToken(User user, string roleName, IReadOnlyCollection<string> rightCodes);
}
