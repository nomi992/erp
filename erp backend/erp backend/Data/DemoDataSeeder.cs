using erp_backend.Accounts;
using erp_backend.Accounts.Dtos;
using erp_backend.Auth;
using erp_backend.CostCenters;
using erp_backend.CostCenters.Dtos;
using erp_backend.Inventory;
using erp_backend.Inventory.Dtos;
using erp_backend.Invoices;
using erp_backend.Invoices.Dtos;
using erp_backend.Models;
using erp_backend.Partners;
using erp_backend.Partners.Dtos;
using erp_backend.ProductCategories;
using erp_backend.ProductCategories.Dtos;
using erp_backend.Products;
using erp_backend.Products.Dtos;
using erp_backend.StockAdjustments;
using erp_backend.StockAdjustments.Dtos;
using erp_backend.TaxRates;
using erp_backend.TaxRates.Dtos;
using erp_backend.Transfers;
using erp_backend.Transfers.Dtos;
using erp_backend.UnitsOfMeasure;
using erp_backend.UnitsOfMeasure.Dtos;
using erp_backend.Vouchers;
using erp_backend.Vouchers.Dtos;
using erp_backend.Warehouses;
using erp_backend.Warehouses.Dtos;
using Microsoft.EntityFrameworkCore;

namespace erp_backend.Data;

/// <summary>
/// Dev/test-only demo dataset: a basic Chart of Accounts, a handful of Cost Centers/Tax Rates, a
/// paint-distribution product catalog (six companies x six paint categories x six colors = 216+
/// products, each with Quarter/Gallon/Drum variants), two warehouses, and opening stock posted to
/// both. Runs through the same repositories/services the API uses (not raw table inserts), so every
/// business rule (code uniqueness, moving-average costing, auto-posted opening-balance vouchers,
/// ledger-account provisioning) fires exactly as it would from a real request.
///
/// Triggered explicitly (via `--seed-demo` or SEED_DEMO_DATA=true), never on every startup, because
/// generating ~1300 stock-ledger postings through the full repository/voucher-posting pipeline takes
/// real time. Coarse-idempotent: re-running skips the whole product/stock section once >= 200
/// products already exist, and every earlier section (accounts/categories/UOM/etc.) checks
/// individually before creating, so partial re-runs after a failure don't duplicate anything.
/// </summary>
public static class DemoDataSeeder
{
    private const string SeedActor = "system-seed";
    // Plain calendar dates (Kind=Unspecified) — the columns they land in are "timestamp without time
    // zone" precisely so Kind doesn't matter, but Npgsql actively rejects Kind=Utc there, so these
    // must NOT be tagged DateTimeKind.Utc despite representing UTC-normalized business dates.
    private static readonly DateTime OpeningDate = new(2026, 1, 1);
    private static readonly DateTime PurchaseInvoiceDate = new(2026, 2, 1);
    private static readonly DateTime SalesInvoiceDate = new(2026, 3, 1);
    private static readonly DateTime ReturnDate = new(2026, 4, 1);
    private static readonly DateTime TransferDate = new(2026, 5, 1);

