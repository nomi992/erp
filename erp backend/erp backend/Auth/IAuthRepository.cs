using erp_backend.Auth.Dtos;

namespace erp_backend.Auth;

public interface IAuthRepository
{
    Task<LoginResponse> LoginAsync(LoginRequest request);
}
