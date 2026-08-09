using erp_backend.Models.Abstractions;

namespace erp_backend.Models;

public enum InvoiceType
{
    PurchaseOrder,
    PurchaseInvoice,
    PurchaseReturn,
    SalesOrder,
    SalesInvoice,
    SaleReturn,
}

public enum PaymentMode
{
    Cash,
    Credit,
}

public enum InvoicePaymentStatus
{
    Unpaid,
    PartiallyPaid,
    Paid,
    Overdue,
}

public enum FulfillmentStatus
{
    Open,
    PartiallyFulfilled,
    Fulfilled,
    Cancelled,
}

public enum InvoiceStatus
{
    Draft,
    PendingApproval,
    Posted,
    Rejected,
    Cancelled,
}

/// <summary>
/// One shared header for all 6 order/invoice-like documents (InvoiceType drives which fields are
/// meaningful): PaymentMode/AmountPaid/OutstandingAmount/PaymentStatus apply to the 4 invoice/return
/// types only; FulfillmentStatus applies to the 2 order types only. DueDate is deliberately
/// dual-purpose: payment due date for invoices/returns, requested/expected delivery date for orders.
/// ReferenceInvoiceId threads the whole chain: PurchaseOrder -> PurchaseInvoice -> PurchaseReturn,
/// and symmetrically for Sales; null for the two order types themselves.
/// </summary>
public class InvoiceHeader : IBranchScoped
{
    public int Id { get; set; }
    public int TenantId { get; set; }
    public int BranchId { get; set; }
    public InvoiceType InvoiceType { get; set; }
    public string InvoiceNo { get; set; } = string.Empty;
    public string? ExternalReferenceNo { get; set; }
    public int PartnerId { get; set; }
    public BusinessPartner? Partner { get; set; }
    public int? ReferenceInvoiceId { get; set; }
    public InvoiceHeader? ReferenceInvoice { get; set; }
    public int WarehouseId { get; set; }
    public Warehouse? Warehouse { get; set; }
    public DateTime Date { get; set; }
    public PaymentMode PaymentMode { get; set; }
    public int PaymentTermDays { get; set; }
    public DateTime DueDate { get; set; }
    public decimal AmountPaid { get; set; }
    public decimal OutstandingAmount { get; set; }
    public InvoicePaymentStatus PaymentStatus { get; set; } = InvoicePaymentStatus.Unpaid;
    public FulfillmentStatus? FulfillmentStatus { get; set; }
    public InvoiceStatus Status { get; set; } = InvoiceStatus.Draft;
    public string? Narration { get; set; }
    public int? LinkedVoucherId { get; set; }
    public VoucherHeader? LinkedVoucher { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    public string? ApprovedBy { get; set; }
    public DateTime? ApprovedAtUtc { get; set; }

    public List<InvoiceLine> Lines { get; set; } = [];
}
