using erp_backend.Models.Abstractions;

namespace erp_backend.Models;

public enum VoucherType
{
    Payment,
    Receipt,
    Journal,
    Sales,
    Purchase,
    Contra,
    DebitNote,
    CreditNote,
}

public enum VoucherStatus
{
    Draft,
    PendingApproval,
    Posted,
    Rejected,
}

public class VoucherHeader : IBranchScoped
{
    public int Id { get; set; }
    public int TenantId { get; set; }
    public int BranchId { get; set; }
    public VoucherType VoucherType { get; set; }
    public string VoucherNo { get; set; } = string.Empty;
    public DateTime Date { get; set; }
    public string Narration { get; set; } = string.Empty;
    public VoucherStatus Status { get; set; } = VoucherStatus.Draft;
    public string CurrencyCode { get; set; } = "USD";
    public decimal ExchangeRate { get; set; } = 1;
    public int? ReversalOfVoucherId { get; set; }
    public VoucherHeader? ReversalOfVoucher { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    public string? ApprovedBy { get; set; }
    public DateTime? ApprovedAtUtc { get; set; }

    public List<VoucherDetail> Details { get; set; } = [];
    public List<VoucherAttachment> Attachments { get; set; } = [];
}
