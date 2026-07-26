using erp_backend.Models;

namespace erp_backend.StockAdjustments.Dtos;

public class StockAdjustmentLineResponse
{
    public int Id { get; set; }
    public int ProductVariantId { get; set; }
    public string ProductVariantName { get; set; } = string.Empty;
    public string ProductName { get; set; } = string.Empty;
    public AdjustmentDirection Direction { get; set; }
    public decimal BaseQty { get; set; }
    public decimal? UnitCost { get; set; }
    public decimal LineValue { get; set; }
}

public class StockAdjustmentResponse
{
    public int Id { get; set; }
    public string AdjustmentNo { get; set; } = string.Empty;
    public int WarehouseId { get; set; }
    public string WarehouseName { get; set; } = string.Empty;
    public DateTime Date { get; set; }
    public AdjustmentReasonCode ReasonCode { get; set; }
    public StockAdjustmentStatus Status { get; set; }
    public string? Narration { get; set; }
    public int? LinkedVoucherId { get; set; }
    public string? LinkedVoucherNo { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
    public DateTime CreatedAtUtc { get; set; }
    public string? ApprovedBy { get; set; }
    public DateTime? ApprovedAtUtc { get; set; }
    public List<StockAdjustmentLineResponse> Lines { get; set; } = [];
}

public class StockAdjustmentListItemResponse
{
    public int Id { get; set; }
    public string AdjustmentNo { get; set; } = string.Empty;
    public string WarehouseName { get; set; } = string.Empty;
    public DateTime Date { get; set; }
    public AdjustmentReasonCode ReasonCode { get; set; }
    public StockAdjustmentStatus Status { get; set; }
}
