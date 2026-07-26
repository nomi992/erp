using erp_backend.Models;

namespace erp_backend.Inventory.Dtos;

public class StockAccountMappingResponse
{
    public int Id { get; set; }
    public int? ProductCategoryId { get; set; }
    public string? ProductCategoryName { get; set; }
    public int InventoryAssetAccountId { get; set; }
    public int COGSAccountId { get; set; }
    public int AccountsPayableAccountId { get; set; }
    public int SalesRevenueAccountId { get; set; }
    public int AccountsReceivableAccountId { get; set; }
    public int? CashOrBankAccountId { get; set; }
    public int? InputTaxAccountId { get; set; }
    public int? OutputTaxAccountId { get; set; }
    public int StockAdjustmentVarianceAccountId { get; set; }
    public int OpeningBalanceEquityAccountId { get; set; }
    public bool IsActive { get; set; }

    public static StockAccountMappingResponse FromEntity(StockAccountMapping entity) =>
        new()
        {
            Id = entity.Id,
            ProductCategoryId = entity.ProductCategoryId,
            ProductCategoryName = entity.ProductCategory?.Name,
            InventoryAssetAccountId = entity.InventoryAssetAccountId,
            COGSAccountId = entity.COGSAccountId,
            AccountsPayableAccountId = entity.AccountsPayableAccountId,
            SalesRevenueAccountId = entity.SalesRevenueAccountId,
            AccountsReceivableAccountId = entity.AccountsReceivableAccountId,
            CashOrBankAccountId = entity.CashOrBankAccountId,
            InputTaxAccountId = entity.InputTaxAccountId,
            OutputTaxAccountId = entity.OutputTaxAccountId,
            StockAdjustmentVarianceAccountId = entity.StockAdjustmentVarianceAccountId,
            OpeningBalanceEquityAccountId = entity.OpeningBalanceEquityAccountId,
            IsActive = entity.IsActive,
        };
}
