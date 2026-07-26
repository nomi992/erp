using erp_backend.Inventory.Dtos;
using erp_backend.Models;

namespace erp_backend.Inventory;

public interface IStockAccountMappingRepository
{
    Task<List<StockAccountMappingResponse>> GetAllAsync();
    Task<StockAccountMappingResponse> GetByIdAsync(int id);
    Task<StockAccountMappingResponse> CreateAsync(StockAccountMappingRequest request);
    Task<StockAccountMappingResponse> UpdateAsync(int id, StockAccountMappingRequest request);

    /// <summary>
    /// Resolves the applicable mapping for a product's category, falling back to the tenant-wide
    /// default (ProductCategoryId == null) row. Used internally by Invoice/StockAdjustment posting.
    /// </summary>
    Task<StockAccountMapping> ResolveForCategoryAsync(int? productCategoryId);
}
