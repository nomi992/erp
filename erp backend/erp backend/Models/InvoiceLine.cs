using erp_backend.Models.Abstractions;

namespace erp_backend.Models;

public class InvoiceLine : IBranchScoped
{
    public int Id { get; set; }
    public int TenantId { get; set; }
    public int BranchId { get; set; }
    public int InvoiceHeaderId { get; set; }
    public InvoiceHeader? InvoiceHeader { get; set; }
    public int ProductVariantId { get; set; }
    public ProductVariant? ProductVariant { get; set; }
    public int UnitOfMeasureId { get; set; }
    public UnitOfMeasure? UnitOfMeasure { get; set; }
    public decimal Qty { get; set; }
    public decimal BaseQty { get; set; }

    // Unit cost for purchase-side types, unit price for sale-side types; also serves as an Order's
    // agreed price and an Invoice's actual price.
    public decimal UnitAmount { get; set; }

    // Populated only for SalesInvoice/SaleReturn lines — the COGS basis captured at posting time.
    public decimal? UnitCostAtSale { get; set; }

    public int? TaxRateId { get; set; }
    public TaxRate? TaxRate { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal LineTotal { get; set; }

    // Meaningful only on Order-type lines: how much of this ordered quantity has been converted into
    // a downstream Invoice-type line so far.
    public decimal FulfilledBaseQty { get; set; }

    public int? ReferenceInvoiceLineId { get; set; }
    public InvoiceLine? ReferenceInvoiceLine { get; set; }
}
