using erp_backend.Data;
using erp_backend.Models;

namespace erp_backend.Tenants;

/// <summary>
/// Runs once, right after a Tenant row is inserted (see TenantRepository.CreateAsync). Deliberately
/// builds every Account/StockAccountMapping/BusinessPartner entity in-process and reads them back
/// from the in-memory graph rather than re-querying the DbSets or delegating to
/// IStockAccountMappingRepository/IPartnerLedgerAccountResolver: those all resolve tenant-scoped data
/// through AppDbContext's global query filters, which key off the *ambient* ICurrentTenantContext
/// (the caller's own tenant - e.g. the platform tenant for a SystemAdmin), not the brand-new tenant
/// being provisioned. Querying here would silently see zero rows. Building the graph directly and
/// stamping TenantId explicitly sidesteps that entirely (SaveChanges only auto-stamps TenantId when
/// it's still 0, so an explicit value is never overwritten - see AppDbContext.ApplyTenantBranchStamping).
/// </summary>
public class TenantDefaultsProvisioner : ITenantDefaultsProvisioner
{
    private const string WalkInCustomerCode = "WALKIN";

    private readonly AppDbContext _context;

    public TenantDefaultsProvisioner(AppDbContext context)
    {
        _context = context;
    }

    public async Task ProvisionAsync(Tenant tenant)
    {
        var assets = NewAccount(tenant.Id, "1000", "Assets", AccountType.Asset, AccountNature.Debit, parent: null, isControl: true);
        var liabilities = NewAccount(tenant.Id, "2000", "Liabilities", AccountType.Liability, AccountNature.Credit, parent: null, isControl: true);
        var equity = NewAccount(tenant.Id, "3000", "Equity", AccountType.Equity, AccountNature.Credit, parent: null, isControl: true);
        var income = NewAccount(tenant.Id, "4000", "Income", AccountType.Income, AccountNature.Credit, parent: null, isControl: true);
        var expense = NewAccount(tenant.Id, "5000", "Expenses", AccountType.Expense, AccountNature.Debit, parent: null, isControl: true);

        var receivable = NewAccount(tenant.Id, "1100", "Accounts Receivable", AccountType.Asset, AccountNature.Debit, assets, isControl: true);
        var inventory = NewAccount(tenant.Id, "1200", "Inventory", AccountType.Asset, AccountNature.Debit, assets, isControl: false);
        var cashAndBank = NewAccount(tenant.Id, "1300", "Cash and Bank", AccountType.Asset, AccountNature.Debit, assets, isControl: false);
        cashAndBank.IsCashAccount = true;
        var inputTax = NewAccount(tenant.Id, "1400", "Input Tax Recoverable", AccountType.Asset, AccountNature.Debit, assets, isControl: false);

        var payable = NewAccount(tenant.Id, "2100", "Accounts Payable", AccountType.Liability, AccountNature.Credit, liabilities, isControl: true);
        var outputTax = NewAccount(tenant.Id, "2200", "Output Tax Payable", AccountType.Liability, AccountNature.Credit, liabilities, isControl: false);

        var openingBalanceEquity = NewAccount(tenant.Id, "3100", "Opening Balance Equity", AccountType.Equity, AccountNature.Credit, equity, isControl: false);

        var salesRevenue = NewAccount(tenant.Id, "4100", "Sales Revenue", AccountType.Income, AccountNature.Credit, income, isControl: false);

        var cogs = NewAccount(tenant.Id, "5100", "Cost of Goods Sold", AccountType.Expense, AccountNature.Debit, expense, isControl: false);
        var stockVariance = NewAccount(tenant.Id, "5200", "Stock Adjustment / Variance", AccountType.Expense, AccountNature.Debit, expense, isControl: false);

        var allAccounts = new[]
        {
            assets, liabilities, equity, income, expense,
            receivable, inventory, cashAndBank, inputTax,
            payable, outputTax,
            openingBalanceEquity,
            salesRevenue,
            cogs, stockVariance,
        };

        _context.Accounts.AddRange(allAccounts);
        await _context.SaveChangesAsync();

        var mapping = new StockAccountMapping
        {
            TenantId = tenant.Id,
            ProductCategoryId = null,
            InventoryAssetAccountId = inventory.Id,
            COGSAccountId = cogs.Id,
            AccountsPayableAccountId = payable.Id,
            SalesRevenueAccountId = salesRevenue.Id,
            AccountsReceivableAccountId = receivable.Id,
            CashOrBankAccountId = cashAndBank.Id,
            InputTaxAccountId = inputTax.Id,
            OutputTaxAccountId = outputTax.Id,
            StockAdjustmentVarianceAccountId = stockVariance.Id,
            OpeningBalanceEquityAccountId = openingBalanceEquity.Id,
            IsActive = true,
        };
        _context.StockAccountMappings.Add(mapping);

        var walkInCustomer = new BusinessPartner
        {
            TenantId = tenant.Id,
            PartnerType = PartnerType.Customer,
            Code = WalkInCustomerCode,
            Name = "Walk-in Customer",
            IsActive = true,
            IsDefault = true,
        };

        // Mirrors PartnerLedgerAccountResolver.BuildSubAccount - the sub-ledger account under the
        // tenant's new Accounts Receivable control account, built directly here for the reason
        // explained on the type.
        walkInCustomer.ReceivableAccount = new Account
        {
            TenantId = tenant.Id,
            Code = $"{receivable.Code}-{walkInCustomer.Code}",
            Name = walkInCustomer.Name,
            Type = receivable.Type,
            Nature = receivable.Nature,
            ParentAccountId = receivable.Id,
            IsControlAccount = false,
        };

        _context.BusinessPartners.Add(walkInCustomer);
        await _context.SaveChangesAsync();
    }

    private static Account NewAccount(
        int tenantId,
        string code,
        string name,
        AccountType type,
        AccountNature nature,
        Account? parent,
        bool isControl) => new()
    {
        TenantId = tenantId,
        Code = code,
        Name = name,
        Type = type,
        Nature = nature,
        // Set via navigation, not ParentAccountId directly: the parent (for non-root accounts) is
        // still unsaved at this point, so its Id is 0 - EF Core fixes up the real FK from this
        // navigation once both ends are tracked and SaveChanges assigns the parent's real Id.
        ParentAccount = parent,
        IsControlAccount = isControl,
    };
}
