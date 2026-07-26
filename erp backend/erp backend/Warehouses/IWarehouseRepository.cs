using erp_backend.Warehouses.Dtos;

namespace erp_backend.Warehouses;

public interface IWarehouseRepository
{
    Task<List<WarehouseResponse>> GetAllAsync();
    Task<WarehouseResponse> GetByIdAsync(int id);
    Task<WarehouseResponse> CreateAsync(WarehouseRequest request);
    Task<WarehouseResponse> UpdateAsync(int id, WarehouseRequest request);
    Task ActivateAsync(int id);
    Task DeactivateAsync(int id);
}
