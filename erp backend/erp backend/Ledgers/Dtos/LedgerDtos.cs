using erp_backend.Models;

namespace erp_backend.Ledgers.Dtos;

public class GeneralLedgerEntryResponse
{
    public int VoucherId { get; set; }
    public string VoucherNo { get; set; } = string.Empty;
    public VoucherType VoucherType { get; set; }
    public DateTime Date { get; set; }
    public string Narration { get; set; } = string.Empty;
    public int AccountId { get; set; }
    public string AccountCode { get; set; } = string.Empty;
    public string AccountName { get; set; } = string.Empty;
    public decimal DebitAmount { get; set; }
    public decimal CreditAmount { get; set; }
}

public class AccountLedgerEntryResponse
{
    public int VoucherId { get; set; }
    public string VoucherNo { get; set; } = string.Empty;
    public DateTime Date { get; set; }
    public string Narration { get; set; } = string.Empty;
    public decimal DebitAmount { get; set; }
    public decimal CreditAmount { get; set; }
    public decimal RunningBalance { get; set; }
    public AccountNature RunningBalanceNature { get; set; }
}

public class AccountLedgerResponse
{
    public int AccountId { get; set; }
    public string AccountCode { get; set; } = string.Empty;
    public string AccountName { get; set; } = string.Empty;
    public decimal OpeningBalance { get; set; }
    public AccountNature OpeningBalanceNature { get; set; }
    public decimal ClosingBalance { get; set; }
    public AccountNature ClosingBalanceNature { get; set; }
    public List<AccountLedgerEntryResponse> Entries { get; set; } = [];
}

public class SubLedgerEntryResponse
{
    public int AccountId { get; set; }
    public string AccountCode { get; set; } = string.Empty;
    public string AccountName { get; set; } = string.Empty;
    public decimal OutstandingBalance { get; set; }
    public AccountNature OutstandingBalanceNature { get; set; }
    public DateTime? LastActivityDate { get; set; }
    public int AgeInDays { get; set; }
    public string AgeBucket { get; set; } = string.Empty;
}
