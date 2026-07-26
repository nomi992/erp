using erp_backend.CostCenters.Dtos;

namespace erp_backend.CostCenters;

public interface ICostCenterRepository
{
    Task<List<CostCenterResponse>> GetAllAsync();
    Task<CostCenterResponse> GetByIdAsync(int id);
    Task<CostCenterResponse> CreateAsync(CostCenterRequest request);
    Task<CostCenterResponse> UpdateAsync(int id, CostCenterRequest request);
    Task ActivateAsync(int id);
    Task DeactivateAsync(int id);
    Task DeleteAsync(int id);
}
