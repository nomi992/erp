using erp_backend.Models.Abstractions;

namespace erp_backend.Models;

public class BankStatementLine : IBranchScoped
{
    public int Id { get; set; }
    public int TenantId { get; set; }
    public int BranchId { get; set; }
    public int BankAccountId { get; set; }
    public Account? BankAccount { get; set; }
    public DateTime TransactionDate { get; set; }
    public decimal Amount { get; set; }
    public string Description { get; set; } = string.Empty;
    public string ReferenceNo { get; set; } = string.Empty;
    public bool IsMatched { get; set; }
    public int? MatchedVoucherDetailId { get; set; }
    public VoucherDetail? MatchedVoucherDetail { get; set; }
    public DateTime ImportedAtUtc { get; set; } = DateTime.UtcNow;
}
