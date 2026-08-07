using erp_backend.Models.Abstractions;

namespace erp_backend.Models;

public enum AccountType
{
    Asset,
    Liability,
    Equity,
    Income,
    Expense,
}

public enum AccountNature
{
    Debit,
    Credit,
}

public class Account : ITenantScoped
{
    public int Id { get; set; }
    public int TenantId { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public AccountType Type { get; set; }
    public AccountNature Nature { get; set; }
    public int? ParentAccountId { get; set; }
    public Account? ParentAccount { get; set; }
    public bool IsActive { get; set; } = true;
    public bool IsControlAccount { get; set; }
    public bool IsCashAccount { get; set; }
    public bool IsBankAccount { get; set; }
    public decimal OpeningBalance { get; set; }
    public AccountNature OpeningBalanceNature { get; set; } = AccountNature.Debit;
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
}
