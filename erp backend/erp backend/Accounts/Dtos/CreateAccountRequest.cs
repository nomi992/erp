using System.ComponentModel.DataAnnotations;
using erp_backend.Models;

namespace erp_backend.Accounts.Dtos;

public class CreateAccountRequest
{
    [Required]
    public string Code { get; set; } = string.Empty;

    [Required]
    public string Name { get; set; } = string.Empty;

    [Required]
    public AccountType Type { get; set; }

    [Required]
    public AccountNature Nature { get; set; }

    public int? ParentAccountId { get; set; }

    public bool IsControlAccount { get; set; }

    public bool IsCashAccount { get; set; }

    public bool IsBankAccount { get; set; }

    public decimal OpeningBalance { get; set; }

    public AccountNature OpeningBalanceNature { get; set; } = AccountNature.Debit;
}
