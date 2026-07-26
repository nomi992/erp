using erp_backend.UnitsOfMeasure.Dtos;

namespace erp_backend.UnitsOfMeasure;

public interface IUnitOfMeasureRepository
{
    Task<List<UnitOfMeasureResponse>> GetAllAsync();
    Task<UnitOfMeasureResponse> GetByIdAsync(int id);
    Task<UnitOfMeasureResponse> CreateAsync(UnitOfMeasureRequest request);
    Task<UnitOfMeasureResponse> UpdateAsync(int id, UnitOfMeasureRequest request);
    Task ActivateAsync(int id);
    Task DeactivateAsync(int id);
}