    public static async Task SeedAsync(IServiceProvider services, int tenantId, int branchId)
    {
        var tenantContext = services.GetRequiredService<ICurrentTenantContext>();
        tenantContext.SetTenant(tenantId);
        tenantContext.SetBranch(branchId);

        var db = services.GetRequiredService<AppDbContext>();
        var accounts = services.GetRequiredService<IAccountRepository>();
        var costCenters = services.GetRequiredService<ICostCenterRepository>();
        var taxRates = services.GetRequiredService<ITaxRateRepository>();
        var categories = services.GetRequiredService<IProductCategoryRepository>();
        var uoms = services.GetRequiredService<IUnitOfMeasureRepository>();
        var warehouses = services.GetRequiredService<IWarehouseRepository>();
        var stockMappings = services.GetRequiredService<IStockAccountMappingRepository>();
        var partners = services.GetRequiredService<IBusinessPartnerRepository>();
        var products = services.GetRequiredService<IProductRepository>();
        var stockAdjustments = services.GetRequiredService<IStockAdjustmentRepository>();
        var invoices = services.GetRequiredService<IInvoiceRepository>();
        var stockTransfers = services.GetRequiredService<IStockTransferRepository>();
        var vouchers = services.GetRequiredService<IVoucherRepository>();

        Console.WriteLine("[DemoDataSeeder] Seeding Chart of Accounts...");
        var acct = await SeedChartOfAccountsAsync(db, accounts);

        Console.WriteLine("[DemoDataSeeder] Seeding Cost Centers...");
        var costCenterIds = await SeedCostCentersAsync(db, costCenters);

        Console.WriteLine("[DemoDataSeeder] Seeding Tax Rates...");
        await SeedTaxRatesAsync(db, taxRates);

        Console.WriteLine("[DemoDataSeeder] Seeding Product Categories...");
        var categoryIds = await SeedProductCategoriesAsync(db, categories);

        Console.WriteLine("[DemoDataSeeder] Seeding Units of Measure...");
        var pcsUomId = await SeedUnitOfMeasureAsync(db, uoms);

        Console.WriteLine("[DemoDataSeeder] Seeding Warehouses...");
        var (warehouse1Id, warehouse2Id) = await SeedWarehousesAsync(db, warehouses, costCenterIds["Warehouse Operations"]);

        Console.WriteLine("[DemoDataSeeder] Seeding default Stock Account Mapping...");
        await SeedStockAccountMappingAsync(db, stockMappings, acct);

        Console.WriteLine("[DemoDataSeeder] Seeding Business Partners...");
        await SeedBusinessPartnersAsync(db, partners);

        var existingProductCount = await db.Products.CountAsync();
        if (existingProductCount >= 200)
        {
            Console.WriteLine($"[DemoDataSeeder] {existingProductCount} products already exist — skipping product/stock generation.");
        }
        else
        {
            Console.WriteLine("[DemoDataSeeder] Seeding paint product catalog (this posts real stock movements, may take a few minutes)...");
            await SeedProductsAndStockAsync(db, products, stockAdjustments, categoryIds, pcsUomId, warehouse1Id, warehouse2Id);
        }

        Console.WriteLine("[DemoDataSeeder] Seeding purchase invoices & purchase returns...");
        await SeedPurchasesAsync(db, invoices, warehouse1Id);

        Console.WriteLine("[DemoDataSeeder] Seeding sales invoices & sale returns...");
        await SeedSalesAsync(db, invoices, warehouse1Id);

        Console.WriteLine("[DemoDataSeeder] Seeding stock transfers...");
        await SeedStockTransfersAsync(db, stockTransfers, warehouse1Id, warehouse2Id);

        Console.WriteLine("[DemoDataSeeder] Seeding expense vouchers...");
        await SeedExpenseVouchersAsync(db, vouchers, acct);

        Console.WriteLine("[DemoDataSeeder] Done.");
    }

    // ---------------------------------------------------------------- Chart of Accounts

    private record AccountSpec(string Code, string Name, AccountType Type, AccountNature Nature, string? ParentCode,
        bool IsControl = false, bool IsCash = false, bool IsBank = false);

