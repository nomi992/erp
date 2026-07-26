using erp_backend.Models;

namespace erp_backend.StockAdjustments.Dtos;

public class StockAdjustmentLineRequest
{
    public int ProductVariantId { get; set; }
    public AdjustmentDirection Direction { get; set; }
    public decimal BaseQty { get; set; }

    // Required when Direction == Increase; ignored for Decrease (always values at current AverageCost).
    public decimal? UnitCost { get; set; }
}

public class StockAdjustmentRequest
{
    public int WarehouseId { get; set; }
    public DateTime Date { get; set; }
    public AdjustmentReasonCode ReasonCode { get; set; }
    public string? Narration { get; set; }
    public List<StockAdjustmentLineRequest> Lines { get; set; } = [];
}
