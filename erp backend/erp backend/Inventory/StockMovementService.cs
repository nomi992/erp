using erp_backend.Data;
using erp_backend.Exceptions;
using erp_backend.Messages;
using erp_backend.Models;
using Microsoft.EntityFrameworkCore;

namespace erp_backend.Inventory;

public class StockMovementService : IStockMovementService
{
    private readonly AppDbContext _context;

    public StockMovementService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<StockMovementResult> ReceiveAsync(
        int productVariantId, int warehouseId, decimal baseQty, decimal unitCost, DateTime date,
        MovementType movementType, SourceDocumentType sourceType, int sourceDocumentId, int? sourceDocumentLineId,
        string? narration, string username)
    {
        var variant = await _context.ProductVariants.Include(v => v.Product).FirstOrDefaultAsync(v => v.Id == productVariantId)
            ?? throw new BadRequestException(ResponseMessage.StockMovementVariantNotFound);

        if (await _context.Warehouses.FindAsync(warehouseId) is null)
        {
            throw new BadRequestException(ResponseMessage.StockMovementWarehouseNotFound);
        }

        var balance = await GetOrCreateBalanceAsync(productVariantId, warehouseId);
        balance.ReorderLevel = variant.Product!.ReorderLevel;

        var newQuantity = balance.QuantityOnHand + baseQty;
        var newAverageCost = newQuantity == 0
            ? 0
            : (balance.QuantityOnHand * balance.AverageCost + baseQty * unitCost) / newQuantity;

        var entry = new StockLedgerEntry
        {
            ProductVariantId = productVariantId,
            WarehouseId = warehouseId,
            TransactionDate = date,
            MovementType = movementType,
            QuantityIn = baseQty,
            QuantityOut = 0,
            UnitCost = unitCost,
            TotalCostSigned = baseQty * unitCost,
            RunningQuantity = newQuantity,
            RunningValue = newQuantity * newAverageCost,
            SourceDocumentType = sourceType,
            SourceDocumentId = sourceDocumentId,
            SourceDocumentLineId = sourceDocumentLineId,
            Narration = narration,
            CreatedBy = username,
        };

        _context.StockLedgerEntries.Add(entry);

        balance.QuantityOnHand = newQuantity;
        balance.AverageCost = newAverageCost;
        balance.LastMovementAtUtc = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return new StockMovementResult
        {
            StockLedgerEntryId = entry.Id,
            NewQuantityOnHand = newQuantity,
            NewAverageCost = newAverageCost,
            MovementValue = baseQty * unitCost,
        };
    }

    public async Task<StockMovementResult> IssueAsync(
        int productVariantId, int warehouseId, decimal baseQty, DateTime date,
        MovementType movementType, SourceDocumentType sourceType, int sourceDocumentId, int? sourceDocumentLineId,
        string? narration, string username, bool allowNegativeStock = false)
    {
        var variant = await _context.ProductVariants.Include(v => v.Product).FirstOrDefaultAsync(v => v.Id == productVariantId)
            ?? throw new BadRequestException(ResponseMessage.StockMovementVariantNotFound);

        if (await _context.Warehouses.FindAsync(warehouseId) is null)
        {
            throw new BadRequestException(ResponseMessage.StockMovementWarehouseNotFound);
        }

        var balance = await GetOrCreateBalanceAsync(productVariantId, warehouseId);
        balance.ReorderLevel = variant.Product!.ReorderLevel;

        if (balance.QuantityOnHand < baseQty && !allowNegativeStock)
        {
            throw new BadRequestException(ResponseMessage.InsufficientStock, balance.QuantityOnHand);
        }

        var unitCost = balance.AverageCost;
        var newQuantity = balance.QuantityOnHand - baseQty;

        var entry = new StockLedgerEntry
        {
            ProductVariantId = productVariantId,
            WarehouseId = warehouseId,
            TransactionDate = date,
            MovementType = movementType,
            QuantityIn = 0,
            QuantityOut = baseQty,
            UnitCost = unitCost,
            TotalCostSigned = -(baseQty * unitCost),
            RunningQuantity = newQuantity,
            RunningValue = newQuantity * unitCost,
            SourceDocumentType = sourceType,
            SourceDocumentId = sourceDocumentId,
            SourceDocumentLineId = sourceDocumentLineId,
            Narration = narration,
            CreatedBy = username,
        };

        _context.StockLedgerEntries.Add(entry);

        balance.QuantityOnHand = newQuantity;
        // AverageCost is deliberately left unchanged on issue — moving-average costing only re-bases
        // on receipt, an issue draws down quantity at the cost basis already on record.
        balance.LastMovementAtUtc = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return new StockMovementResult
        {
            StockLedgerEntryId = entry.Id,
            NewQuantityOnHand = newQuantity,
            NewAverageCost = unitCost,
            MovementValue = baseQty * unitCost,
        };
    }

    private async Task<StockBalance> GetOrCreateBalanceAsync(int productVariantId, int warehouseId)
    {
        var balance = await _context.StockBalances
            .FirstOrDefaultAsync(b => b.ProductVariantId == productVariantId && b.WarehouseId == warehouseId);

        if (balance is not null)
        {
            return balance;
        }

        balance = new StockBalance
        {
            ProductVariantId = productVariantId,
            WarehouseId = warehouseId,
            QuantityOnHand = 0,
            AverageCost = 0,
        };

        _context.StockBalances.Add(balance);
        return balance;
    }
}
