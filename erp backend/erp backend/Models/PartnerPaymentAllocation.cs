using erp_backend.Models.Abstractions;

namespace erp_backend.Models;

public class PartnerPaymentAllocation : IBranchScoped
{
    public int Id { get; set; }
    public int TenantId { get; set; }
    public int BranchId { get; set; }
    public int PartnerPaymentHeaderId { get; set; }
    public PartnerPaymentHeader? PartnerPaymentHeader { get; set; }
    public int InvoiceHeaderId { get; set; }
    public InvoiceHeader? InvoiceHeader { get; set; }
    public decimal AllocatedAmount { get; set; }
}
