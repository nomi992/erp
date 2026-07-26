using erp_backend.Models.Abstractions;

namespace erp_backend.Models;

/// <summary>
/// Keyed by ProductCategoryId (nullable = tenant-wide fallback row). "At most one null row per tenant"
/// is enforced in StockAccountMappingRepository, not a DB constraint (SQL Server treats NULLs as distinct).
/// </summary>
public class StockAccountMapping : ITenantScoped
{
    public int Id { get; set; }
    public int TenantId { get; set; }
    public int? ProductCategoryId { get; set; }
    public ProductCategory? ProductCategory { get; set; }

    public int InventoryAssetAccountId { get; set; }
    public Account? InventoryAssetAccount { get; set; }
    public int COGSAccountId { get; set; }
    public Account? COGSAccount { get; set; }
    public int AccountsPayableAccountId { get; set; }
    public Account? AccountsPayableAccount { get; set; }
    public int SalesRevenueAccountId { get; set; }
    public Account? SalesRevenueAccount { get; set; }
    public int AccountsReceivableAccountId { get; set; }
    public Account? AccountsReceivableAccount { get; set; }
    public int? CashOrBankAccountId { get; set; }
    public Account? CashOrBankAccount { get; set; }
    public int? InputTaxAccountId { get; set; }
    public Account? InputTaxAccount { get; set; }
    public int? OutputTaxAccountId { get; set; }
    public Account? OutputTaxAccount { get; set; }
    public int StockAdjustmentVarianceAccountId { get; set; }
    public Account? StockAdjustmentVarianceAccount { get; set; }

    // Cr side specifically for ReasonCode == OpeningBalance adjustments; kept separate from
    // StockAdjustmentVarianceAccountId because seeding pre-existing stock is an equity/balancing
    // entry, not a gain/loss/variance event.
    public int OpeningBalanceEquityAccountId { get; set; }
    public Account? OpeningBalanceEquityAccount { get; set; }

    public bool IsActive { get; set; } = true;
}
