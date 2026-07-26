using erp_backend.Models;

namespace erp_backend.Accounts.Dtos;

public class AccountResponse
{
    public int Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public AccountType Type { get; set; }
    public AccountNature Nature { get; set; }
    public int? ParentAccountId { get; set; }
    public string? ParentAccountName { get; set; }
    public bool IsActive { get; set; }
    public bool IsControlAccount { get; set; }
    public bool IsCashAccount { get; set; }
    public decimal OpeningBalance { get; set; }
    public AccountNature OpeningBalanceNature { get; set; }

    public static AccountResponse FromEntity(Account account, string? parentAccountName = null) =>
        new()
        {
            Id = account.Id,
            Code = account.Code,
            Name = account.Name,
            Type = account.Type,
            Nature = account.Nature,
            ParentAccountId = account.ParentAccountId,
            ParentAccountName = parentAccountName,
            IsActive = account.IsActive,
            IsControlAccount = account.IsControlAccount,
            IsCashAccount = account.IsCashAccount,
            OpeningBalance = account.OpeningBalance,
            OpeningBalanceNature = account.OpeningBalanceNature,
        };
}
