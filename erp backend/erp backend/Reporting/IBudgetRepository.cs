using erp_backend.Reporting.Dtos;

namespace erp_backend.Reporting;

public interface IBudgetRepository
{
    Task<List<BudgetResponse>> GetAllAsync(int? year, int? month);
    Task<BudgetResponse> CreateAsync(BudgetRequest request);
    Task<BudgetResponse> UpdateAsync(int id, BudgetRequest request);
    Task DeleteAsync(int id);
}
