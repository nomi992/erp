using erp_backend.Models;

namespace erp_backend.Inventory.Dtos;

public class StockLedgerEntryResponse
{
    public int Id { get; set; }
    public int ProductVariantId { get; set; }
    public string ProductVariantName { get; set; } = string.Empty;
    public string ProductName { get; set; } = string.Empty;
    public int WarehouseId { get; set; }
    public string WarehouseName { get; set; } = string.Empty;
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
    public string? Narration { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
    public DateTime CreatedAtUtc { get; set; }
}
