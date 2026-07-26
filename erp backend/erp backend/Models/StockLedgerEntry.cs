using erp_backend.Models.Abstractions;

namespace erp_backend.Models;

public enum MovementType
{
    PurchaseReceipt,
    PurchaseReturnIssue,
    SaleIssue,
    SaleReturnReceipt,
    TransferOut,
    TransferIn,
    AdjustmentIncrease,
    AdjustmentDecrease,
    OpeningBalance,
}

public enum SourceDocumentType
{
    Invoice,
    StockTransfer,
    StockAdjustment,
}

/// <summary>
/// Append-only audit trail of every stock movement. Never UPDATE/DELETE — corrections post new
/// offsetting rows, the same immutability principle already applied to posted Vouchers.
/// SourceDocumentId is a deliberate unenforced polymorphic reference (no DB FK): acceptable since
/// documents are never hard-deleted, only cancelled.
/// </summary>
public class StockLedgerEntry : IBranchScoped
{
    public int Id { get; set; }
    public int TenantId { get; set; }
    public int BranchId { get; set; }
    public int ProductVariantId { get; set; }
    public ProductVariant? ProductVariant { get; set; }
    public int WarehouseId { get; set; }
    public Warehouse? Warehouse { get; set; }
    public DateTime TransactionDate { get; set; }
    public MovementType MovementType { get; set; }
    public decimal QuantityIn { get; set; }
    public decimal QuantityOut { get; set; }
    public decimal UnitCost { get; set; }
    public decimal TotalCostSigned { get; set; }
    public decimal RunningQuantity { get; set; }
    public decimal RunningValue { get; set; }
    public SourceDocumentType SourceDocumentType { get; set; }
    public int SourceDocumentId { get; set; }
    public int? SourceDocumentLineId { get; set; }
    public string? Narration { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
}
