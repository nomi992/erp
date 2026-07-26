using erp_backend.Tenants.Dtos;

namespace erp_backend.Tenants;

public interface ITenantRepository
{
    Task<List<TenantResponse>> GetAllAsync();
    Task<TenantResponse> GetByIdAsync(int id);
    Task<TenantResponse> CreateAsync(CreateTenantRequest request);
    Task<TenantResponse> UpdateAsync(int id, UpdateTenantRequest request);
    Task ActivateAsync(int id);
    Task DeactivateAsync(int id);
}