    private static async Task<Dictionary<string, int>> SeedChartOfAccountsAsync(AppDbContext db, IAccountRepository accounts)
    {
        var specs = new[]
        {
            new AccountSpec("1000", "Assets", AccountType.Asset, AccountNature.Debit, null),
            new AccountSpec("1100", "Current Assets", AccountType.Asset, AccountNature.Debit, "1000"),
            new AccountSpec("1110", "Cash in Hand", AccountType.Asset, AccountNature.Debit, "1100", IsCash: true),
            new AccountSpec("1120", "Bank Account - Main", AccountType.Asset, AccountNature.Debit, "1100", IsBank: true),
            new AccountSpec("1130", "Accounts Receivable", AccountType.Asset, AccountNature.Debit, "1100", IsControl: true),
            new AccountSpec("1140", "Inventory Asset - Paints", AccountType.Asset, AccountNature.Debit, "1100"),
            new AccountSpec("1150", "Input Tax Recoverable", AccountType.Asset, AccountNature.Debit, "1100"),
            new AccountSpec("1160", "Advance to Suppliers", AccountType.Asset, AccountNature.Debit, "1100"),
            new AccountSpec("1200", "Fixed Assets", AccountType.Asset, AccountNature.Debit, "1000"),
            new AccountSpec("1210", "Furniture & Fixtures", AccountType.Asset, AccountNature.Debit, "1200"),
            new AccountSpec("1220", "Vehicles", AccountType.Asset, AccountNature.Debit, "1200"),
            new AccountSpec("1230", "Office Equipment", AccountType.Asset, AccountNature.Debit, "1200"),

            new AccountSpec("2000", "Liabilities", AccountType.Liability, AccountNature.Credit, null),
            new AccountSpec("2100", "Current Liabilities", AccountType.Liability, AccountNature.Credit, "2000"),
            new AccountSpec("2110", "Accounts Payable", AccountType.Liability, AccountNature.Credit, "2100", IsControl: true),
            new AccountSpec("2120", "Output Tax Payable", AccountType.Liability, AccountNature.Credit, "2100"),
            new AccountSpec("2130", "Accrued Expenses", AccountType.Liability, AccountNature.Credit, "2100"),

            new AccountSpec("3000", "Equity", AccountType.Equity, AccountNature.Credit, null),
            new AccountSpec("3100", "Owner's Capital", AccountType.Equity, AccountNature.Credit, "3000"),
            new AccountSpec("3200", "Retained Earnings", AccountType.Equity, AccountNature.Credit, "3000"),
            new AccountSpec("3300", "Opening Balance Equity", AccountType.Equity, AccountNature.Credit, "3000"),

            new AccountSpec("4000", "Income", AccountType.Income, AccountNature.Credit, null),
            new AccountSpec("4100", "Sales Revenue - Paints", AccountType.Income, AccountNature.Credit, "4000"),
            new AccountSpec("4200", "Sales Returns & Allowances", AccountType.Income, AccountNature.Credit, "4000"),
            new AccountSpec("4300", "Discount Income", AccountType.Income, AccountNature.Credit, "4000"),

            new AccountSpec("5000", "Expenses", AccountType.Expense, AccountNature.Debit, null),
            new AccountSpec("5100", "Cost of Goods Sold - Paints", AccountType.Expense, AccountNature.Debit, "5000"),
            new AccountSpec("5200", "Purchase Returns & Allowances", AccountType.Expense, AccountNature.Debit, "5000"),
            new AccountSpec("5300", "Stock Adjustment Variance", AccountType.Expense, AccountNature.Debit, "5000"),
            new AccountSpec("5400", "Rent Expense", AccountType.Expense, AccountNature.Debit, "5000"),
            new AccountSpec("5500", "Salaries & Wages", AccountType.Expense, AccountNature.Debit, "5000"),
            new AccountSpec("5600", "Utilities Expense", AccountType.Expense, AccountNature.Debit, "5000"),
        };

        var codeToId = new Dictionary<string, int>();
        foreach (var spec in specs)
        {
            var existing = await db.Accounts.FirstOrDefaultAsync(a => a.Code == spec.Code);
            if (existing is not null)
            {
                codeToId[spec.Code] = existing.Id;
                continue;
            }

            var response = await accounts.CreateAsync(new CreateAccountRequest
            {
                Code = spec.Code,
                Name = spec.Name,
                Type = spec.Type,
                Nature = spec.Nature,
                ParentAccountId = spec.ParentCode is null ? null : codeToId[spec.ParentCode],
                IsControlAccount = spec.IsControl,
                IsCashAccount = spec.IsCash,
                IsBankAccount = spec.IsBank,
            });
            codeToId[spec.Code] = response.Id;
        }

        return codeToId;
    }

    // ---------------------------------------------------------------- Cost Centers

    private static async Task<Dictionary<string, int>> SeedCostCentersAsync(AppDbContext db, ICostCenterRepository costCenters)
    {
        var names = new[] { "Warehouse Operations", "Sales & Distribution", "Administration" };
        var nameToId = new Dictionary<string, int>();

        foreach (var name in names)
        {
            var existing = await db.CostCenters.FirstOrDefaultAsync(c => c.Name == name);
            if (existing is not null)
            {
                nameToId[name] = existing.Id;
                continue;
            }

            var response = await costCenters.CreateAsync(new CostCenterRequest { Name = name });
            nameToId[name] = response.Id;
        }

        return nameToId;
    }

    // ---------------------------------------------------------------- Tax Rates

    private static async Task SeedTaxRatesAsync(AppDbContext db, ITaxRateRepository taxRates)
    {
        var specs = new (string Name, decimal Percentage)[]
        {
            ("Sales Tax 17%", 17m),
            ("Exempt", 0m),
        };

        foreach (var (name, percentage) in specs)
        {
            var exists = await db.TaxRates.AnyAsync(t => t.Name == name);
            if (!exists)
            {
                await taxRates.CreateAsync(new TaxRateRequest { Name = name, Percentage = percentage });
            }
        }
    }

    // ---------------------------------------------------------------- Product Categories

    private static readonly (string Code, string Name)[] CategorySpecs =
    [
        ("EXE", "Exterior Emulsion"),
        ("INE", "Interior Emulsion"),
        ("ENL", "Synthetic Enamel"),
        ("DIS", "Distemper"),
        ("PRM", "Primer"),
        ("VAR", "Wood & Metal Varnish"),
    ];

    private static async Task<Dictionary<string, int>> SeedProductCategoriesAsync(AppDbContext db, IProductCategoryRepository categories)
    {
        const string parentName = "Paints";
        var parent = await db.ProductCategories.FirstOrDefaultAsync(c => c.Name == parentName && c.ParentProductCategoryId == null);
        int parentId;
        if (parent is not null)
        {
            parentId = parent.Id;
        }
        else
        {
            var response = await categories.CreateAsync(new ProductCategoryRequest { Name = parentName });
            parentId = response.Id;
        }

        var codeToId = new Dictionary<string, int>();
        foreach (var (code, name) in CategorySpecs)
        {
            var existing = await db.ProductCategories.FirstOrDefaultAsync(c => c.Name == name && c.ParentProductCategoryId == parentId);
            if (existing is not null)
            {
                codeToId[code] = existing.Id;
                continue;
            }

            var response = await categories.CreateAsync(new ProductCategoryRequest { Name = name, ParentProductCategoryId = parentId });
            codeToId[code] = response.Id;
        }

        return codeToId;
    }

