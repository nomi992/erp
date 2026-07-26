using erp_backend.Models;

namespace erp_backend.Invoices.Dtos;

public class InvoiceLineResponse
{
    public int Id { get; set; }
    public int ProductVariantId { get; set; }
    public string ProductVariantName { get; set; } = string.Empty;
    public string ProductName { get; set; } = string.Empty;
    public int UnitOfMeasureId { get; set; }
    public string UnitOfMeasureCode { get; set; } = string.Empty;
    public decimal Qty { get; set; }
    public decimal BaseQty { get; set; }
    public decimal UnitAmount { get; set; }
    public decimal? UnitCostAtSale { get; set; }
    public int? TaxRateId { get; set; }
    public string? TaxRateName { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal LineTotal { get; set; }
    public decimal FulfilledBaseQty { get; set; }
    public int? ReferenceInvoiceLineId { get; set; }
}

public class InvoiceResponse
{
    public int Id { get; set; }
    public InvoiceType InvoiceType { get; set; }
    public string InvoiceNo { get; set; } = string.Empty;
    public string? ExternalReferenceNo { get; set; }
    public int PartnerId { get; set; }
    public string PartnerName { get; set; } = string.Empty;
    public int? ReferenceInvoiceId { get; set; }
    public string? ReferenceInvoiceNo { get; set; }
    public int WarehouseId { get; set; }
    public string WarehouseName { get; set; } = string.Empty;
    public DateTime Date { get; set; }
    public PaymentMode PaymentMode { get; set; }
    public int PaymentTermDays { get; set; }
    public DateTime DueDate { get; set; }
    public decimal AmountPaid { get; set; }
    public decimal OutstandingAmount { get; set; }
    public InvoicePaymentStatus PaymentStatus { get; set; }
    public FulfillmentStatus? FulfillmentStatus { get; set; }
    public InvoiceStatus Status { get; set; }
    public string? Narration { get; set; }
    public int? LinkedVoucherId { get; set; }
    public string? LinkedVoucherNo { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
    public DateTime CreatedAtUtc { get; set; }
    public string? ApprovedBy { get; set; }
    public DateTime? ApprovedAtUtc { get; set; }
    public decimal TotalNet { get; set; }
    public decimal TotalTax { get; set; }
    public decimal TotalAmount { get; set; }
    public List<InvoiceLineResponse> Lines { get; set; } = [];
}

public class InvoiceListItemResponse
{
    public int Id { get; set; }
    public InvoiceType InvoiceType { get; set; }
    public string InvoiceNo { get; set; } = string.Empty;
    public int PartnerId { get; set; }
    public string PartnerName { get; set; } = string.Empty;
    public DateTime Date { get; set; }
    public DateTime DueDate { get; set; }
    public decimal OutstandingAmount { get; set; }
    public InvoicePaymentStatus PaymentStatus { get; set; }
    public FulfillmentStatus? FulfillmentStatus { get; set; }
    public InvoiceStatus Status { get; set; }
    public decimal TotalAmount { get; set; }
}
