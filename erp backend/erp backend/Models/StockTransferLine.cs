using erp_backend.Models.Abstractions;

namespace erp_backend.Models;

public class StockTransferLine : IBranchScoped
{
    public int Id { get; set; }
    public int TenantId { get; set; }
    public int BranchId { get; set; }
    public int StockTransferHeaderId { get; set; }
    public StockTransferHeader? StockTransferHeader { get; set; }
    public int ProductVariantId { get; set; }
    public ProductVariant? ProductVariant { get; set; }
    public int UnitOfMeasureId { get; set; }
    public UnitOfMeasure? UnitOfMeasure { get; set; }
    public decimal Qty { get; set; }
    public decimal BaseQty { get; set; }

    // Captured from the source warehouse's AverageCost at Approve time.
    public decimal UnitCostAtTransfer { get; set; }

    // Supports partial receipt on the cross-branch dual-step flow.
    public decimal ReceivedBaseQty { get; set; }
}
