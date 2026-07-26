using erp_backend.Data;
using erp_backend.Inventory.Dtos;
using erp_backend.Models;
using Microsoft.EntityFrameworkCore;

namespace erp_backend.Inventory;

public class StockLedgerRepository : IStockLedgerRepository
{
    private readonly AppDbContext _context;

    public StockLedgerRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<PagedResult<StockLedgerEntryResponse>> GetLedgerAsync(
        int? productVariantId, int? warehouseId, DateTime? from, DateTime? to, int pageNumber, int pageSize)
    {
        var query = _context.StockLedgerEntries
            .Include(e => e.ProductVariant).ThenInclude(v => v!.Product)
            .Include(e => e.Warehouse)
            .AsQueryable();

        if (productVariantId is not null) query = query.Where(e => e.ProductVariantId == productVariantId);
        if (warehouseId is not null) query = query.Where(e => e.WarehouseId == warehouseId);
        if (from is not null) query = query.Where(e => e.TransactionDate >= from);
        if (to is not null) query = query.Where(e => e.TransactionDate <= to);

        query = query.OrderByDescending(e => e.TransactionDate).ThenByDescending(e => e.Id);

        var totalCount = await query.CountAsync();
        var items = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Select(e => new StockLedgerEntryResponse
            {
                Id = e.Id,
                ProductVariantId = e.ProductVariantId,
                ProductVariantName = e.ProductVariant!.Name,
                ProductName = e.ProductVariant!.Product!.Name,
                WarehouseId = e.WarehouseId,
                WarehouseName = e.Warehouse!.Name,
                TransactionDate = e.TransactionDate,
                MovementType = e.MovementType,
                QuantityIn = e.QuantityIn,
                QuantityOut = e.QuantityOut,
                UnitCost = e.UnitCost,
                TotalCostSigned = e.TotalCostSigned,
                RunningQuantity = e.RunningQuantity,
                RunningValue = e.RunningValue,
                SourceDocumentType = e.SourceDocumentType,
                SourceDocumentId = e.SourceDocumentId,
                Narration = e.Narration,
                CreatedBy = e.CreatedBy,
                CreatedAtUtc = e.CreatedAtUtc,
            })
            .AsNoTracking()
            .ToListAsync();

        return new PagedResult<StockLedgerEntryResponse> { Items = items, TotalCount = totalCount, PageNumber = pageNumber, PageSize = pageSize };
    }

    public async Task<List<StockOnHandResponse>> GetOnHandAsync(int? warehouseId, int? productVariantId, bool lowStockOnly)
    {
        var query = _context.StockBalances
            .Include(b => b.ProductVariant).ThenInclude(v => v!.Product)
            .Include(b => b.Warehouse)
            .AsQueryable();

        if (warehouseId is not null) query = query.Where(b => b.WarehouseId == warehouseId);
        if (productVariantId is not null) query = query.Where(b => b.ProductVariantId == productVariantId);

        var balances = await query.AsNoTracking().ToListAsync();

        var results = balances.Select(b => new StockOnHandResponse
        {
            ProductVariantId = b.ProductVariantId,
            ProductVariantName = b.ProductVariant?.Name ?? string.Empty,
            ProductName = b.ProductVariant?.Product?.Name ?? string.Empty,
            WarehouseId = b.WarehouseId,
            WarehouseName = b.Warehouse?.Name ?? string.Empty,
            QuantityOnHand = b.QuantityOnHand,
            AverageCost = b.AverageCost,
            StockValue = Math.Round(b.QuantityOnHand * b.AverageCost, 2),
            ReorderLevel = b.ReorderLevel,
            IsLowStock = b.ReorderLevel is decimal reorderLevel && b.QuantityOnHand <= reorderLevel,
            LastMovementAtUtc = b.LastMovementAtUtc,
        });

        if (lowStockOnly)
        {
            results = results.Where(r => r.IsLowStock);
        }

        return results.OrderBy(r => r.ProductName).ThenBy(r => r.WarehouseName).ToList();
    }

    public Task<List<PartnerAgingResponse>> GetAccountsReceivableAgingAsync() =>
        GetAgingAsync(InvoiceType.SalesInvoice, InvoiceType.SaleReturn);

    public Task<List<PartnerAgingResponse>> GetAccountsPayableAgingAsync() =>
        GetAgingAsync(InvoiceType.PurchaseInvoice, InvoiceType.PurchaseReturn);

    private async Task<List<PartnerAgingResponse>> GetAgingAsync(InvoiceType primaryType, InvoiceType returnType)
    {
        var today = DateTime.UtcNow.Date;

        var outstanding = await _context.InvoiceHeaders
            .Include(h => h.Partner)
            .Where(h => h.Status == InvoiceStatus.Posted
                && (h.InvoiceType == primaryType || h.InvoiceType == returnType)
                && h.OutstandingAmount > 0)
            .Select(h => new { h.PartnerId, PartnerName = h.Partner!.Name, h.DueDate, h.OutstandingAmount })
            .AsNoTracking()
            .ToListAsync();

        return outstanding
            .GroupBy(h => new { h.PartnerId, h.PartnerName })
            .Select(g =>
            {
                var response = new PartnerAgingResponse { PartnerId = g.Key.PartnerId, PartnerName = g.Key.PartnerName };

                foreach (var invoice in g)
                {
                    var daysPastDue = (today - invoice.DueDate.Date).Days;

                    if (daysPastDue <= 0) response.Current += invoice.OutstandingAmount;
                    else if (daysPastDue <= 30) response.Days1To30 += invoice.OutstandingAmount;
                    else if (daysPastDue <= 60) response.Days31To60 += invoice.OutstandingAmount;
                    else if (daysPastDue <= 90) response.Days61To90 += invoice.OutstandingAmount;
                    else response.DaysOver90 += invoice.OutstandingAmount;

                    response.Total += invoice.OutstandingAmount;
                }

                return response;
            })
            .OrderByDescending(r => r.Total)
            .ToList();
    }
}
