using erp_backend.Models;
using erp_backend.TaxRates.Dtos;

namespace erp_backend.TaxRates;

public interface ITaxRateRepository
{
    Task<List<TaxRate>> GetAllAsync();
    Task<TaxRate> CreateAsync(TaxRateRequest request);
    Task<TaxRate> UpdateAsync(int id, TaxRateRequest request);
    Task ActivateAsync(int id);
    Task DeactivateAsync(int id);
}