    // ---------------------------------------------------------------- Unit of Measure

    private static async Task<int> SeedUnitOfMeasureAsync(AppDbContext db, IUnitOfMeasureRepository uoms)
    {
        var existing = await db.UnitsOfMeasure.FirstOrDefaultAsync(u => u.Code == "PCS");
        if (existing is not null)
        {
            return existing.Id;
        }

        var response = await uoms.CreateAsync(new UnitOfMeasureRequest { Code = "PCS", Name = "Piece" });
        return response.Id;
    }

    // ---------------------------------------------------------------- Warehouses

    private static async Task<(int Warehouse1Id, int Warehouse2Id)> SeedWarehousesAsync(
        AppDbContext db, IWarehouseRepository warehouses, int costCenterId)
    {
        async Task<int> GetOrCreate(string code, string name, bool isDefault)
        {
            var existing = await db.Warehouses.FirstOrDefaultAsync(w => w.Code == code);
            if (existing is not null)
            {
                return existing.Id;
            }

            var response = await warehouses.CreateAsync(new WarehouseRequest
            {
                Code = code,
                Name = name,
                CostCenterId = costCenterId,
                IsDefault = isDefault,
            });
            return response.Id;
        }

        var wh1 = await GetOrCreate("WH-KHI", "Main Warehouse - Karachi", isDefault: true);
        var wh2 = await GetOrCreate("WH-LHR", "Branch Warehouse - Lahore", isDefault: false);
        return (wh1, wh2);
    }

    // ---------------------------------------------------------------- Stock Account Mapping

    private static async Task SeedStockAccountMappingAsync(AppDbContext db, IStockAccountMappingRepository stockMappings, Dictionary<string, int> acct)
    {
        var existing = await db.StockAccountMappings.AnyAsync(m => m.ProductCategoryId == null);
        if (existing)
        {
            return;
        }

        await stockMappings.CreateAsync(new StockAccountMappingRequest
        {
            ProductCategoryId = null,
            InventoryAssetAccountId = acct["1140"],
            COGSAccountId = acct["5100"],
            AccountsPayableAccountId = acct["2110"],
            SalesRevenueAccountId = acct["4100"],
            AccountsReceivableAccountId = acct["1130"],
            CashOrBankAccountId = acct["1120"],
            InputTaxAccountId = acct["1150"],
            OutputTaxAccountId = acct["2120"],
            StockAdjustmentVarianceAccountId = acct["5300"],
            OpeningBalanceEquityAccountId = acct["3300"],
        });
    }

    // ---------------------------------------------------------------- Business Partners

    private static readonly (string CompanyCode, string CompanyName)[] Companies =
    [
        ("BRG", "Berger"),
        ("DLX", "ICI Dulux"),
        ("BRT", "Brighto"),
        ("DMD", "Diamond"),
        ("MPI", "Master"),
        ("KPP", "Kansai"),
    ];

    private static async Task SeedBusinessPartnersAsync(AppDbContext db, IBusinessPartnerRepository partners)
    {
        foreach (var (companyCode, companyName) in Companies)
        {
            var code = $"SUP-{companyCode}";
            if (await db.BusinessPartners.AnyAsync(p => p.Code == code))
            {
                continue;
            }

            await partners.CreateAsync(new BusinessPartnerRequest
            {
                PartnerType = PartnerType.Supplier,
                Code = code,
                Name = $"{companyName} Paints Distribution (Pvt) Ltd",
                DefaultPaymentTermDays = 15,
            });
        }

        var customers = new (string Code, string Name, string City)[]
        {
            ("CUST-001", "Al-Noor Hardware Store", "Karachi"),
            ("CUST-002", "City Paint House", "Lahore"),
            ("CUST-003", "Metro Building Supplies", "Islamabad"),
            ("CUST-004", "Bilal Traders", "Faisalabad"),
            ("CUST-005", "Al-Madina Hardware", "Multan"),
            ("CUST-006", "Rawal Paint Center", "Rawalpindi"),
        };

        foreach (var (code, name, city) in customers)
        {
            if (await db.BusinessPartners.AnyAsync(p => p.Code == code))
            {
                continue;
            }

            await partners.CreateAsync(new BusinessPartnerRequest
            {
                PartnerType = PartnerType.Customer,
                Code = code,
                Name = name,
                Address = city,
                DefaultPaymentTermDays = 30,
                CreditLimit = 500_000m,
            });
        }
    }

