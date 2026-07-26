using erp_backend.Models;

namespace erp_backend.Inventory;

public class StockMovementResult
{
    public int StockLedgerEntryId { get; set; }
    public decimal NewQuantityOnHand { get; set; }
    public decimal NewAverageCost { get; set; }
    public decimal MovementValue { get; set; }
}

/// <summary>
/// The shared moving-average costing engine every stock-affecting document repository calls into,
/// instead of duplicating the math. Does not open its own transaction — the calling document
/// repository owns the transaction boundary.
/// </summary>
public interface IStockMovementService
{
    Task<StockMovementResult> ReceiveAsync(
        int productVariantId, int warehouseId, decimal baseQty, decimal unitCost, DateTime date,
        MovementType movementType, SourceDocumentType sourceType, int sourceDocumentId, int? sourceDocumentLineId,
        string? narration, string username);

    Task<StockMovementResult> IssueAsync(
        int productVariantId, int warehouseId, decimal baseQty, DateTime date,
        MovementType movementType, SourceDocumentType sourceType, int sourceDocumentId, int? sourceDocumentLineId,
        string? narration, string username, bool allowNegativeStock = false);
}
