using erp_backend.Models;

namespace erp_backend.Invoices.Dtos;

public class InvoiceLineRequest
{
    public int ProductVariantId { get; set; }
    public int UnitOfMeasureId { get; set; }
    public decimal Qty { get; set; }
    public decimal UnitAmount { get; set; }
    public int? TaxRateId { get; set; }

    // Set only when this line is raised against a prior order line (PurchaseInvoice/SalesInvoice) or
    // is returning a prior invoice line (PurchaseReturn/SaleReturn).
    public int? ReferenceInvoiceLineId { get; set; }
}

public class InvoiceRequest
{
    public InvoiceType InvoiceType { get; set; }
    public string? ExternalReferenceNo { get; set; }
    public int PartnerId { get; set; }

    // Null for the two order types. Optional for PurchaseInvoice/SalesInvoice (set only when raised
    // against a prior order). Required for the two return types.
    public int? ReferenceInvoiceId { get; set; }

    public int WarehouseId { get; set; }
    public DateTime Date { get; set; }
    public PaymentMode PaymentMode { get; set; }
    public int PaymentTermDays { get; set; }

    // Requested/expected delivery date for the two order types. Ignored for the four invoice/return
    // types — the repository always computes DueDate = Date.AddDays(PaymentTermDays) for those.
    public DateTime? RequestedDeliveryDate { get; set; }

    public string? Narration { get; set; }
    public List<InvoiceLineRequest> Lines { get; set; } = [];
}