    // ---------------------------------------------------------------- Products, prices & opening stock

    private record ColorSpec(string Code, string Name);

    private static readonly Dictionary<string, ColorSpec[]> ColorsByCategory = new()
    {
        ["EXE"] =
        [
            new("WHT", "White"), new("IVR", "Ivory"), new("SKB", "Sky Blue"),
            new("LGR", "Light Grey"), new("SUY", "Sunshine Yellow"), new("TRC", "Terracotta"),
        ],
        ["INE"] =
        [
            new("WHT", "White"), new("MAG", "Magnolia"), new("PPK", "Pastel Pink"),
            new("MGR", "Mint Green"), new("CRM", "Cream"), new("LGR", "Light Grey"),
        ],
        ["ENL"] =
        [
            new("WHT", "White"), new("BLK", "Black"), new("RDO", "Red Oxide"),
            new("RBL", "Royal Blue"), new("BGR", "Bottle Green"), new("GBR", "Golden Brown"),
        ],
        ["DIS"] =
        [
            new("WHT", "White"), new("IVR", "Ivory"), new("PBL", "Pastel Blue"),
            new("CRM", "Cream"), new("LPK", "Light Pink"), new("OWH", "Off White"),
        ],
        ["PRM"] =
        [
            new("RDO", "Red Oxide"), new("GRY", "Grey"), new("WHT", "White"),
            new("ZCY", "Zinc Chromate Yellow"), new("PPR", "Pink Primer"), new("UGR", "Universal Grey"),
        ],
        ["VAR"] =
        [
            new("CLR", "Clear"), new("TEK", "Teak"), new("WAL", "Walnut"),
            new("MAH", "Mahogany"), new("RSW", "Rosewood"), new("BLK", "Black"),
        ],
    };

    // Base purchase cost (PKR) per pack size, before the per-company price multiplier.
    private static readonly Dictionary<string, (decimal Quarter, decimal Gallon, decimal Drum)> BaseCostByCategory = new()
    {
        ["EXE"] = (550m, 2000m, 9000m),
        ["INE"] = (480m, 1750m, 7800m),
        ["ENL"] = (620m, 2300m, 10200m),
        ["DIS"] = (320m, 1150m, 5200m),
        ["PRM"] = (400m, 1450m, 6400m),
        ["VAR"] = (580m, 2100m, 9400m),
    };

    private static readonly Dictionary<string, decimal> CompanyPriceMultiplier = new()
    {
        ["BRG"] = 1.15m,
        ["DLX"] = 1.25m,
        ["BRT"] = 0.95m,
        ["DMD"] = 0.85m,
        ["MPI"] = 1.00m,
        ["KPP"] = 1.05m,
    };

    // (Size label, variant-code suffix, litres shown in name, opening-qty range for Warehouse 1)
    private static readonly (string Label, string Suffix, string Litres, int QtyMin, int QtyMax)[] PackSizes =
    [
        ("Quarter", "QTR", "0.9L", 60, 180),
        ("Gallon", "GAL", "3.6L", 30, 100),
        ("Drum", "DRM", "20L", 8, 35),
    ];

