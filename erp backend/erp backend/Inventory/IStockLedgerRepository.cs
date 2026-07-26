using erp_backend.Inventory.Dtos;
using erp_backend.Models;

namespace erp_backend.Inventory;

/// <summary>
/// Read-only reporting queries over the inventory/AR/AP data other repositories maintain — ledger
/// inquiry, current on-hand, and AR/AP aging. Not gated by any right, consistent with every other
/// read (GET) endpoint in this codebase.
/// </summary>
public interface IStockLedgerRepository
{
    Task<PagedResult<StockLedgerEntryResponse>> GetLedgerAsync(
        int? productVariantId, int? warehouseId, DateTime? from, DateTime? to, int pageNumber, int pageSize);

    Task<List<StockOnHandResponse>> GetOnHandAsync(int? warehouseId, int? productVariantId, bool lowStockOnly);

    Task<List<PartnerAgingResponse>> GetAccountsReceivableAgingAsync();

    Task<List<PartnerAgingResponse>> GetAccountsPayableAgingAsync();
}
