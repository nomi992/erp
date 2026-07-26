namespace erp_backend.Inventory.Dtos;

public class StockAccountMappingRequest
{
    public int? ProductCategoryId { get; set; }
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
}
