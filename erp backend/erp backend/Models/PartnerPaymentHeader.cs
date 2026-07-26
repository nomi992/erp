using erp_backend.Models.Abstractions;

namespace erp_backend.Models;

public enum PaymentDirection
{
    CustomerReceipt,
    SupplierPayment,
}

public enum PartnerPaymentStatus
{
    Draft,
    PendingApproval,
    Posted,
    Rejected,
}

public class PartnerPaymentHeader : IBranchScoped
{
    public int Id { get; set; }
    public int TenantId { get; set; }
    public int BranchId { get; set; }
    public PaymentDirection Direction { get; set; }
    public string PaymentNo { get; set; } = string.Empty;
    public int PartnerId { get; set; }
    public BusinessPartner? Partner { get; set; }
    public DateTime Date { get; set; }
    public int BankOrCashAccountId { get; set; }
    public Account? BankOrCashAccount { get; set; }
    public decimal TotalAmount { get; set; }
    public string? Narration { get; set; }
    public PartnerPaymentStatus Status { get; set; } = PartnerPaymentStatus.Draft;
    public int? LinkedVoucherId { get; set; }
    public VoucherHeader? LinkedVoucher { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    public string? ApprovedBy { get; set; }
    public DateTime? ApprovedAtUtc { get; set; }

    public List<PartnerPaymentAllocation> Allocations { get; set; } = [];

    [System.ComponentModel.DataAnnotations.Timestamp]
    public byte[] RowVersion { get; set; } = [];
}
