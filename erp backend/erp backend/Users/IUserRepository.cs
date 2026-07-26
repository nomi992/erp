using erp_backend.Users.Dtos;

namespace erp_backend.Users;

public interface IUserRepository
{
    Task<List<UserResponse>> GetAllAsync(int? tenantId);
    Task<UserResponse> GetByIdAsync(int id);
    Task<UserResponse> CreateAsync(CreateUserRequest request);
    Task<UserResponse> UpdateAsync(int id, UpdateUserRequest request);
    Task ActivateAsync(int id);
    Task DeactivateAsync(int id);
    Task GrantBranchAsync(int userId, int branchId);
    Task RevokeBranchAsync(int userId, int branchId);
}
