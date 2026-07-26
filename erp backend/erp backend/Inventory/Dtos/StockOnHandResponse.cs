namespace erp_backend.Inventory.Dtos;

public class StockOnHandResponse
{
    public int ProductVariantId { get; set; }
    public string ProductVariantName { get; set; } = string.Empty;
    public string ProductName { get; set; } = string.Empty;
    public int WarehouseId { get; set; }
    public string WarehouseName { get; set; } = string.Empty;
    public decimal QuantityOnHand { get; set; }
    public decimal AverageCost { get; set; }
    public decimal StockValue { get; set; }
    public decimal? ReorderLevel { get; set; }
    public bool IsLowStock { get; set; }
    public DateTime? LastMovementAtUtc { get; set; }
}