    private static async Task SeedProductsAndStockAsync(
        AppDbContext db,
        IProductRepository products,
        IStockAdjustmentRepository stockAdjustments,
        Dictionary<string, int> categoryIds,
        int baseUomId,
        int warehouse1Id,
        int warehouse2Id)
    {
        var rng = new Random(20260810);
        var lines1 = new List<StockAdjustmentLineRequest>();
        var lines2 = new List<StockAdjustmentLineRequest>();
        var productCount = 0;

        foreach (var (companyCode, companyName) in Companies)
        {
            foreach (var (categoryCode, categoryName) in CategorySpecs)
            {
                var baseCost = BaseCostByCategory[categoryCode];
                var multiplier = CompanyPriceMultiplier[companyCode];

                foreach (var color in ColorsByCategory[categoryCode])
                {
                    var sku = $"{companyCode}-{categoryCode}-{color.Code}";
                    var existingProduct = await db.Products.Include(p => p.Variants).FirstOrDefaultAsync(p => p.SKU == sku);
                    if (existingProduct is not null)
                    {
                        continue;
                    }

                    var productName = $"{companyName} {categoryName} - {color.Name}";
                    var variantRequests = PackSizes.Select(size => new ProductVariantRequest
                    {
                        Name = $"{size.Label} ({size.Litres})",
                        VariantCode = $"{sku}-{size.Suffix}",
                        IsDefault = size.Suffix == "GAL",
                    }).ToList();

                    var productResponse = await products.CreateAsync(new ProductRequest
                    {
                        SKU = sku,
                        Name = productName,
                        ProductCategoryId = categoryIds[categoryCode],
                        BaseUnitOfMeasureId = baseUomId,
                        HasVariants = true,
                        IsStockTracked = true,
                        Variants = variantRequests,
                    });

                    for (var i = 0; i < PackSizes.Length; i++)
                    {
                        var size = PackSizes[i];
                        var variant = productResponse.Variants.First(v => v.VariantCode == $"{sku}-{size.Suffix}");

                        var baseSizeCost = size.Suffix switch
                        {
                            "QTR" => baseCost.Quarter,
                            "GAL" => baseCost.Gallon,
                            _ => baseCost.Drum,
                        };
                        var purchaseCost = Math.Round(baseSizeCost * multiplier, 2);
                        var salePrice = Math.Round(purchaseCost * 1.30m, 2);
                        var retailPrice = Math.Round(salePrice * 1.10m, 2);

                        await products.SetVariantPricesAsync(productResponse.Id, variant.Id,
                        [
                            new ProductVariantPriceRequest { PriceType = PriceType.Purchase, Amount = purchaseCost },
                            new ProductVariantPriceRequest { PriceType = PriceType.Sale, Amount = salePrice },
                            new ProductVariantPriceRequest { PriceType = PriceType.Retail, Amount = retailPrice },
                        ]);

                        var qty1 = rng.Next(size.QtyMin, size.QtyMax + 1);
                        var qty2 = Math.Max(1, (int)(qty1 * (0.4 + rng.NextDouble() * 0.3)));

                        lines1.Add(new StockAdjustmentLineRequest
                        {
                            ProductVariantId = variant.Id,
                            Direction = AdjustmentDirection.Increase,
                            BaseQty = qty1,
                            UnitCost = purchaseCost,
                        });
                        lines2.Add(new StockAdjustmentLineRequest
                        {
                            ProductVariantId = variant.Id,
                            Direction = AdjustmentDirection.Increase,
                            BaseQty = qty2,
                            UnitCost = purchaseCost,
                        });
                    }

                    productCount++;
                    if (productCount % 20 == 0)
                    {
                        Console.WriteLine($"[DemoDataSeeder] ...{productCount} products created");
                        db.ChangeTracker.Clear();
                    }
                }
            }
        }

        Console.WriteLine($"[DemoDataSeeder] {productCount} products created. Posting opening stock ({lines1.Count} lines x 2 warehouses)...");

        await PostOpeningBalanceAsync(stockAdjustments, warehouse1Id, "Main Warehouse", lines1);
        db.ChangeTracker.Clear();
        await PostOpeningBalanceAsync(stockAdjustments, warehouse2Id, "Branch Warehouse", lines2);
        db.ChangeTracker.Clear();
    }

    private static async Task PostOpeningBalanceAsync(
        IStockAdjustmentRepository stockAdjustments, int warehouseId, string warehouseLabel, List<StockAdjustmentLineRequest> lines)
    {
        var header = await stockAdjustments.CreateAsync(new StockAdjustmentRequest
        {
            WarehouseId = warehouseId,
            Date = OpeningDate,
            ReasonCode = AdjustmentReasonCode.OpeningBalance,
            Narration = $"Demo data opening stock - {warehouseLabel}",
            Lines = lines,
        }, SeedActor);

        await stockAdjustments.SubmitAsync(header.Id);
        await stockAdjustments.ApproveAsync(header.Id, SeedActor);
    }

    // ---------------------------------------------------------------- Purchases & Purchase Returns

