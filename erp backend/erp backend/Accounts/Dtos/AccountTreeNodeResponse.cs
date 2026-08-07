using erp_backend.Models;

namespace erp_backend.Accounts.Dtos;

public class AccountTreeNodeResponse
{
    public int Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public AccountType Type { get; set; }
    public AccountNature Nature { get; set; }
    public int? ParentAccountId { get; set; }
    public bool IsActive { get; set; }
    public bool IsControlAccount { get; set; }
    public bool IsCashAccount { get; set; }
    public bool IsBankAccount { get; set; }
    public decimal OpeningBalance { get; set; }
    public AccountNature OpeningBalanceNature { get; set; }
    public List<AccountTreeNodeResponse> Children { get; set; } = [];

    public static AccountTreeNodeResponse FromEntity(Account account) =>
        new()
        {
            Id = account.Id,
            Code = account.Code,
            Name = account.Name,
            Type = account.Type,
            Nature = account.Nature,
            ParentAccountId = account.ParentAccountId,
            IsActive = account.IsActive,
            IsControlAccount = account.IsControlAccount,
            IsCashAccount = account.IsCashAccount,
            IsBankAccount = account.IsBankAccount,
            OpeningBalance = account.OpeningBalance,
            OpeningBalanceNature = account.OpeningBalanceNature,
        };
}
