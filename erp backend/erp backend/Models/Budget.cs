using erp_backend.Models.Abstractions;

namespace erp_backend.Models;

public class Budget : IBranchScoped
{
    public int Id { get; set; }
    public int TenantId { get; set; }
    public int BranchId { get; set; }
    public int AccountId { get; set; }
    public Account? Account { get; set; }
    public int Year { get; set; }
    public int Month { get; set; }
    public decimal BudgetedAmount { get; set; }
}
