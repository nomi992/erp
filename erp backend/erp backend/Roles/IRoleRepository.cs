using erp_backend.Roles.Dtos;

namespace erp_backend.Roles;

public interface IRoleRepository
{
    Task<List<RoleResponse>> GetAllAsync(int? tenantId);
    Task<RoleResponse> GetByIdAsync(int id);
    Task<RoleResponse> CreateAsync(RoleRequest request);
    Task<RoleResponse> UpdateAsync(int id, RoleRequest request);
    Task DeleteAsync(int id);
}
