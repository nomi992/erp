using erp_backend.Exceptions;
using erp_backend.Messages;
using erp_backend.Models;
using erp_backend.Repositories;
using erp_backend.TaxRates.Dtos;

namespace erp_backend.TaxRates;

public class TaxRateRepository : ITaxRateRepository
{
    private readonly IRepository<TaxRate> _taxRates;

    public TaxRateRepository(IRepository<TaxRate> taxRates)
    {
        _taxRates = taxRates;
    }

    public async Task<List<TaxRate>> GetAllAsync() =>
        (await _taxRates.GetAllAsync()).OrderBy(r => r.Name).ToList();

    public async Task<TaxRate> CreateAsync(TaxRateRequest request)
    {
        var taxRate = new TaxRate { Name = request.Name, Percentage = request.Percentage };
        await _taxRates.AddAsync(taxRate);
        await _taxRates.SaveChangesAsync();

        return taxRate;
    }

    public async Task<TaxRate> UpdateAsync(int id, TaxRateRequest request)
    {
        var taxRate = await _taxRates.GetByIdAsync(id) ?? throw new NotFoundException(ResponseMessage.TaxRateNotFound);

        taxRate.Name = request.Name;
        taxRate.Percentage = request.Percentage;
        _taxRates.Update(taxRate);
        await _taxRates.SaveChangesAsync();

        return taxRate;
    }

    public Task ActivateAsync(int id) => SetActiveAsync(id, true);

    public Task DeactivateAsync(int id) => SetActiveAsync(id, false);

    private async Task SetActiveAsync(int id, bool isActive)
    {
        var taxRate = await _taxRates.GetByIdAsync(id) ?? throw new NotFoundException(ResponseMessage.TaxRateNotFound);

        taxRate.IsActive = isActive;
        _taxRates.Update(taxRate);
        await _taxRates.SaveChangesAsync();
    }
}
