using erp_backend.Models;

namespace erp_backend.PartnerPayments.Dtos;

public class PartnerPaymentAllocationResponse
{
    public int Id { get; set; }
    public int InvoiceHeaderId { get; set; }
    public string InvoiceNo { get; set; } = string.Empty;
    public decimal AllocatedAmount { get; set; }
    public decimal InvoiceOutstandingAmount { get; set; }
}

public class PartnerPaymentResponse
{
    public int Id { get; set; }
    public PaymentDirection Direction { get; set; }
    public string PaymentNo { get; set; } = string.Empty;
    public int PartnerId { get; set; }
    public string PartnerName { get; set; } = string.Empty;
    public DateTime Date { get; set; }
    public int BankOrCashAccountId { get; set; }
    public string BankOrCashAccountName { get; set; } = string.Empty;
    public decimal TotalAmount { get; set; }
    public string? Narration { get; set; }
    public PartnerPaymentStatus Status { get; set; }
    public int? LinkedVoucherId { get; set; }
    public string? LinkedVoucherNo { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
    public DateTime CreatedAtUtc { get; set; }
    public string? ApprovedBy { get; set; }
    public DateTime? ApprovedAtUtc { get; set; }
    public List<PartnerPaymentAllocationResponse> Allocations { get; set; } = [];
}

public class PartnerPaymentListItemResponse
{
    public int Id { get; set; }
    public PaymentDirection Direction { get; set; }
    public string PaymentNo { get; set; } = string.Empty;
    public int PartnerId { get; set; }
    public string PartnerName { get; set; } = string.Empty;
    public DateTime Date { get; set; }
    public decimal TotalAmount { get; set; }
    public PartnerPaymentStatus Status { get; set; }
}
