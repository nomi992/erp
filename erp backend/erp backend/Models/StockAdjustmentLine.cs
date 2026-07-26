using erp_backend.Models.Abstractions;

namespace erp_backend.Models;

public enum AdjustmentDirection
{
    Increase,
    Decrease,
}

public class StockAdjustmentLine : IBranchScoped
{
    public int Id { get; set; }
    public int TenantId { get; set; }
    public int BranchId { get; set; }
    public int StockAdjustmentHeaderId { get; set; }
    public StockAdjustmentHeader? StockAdjustmentHeader { get; set; }
    public int ProductVariantId { get; set; }
    public ProductVariant? ProductVariant { get; set; }
    public AdjustmentDirection Direction { get; set; }
    public decimal BaseQty { get; set; }

    // Required for Increase; Decrease always values at the current AverageCost, captured here at
    // Approve time for the record.
    public decimal? UnitCost { get; set; }

    public decimal LineValue { get; set; }
}