    private static async Task SeedPurchasesAsync(AppDbContext db, IInvoiceRepository invoices, int warehouseId)
    {
        if (await db.InvoiceHeaders.AnyAsync(h => h.InvoiceType == InvoiceType.PurchaseInvoice))
        {
            Console.WriteLine("[DemoDataSeeder] Purchase invoices already exist — skipping.");
            return;
        }

        var suppliers = await db.BusinessPartners.Where(p => p.Code.StartsWith("SUP-")).OrderBy(p => p.Code).ToListAsync();
        var rng = new Random(20260810 + 1);
        var createdInvoiceIds = new List<int>();

        for (var i = 0; i < suppliers.Count; i++)
        {
            var supplier = suppliers[i];
            var companyCode = supplier.Code["SUP-".Length..];

            var variants = await db.ProductVariants
                .Include(v => v.Product)
                .Include(v => v.Prices)
                .Where(v => v.Product!.SKU.StartsWith(companyCode + "-"))
                .OrderBy(v => v.Id)
                .Take(9)
                .ToListAsync();

            if (variants.Count == 0)
            {
                continue;
            }

            var paymentMode = i % 2 == 0 ? PaymentMode.Credit : PaymentMode.Cash;
            var lines = variants.Select(v => new InvoiceLineRequest
            {
                ProductVariantId = v.Id,
                UnitOfMeasureId = v.Product!.BaseUnitOfMeasureId,
                Qty = rng.Next(20, 60),
                UnitAmount = v.Prices.First(p => p.PriceType == PriceType.Purchase).Amount,
            }).ToList();

            var invoice = await invoices.CreateAsync(new InvoiceRequest
            {
                InvoiceType = InvoiceType.PurchaseInvoice,
                PartnerId = supplier.Id,
                WarehouseId = warehouseId,
                Date = PurchaseInvoiceDate,
                PaymentMode = paymentMode,
                PaymentTermDays = paymentMode == PaymentMode.Credit ? 30 : 0,
                Narration = $"Demo purchase from {supplier.Name}",
                Lines = lines,
            }, SeedActor);

            await invoices.SubmitAsync(invoice.Id);
            await invoices.ApproveAsync(invoice.Id, SeedActor);
            createdInvoiceIds.Add(invoice.Id);
        }

        // Purchase returns: return a quarter of the first line's qty on the first two purchase invoices.
        foreach (var invoiceId in createdInvoiceIds.Take(2))
        {
            var original = await invoices.GetByIdAsync(invoiceId);
            var firstLine = original.Lines.First();
            var returnQty = Math.Max(1m, Math.Floor(firstLine.Qty / 4));

            var ret = await invoices.CreateAsync(new InvoiceRequest
            {
                InvoiceType = InvoiceType.PurchaseReturn,
                PartnerId = original.PartnerId,
                ReferenceInvoiceId = original.Id,
                WarehouseId = warehouseId,
                Date = ReturnDate,
                PaymentMode = original.PaymentMode,
                PaymentTermDays = 0,
                Narration = $"Demo purchase return against {original.InvoiceNo}",
                Lines =
                [
                    new InvoiceLineRequest
                    {
                        ProductVariantId = firstLine.ProductVariantId,
                        UnitOfMeasureId = firstLine.UnitOfMeasureId,
                        Qty = returnQty,
                        UnitAmount = firstLine.UnitAmount,
                        ReferenceInvoiceLineId = firstLine.Id,
                    },
                ],
            }, SeedActor);

            await invoices.SubmitAsync(ret.Id);
            await invoices.ApproveAsync(ret.Id, SeedActor);
        }
    }

    // ---------------------------------------------------------------- Sales & Sale Returns

    private static async Task SeedSalesAsync(AppDbContext db, IInvoiceRepository invoices, int warehouseId)
    {
        if (await db.InvoiceHeaders.AnyAsync(h => h.InvoiceType == InvoiceType.SalesInvoice))
        {
            Console.WriteLine("[DemoDataSeeder] Sales invoices already exist — skipping.");
            return;
        }

        var customers = await db.BusinessPartners.Where(p => p.Code.StartsWith("CUST-")).OrderBy(p => p.Code).ToListAsync();
        var rng = new Random(20260810 + 2);
        var createdInvoiceIds = new List<int>();

        var defaultVariants = await db.ProductVariants
            .Include(v => v.Product)
            .Include(v => v.Prices)
            .Where(v => v.IsDefault)
            .OrderBy(v => v.Id)
            .ToListAsync();

        for (var i = 0; i < customers.Count; i++)
        {
            var customer = customers[i];
            var picks = defaultVariants.Skip(i * 11 % Math.Max(1, defaultVariants.Count - 5)).Take(5).ToList();
            if (picks.Count == 0)
            {
                continue;
            }

            var paymentMode = i % 2 == 0 ? PaymentMode.Credit : PaymentMode.Cash;
            var lines = picks.Select(v => new InvoiceLineRequest
            {
                ProductVariantId = v.Id,
                UnitOfMeasureId = v.Product!.BaseUnitOfMeasureId,
                Qty = rng.Next(5, 15),
                UnitAmount = v.Prices.First(p => p.PriceType == PriceType.Sale).Amount,
            }).ToList();

            var invoice = await invoices.CreateAsync(new InvoiceRequest
            {
                InvoiceType = InvoiceType.SalesInvoice,
                PartnerId = customer.Id,
                WarehouseId = warehouseId,
                Date = SalesInvoiceDate,
                PaymentMode = paymentMode,
                PaymentTermDays = paymentMode == PaymentMode.Credit ? 30 : 0,
                Narration = $"Demo sale to {customer.Name}",
                Lines = lines,
            }, SeedActor);

            await invoices.SubmitAsync(invoice.Id);
            await invoices.ApproveAsync(invoice.Id, SeedActor);
            createdInvoiceIds.Add(invoice.Id);
        }

        // Sale returns: return a quarter of the first line's qty on the first two sales invoices.
        foreach (var invoiceId in createdInvoiceIds.Take(2))
        {
            var original = await invoices.GetByIdAsync(invoiceId);
            var firstLine = original.Lines.First();
            var returnQty = Math.Max(1m, Math.Floor(firstLine.Qty / 4));

            var ret = await invoices.CreateAsync(new InvoiceRequest
            {
                InvoiceType = InvoiceType.SaleReturn,
                PartnerId = original.PartnerId,
                ReferenceInvoiceId = original.Id,
                WarehouseId = warehouseId,
                Date = ReturnDate,
                PaymentMode = original.PaymentMode,
                PaymentTermDays = 0,
                Narration = $"Demo sale return against {original.InvoiceNo}",
                Lines =
                [
                    new InvoiceLineRequest
                    {
                        ProductVariantId = firstLine.ProductVariantId,
                        UnitOfMeasureId = firstLine.UnitOfMeasureId,
                        Qty = returnQty,
                        UnitAmount = firstLine.UnitAmount,
                        ReferenceInvoiceLineId = firstLine.Id,
                    },
                ],
            }, SeedActor);

            await invoices.SubmitAsync(ret.Id);
            await invoices.ApproveAsync(ret.Id, SeedActor);
        }
    }

