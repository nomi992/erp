using erp_backend.Data;
using erp_backend.Exceptions;
using erp_backend.Inventory.Dtos;
using erp_backend.Messages;
using erp_backend.Models;
using Microsoft.EntityFrameworkCore;

namespace erp_backend.Inventory;

public class StockAccountMappingRepository : IStockAccountMappingRepository
{
    private readonly AppDbContext _context;

    public StockAccountMappingRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<StockAccountMappingResponse>> GetAllAsync()
    {
        var mappings = await _context.StockAccountMappings.Include(m => m.ProductCategory).ToListAsync();
        return mappings.Select(StockAccountMappingResponse.FromEntity).ToList();
    }

    public async Task<StockAccountMappingResponse> GetByIdAsync(int id)
    {
        var mapping = await _context.StockAccountMappings.Include(m => m.ProductCategory).FirstOrDefaultAsync(m => m.Id == id)
            ?? throw new NotFoundException(ResponseMessage.StockAccountMappingNotFound);

        return StockAccountMappingResponse.FromEntity(mapping);
    }

    public async Task<StockAccountMappingResponse> CreateAsync(StockAccountMappingRequest request)
    {
        await ValidateUniquenessAsync(request.ProductCategoryId, existingId: null);
        await ValidateAccountsAsync(request);

        var mapping = MapToEntity(new StockAccountMapping(), request);
        _context.StockAccountMappings.Add(mapping);
        await _context.SaveChangesAsync();

        return await GetByIdAsync(mapping.Id);
    }

    public async Task<StockAccountMappingResponse> UpdateAsync(int id, StockAccountMappingRequest request)
    {
        var mapping = await _context.StockAccountMappings.FindAsync(id) ?? throw new NotFoundException(ResponseMessage.StockAccountMappingNotFound);

        await ValidateUniquenessAsync(request.ProductCategoryId, existingId: id);
        await ValidateAccountsAsync(request);

        MapToEntity(mapping, request);
        await _context.SaveChangesAsync();

        return await GetByIdAsync(id);
    }

    public async Task<StockAccountMapping> ResolveForCategoryAsync(int? productCategoryId)
    {
        if (productCategoryId is int categoryId)
        {
            var categoryMapping = await _context.StockAccountMappings
                .FirstOrDefaultAsync(m => m.IsActive && m.ProductCategoryId == categoryId);

            if (categoryMapping is not null)
            {
                return categoryMapping;
            }
        }

        var defaultMapping = await _context.StockAccountMappings.FirstOrDefaultAsync(m => m.IsActive && m.ProductCategoryId == null);
        return defaultMapping ?? throw new BadRequestException(ResponseMessage.StockAccountMappingNotConfigured);
    }

    private async Task ValidateUniquenessAsync(int? productCategoryId, int? existingId)
    {
        var conflict = productCategoryId is null
            ? await _context.StockAccountMappings.AnyAsync(m => m.ProductCategoryId == null && m.Id != (existingId ?? -1))
            : await _context.StockAccountMappings.AnyAsync(m => m.ProductCategoryId == productCategoryId && m.Id != (existingId ?? -1));

        if (conflict)
        {
            throw new BadRequestException(productCategoryId is null
                ? ResponseMessage.StockAccountMappingDefaultAlreadyExists
                : ResponseMessage.StockAccountMappingCategoryAlreadyExists);
        }
    }

    private async Task ValidateAccountsAsync(StockAccountMappingRequest request)
    {
        var requiredIds = new List<int>
        {
            request.InventoryAssetAccountId,
            request.COGSAccountId,
            request.AccountsPayableAccountId,
            request.SalesRevenueAccountId,
            request.AccountsReceivableAccountId,
            request.StockAdjustmentVarianceAccountId,
            request.OpeningBalanceEquityAccountId,
        };

        var optionalIds = new List<int>();
        if (request.CashOrBankAccountId is int cashId) optionalIds.Add(cashId);
        if (request.InputTaxAccountId is int inputTaxId) optionalIds.Add(inputTaxId);
        if (request.OutputTaxAccountId is int outputTaxId) optionalIds.Add(outputTaxId);

        var allIds = requiredIds.Concat(optionalIds).Distinct().ToList();
        var foundCount = await _context.Accounts.CountAsync(a => allIds.Contains(a.Id));

        if (foundCount != allIds.Count)
        {
            throw new BadRequestException(ResponseMessage.StockAccountMappingAccountNotFound);
        }

        if (request.ProductCategoryId is int categoryId && await _context.ProductCategories.FindAsync(categoryId) is null)
        {
            throw new BadRequestException(ResponseMessage.ProductCategoryNotFound);
        }
    }

    private static StockAccountMapping MapToEntity(StockAccountMapping mapping, StockAccountMappingRequest request)
    {
        mapping.ProductCategoryId = request.ProductCategoryId;
        mapping.InventoryAssetAccountId = request.InventoryAssetAccountId;
        mapping.COGSAccountId = request.COGSAccountId;
        mapping.AccountsPayableAccountId = request.AccountsPayableAccountId;
        mapping.SalesRevenueAccountId = request.SalesRevenueAccountId;
        mapping.AccountsReceivableAccountId = request.AccountsReceivableAccountId;
        mapping.CashOrBankAccountId = request.CashOrBankAccountId;
        mapping.InputTaxAccountId = request.InputTaxAccountId;
        mapping.OutputTaxAccountId = request.OutputTaxAccountId;
        mapping.StockAdjustmentVarianceAccountId = request.StockAdjustmentVarianceAccountId;
        mapping.OpeningBalanceEquityAccountId = request.OpeningBalanceEquityAccountId;
        return mapping;
    }
}
