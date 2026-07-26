using System.ComponentModel.DataAnnotations;
using erp_backend.Models;

namespace erp_backend.Reporting.Dtos;

public class BudgetRequest
{
    [Required]
    public int AccountId { get; set; }

    [Range(2000, 2100)]
    public int Year { get; set; }

    [Range(1, 12)]
    public int Month { get; set; }

    public decimal BudgetedAmount { get; set; }
}

public class BudgetResponse
{
    public int Id { get; set; }
    public int AccountId { get; set; }
    public string AccountCode { get; set; } = string.Empty;
    public string AccountName { get; set; } = string.Empty;
    public int Year { get; set; }
    public int Month { get; set; }
    public decimal BudgetedAmount { get; set; }

    public static BudgetResponse FromEntity(Budget budget) => new()
    {
        Id = budget.Id,
        AccountId = budget.AccountId,
        AccountCode = budget.Account?.Code ?? string.Empty,
        AccountName = budget.Account?.Name ?? string.Empty,
        Year = budget.Year,
        Month = budget.Month,
        BudgetedAmount = budget.BudgetedAmount,
    };
}