    // ---------------------------------------------------------------- Stock Transfers

    private static async Task SeedStockTransfersAsync(
        AppDbContext db, IStockTransferRepository stockTransfers, int warehouse1Id, int warehouse2Id)
    {
        if (await db.StockTransferHeaders.AnyAsync())
        {
            Console.WriteLine("[DemoDataSeeder] Stock transfers already exist — skipping.");
            return;
        }

        var rng = new Random(20260810 + 3);
        var variants = await db.ProductVariants.Include(v => v.Product).Where(v => v.IsDefault).OrderBy(v => v.Id).ToListAsync();

        async Task PostTransfer(int sourceId, int destinationId, string label, IEnumerable<ProductVariant> picks)
        {
            var lines = picks.Select(v => new StockTransferLineRequest
            {
                ProductVariantId = v.Id,
                UnitOfMeasureId = v.Product!.BaseUnitOfMeasureId,
                Qty = rng.Next(5, 20),
            }).ToList();

            if (lines.Count == 0)
            {
                return;
            }

            var transfer = await stockTransfers.CreateAsync(new StockTransferRequest
            {
                SourceWarehouseId = sourceId,
                DestinationWarehouseId = destinationId,
                Date = TransferDate,
                Narration = $"Demo stock transfer - {label}",
                Lines = lines,
            }, SeedActor);

            await stockTransfers.SubmitAsync(transfer.Id);
            await stockTransfers.ApproveAsync(transfer.Id, SeedActor);
        }

        await PostTransfer(warehouse1Id, warehouse2Id, "Main to Branch", variants.Take(10));
        await PostTransfer(warehouse2Id, warehouse1Id, "Branch to Main", variants.Skip(10).Take(10));
    }

    // ---------------------------------------------------------------- Expense Vouchers

    private static readonly (string AccountCode, string Label, int Min, int Max)[] ExpenseSpecs =
    [
        ("5400", "Rent", 75_000, 85_000),
        ("5500", "Salaries & Wages", 320_000, 380_000),
        ("5600", "Utilities", 35_000, 55_000),
    ];

    private static async Task SeedExpenseVouchersAsync(AppDbContext db, IVoucherRepository vouchers, Dictionary<string, int> acct)
    {
        const string marker = "Demo Expense";
        if (await db.VoucherHeaders.AnyAsync(v => v.Narration.StartsWith(marker)))
        {
            Console.WriteLine("[DemoDataSeeder] Expense vouchers already exist — skipping.");
            return;
        }

        var rng = new Random(20260810 + 4);
        var bankAccountId = acct["1120"];

        for (var month = 1; month <= 6; month++)
        {
            var date = new DateTime(2026, month, 5);

            foreach (var (accountCode, label, min, max) in ExpenseSpecs)
            {
                var amount = rng.Next(min, max);

                var voucher = await vouchers.CreateAsync(new CreateVoucherRequest
                {
                    VoucherType = VoucherType.Payment,
                    Date = date,
                    Narration = $"{marker} - {label} - {date:MMM yyyy}",
                    Lines =
                    [
                        new VoucherLineRequest { AccountId = acct[accountCode], DebitAmount = amount },
                        new VoucherLineRequest { AccountId = bankAccountId, CreditAmount = amount },
                    ],
                }, SeedActor);

                await vouchers.SubmitAsync(voucher.Id);
                await vouchers.ApproveAsync(voucher.Id, SeedActor);
            }
        }
    }
}
