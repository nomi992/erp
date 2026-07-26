using erp_backend.FiscalPeriods.Dtos;
using erp_backend.Models;

namespace erp_backend.FiscalPeriods;

public interface IFiscalPeriodRepository
{
    Task<List<FiscalPeriod>> GetAllAsync();
    Task<FiscalPeriod> GetByIdAsync(int id);
    Task<FiscalPeriod> CreateAsync(FiscalPeriodRequest request);
    Task CloseAsync(int id);
    Task OpenAsync(int id);
}
