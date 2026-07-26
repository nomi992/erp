using erp_backend.Models;

namespace erp_backend.PartnerPayments.Dtos;

public class PartnerPaymentAllocationRequest
{
    public int InvoiceHeaderId { get; set; }
    public decimal AllocatedAmount { get; set; }
}

public class PartnerPaymentRequest
{
    public PaymentDirection Direction { get; set; }
    public int PartnerId { get; set; }
    public DateTime Date { get; set; }
    public int BankOrCashAccountId { get; set; }
    public decimal TotalAmount { get; set; }
    public string? Narration { get; set; }
    public List<PartnerPaymentAllocationRequest> Allocations { get; set; } = [];
}
