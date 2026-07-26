using System.Reflection;
using erp_backend;
using erp_backend.Auth;
using erp_backend.Models;
using erp_backend.Models.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace erp_backend.Data;

public class AppDbContext : DbContext
{
    private readonly ICurrentTenantContext _tenantContext;

    public AppDbContext(DbContextOptions<AppDbContext> options, ICurrentTenantContext tenantContext) : base(options)
    {
        _tenantContext = tenantContext;
    }

    public DbSet<WeatherForecast> WeatherForecasts => Set<WeatherForecast>();
    public DbSet<Tenant> Tenants => Set<Tenant>();
    public DbSet<Branch> Branches => Set<Branch>();
    public DbSet<UserBranch> UserBranches => Set<UserBranch>();
    public DbSet<User> Users => Set<User>();
    public DbSet<Account> Accounts => Set<Account>();
    public DbSet<CostCenter> CostCenters => Set<CostCenter>();
    public DbSet<FiscalPeriod> FiscalPeriods => Set<FiscalPeriod>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();
    public DbSet<TaxRate> TaxRates => Set<TaxRate>();
    public DbSet<VoucherHeader> VoucherHeaders => Set<VoucherHeader>();
    public DbSet<VoucherDetail> VoucherDetails => Set<VoucherDetail>();
    public DbSet<VoucherAttachment> VoucherAttachments => Set<VoucherAttachment>();
    public DbSet<RecurringVoucherTemplate> RecurringVoucherTemplates => Set<RecurringVoucherTemplate>();
    public DbSet<RecurringVoucherTemplateLine> RecurringVoucherTemplateLines => Set<RecurringVoucherTemplateLine>();
    public DbSet<BankStatementLine> BankStatementLines => Set<BankStatementLine>();
    public DbSet<Budget> Budgets => Set<Budget>();
    public DbSet<ReportSchedule> ReportSchedules => Set<ReportSchedule>();
    public DbSet<Right> Rights => Set<Right>();
    public DbSet<Role> Roles => Set<Role>();
    public DbSet<RoleRight> RoleRights => Set<RoleRight>();

    public DbSet<ProductCategory> ProductCategories => Set<ProductCategory>();
    public DbSet<UnitOfMeasure> UnitsOfMeasure => Set<UnitOfMeasure>();
    public DbSet<Product> Products => Set<Product>();
    public DbSet<ProductVariant> ProductVariants => Set<ProductVariant>();
    public DbSet<UOMConversion> UOMConversions => Set<UOMConversion>();
    public DbSet<ProductVariantPrice> ProductVariantPrices => Set<ProductVariantPrice>();
    public DbSet<BusinessPartner> BusinessPartners => Set<BusinessPartner>();
    public DbSet<Warehouse> Warehouses => Set<Warehouse>();
    public DbSet<StockAccountMapping> StockAccountMappings => Set<StockAccountMapping>();
    public DbSet<StockLedgerEntry> StockLedgerEntries => Set<StockLedgerEntry>();
    public DbSet<StockBalance> StockBalances => Set<StockBalance>();
    public DbSet<InvoiceHeader> InvoiceHeaders => Set<InvoiceHeader>();
    public DbSet<InvoiceLine> InvoiceLines => Set<InvoiceLine>();
    public DbSet<PartnerPaymentHeader> PartnerPaymentHeaders => Set<PartnerPaymentHeader>();
    public DbSet<PartnerPaymentAllocation> PartnerPaymentAllocations => Set<PartnerPaymentAllocation>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Tenant>(entity =>
        {
            entity.HasIndex(t => t.Code).IsUnique();
        });

        modelBuilder.Entity<Branch>(entity =>
        {
            entity.HasIndex(b => new { b.TenantId, b.Code }).IsUnique();

            entity.HasOne(b => b.Tenant)
                .WithMany()
                .HasForeignKey(b => b.TenantId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<UserBranch>(entity =>
        {
            entity.HasKey(ub => new { ub.UserId, ub.BranchId });

            entity.HasOne(ub => ub.User)
                .WithMany(u => u.UserBranches)
                .HasForeignKey(ub => ub.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(ub => ub.Branch)
                .WithMany()
                .HasForeignKey(ub => ub.BranchId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasIndex(u => u.Username).IsUnique();
            entity.HasIndex(u => u.TenantId);

            entity.HasOne(u => u.Tenant)
                .WithMany()
                .HasForeignKey(u => u.TenantId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(u => u.Role)
                .WithMany()
                .HasForeignKey(u => u.RoleId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Right>(entity =>
        {
            entity.HasIndex(r => r.Code).IsUnique();
        });

        modelBuilder.Entity<Role>(entity =>
        {
            entity.HasQueryFilter(r => r.TenantId == null || r.TenantId == _tenantContext.TenantId);
            entity.HasIndex(r => new { r.TenantId, r.Name }).IsUnique();

            entity.HasOne(r => r.Tenant)
                .WithMany()
                .HasForeignKey(r => r.TenantId)
                .OnDelete(DeleteBehavior.Restrict);

            // Fixed literal CreatedAtUtc (not DateTime.UtcNow) so `dotnet ef migrations add` never
            // sees this seed as changed on a later run.
            var builtInRolesSeededAt = new DateTime(2026, 7, 21, 0, 0, 0, DateTimeKind.Utc);

            entity.HasData(
                new Role { Id = 1, TenantId = null, Name = AppRoles.User, Description = "Standard application user.", IsSystemRole = true, CreatedAtUtc = builtInRolesSeededAt },
                new Role { Id = 2, TenantId = null, Name = AppRoles.Admin, Description = "Tenant administrator.", IsSystemRole = true, CreatedAtUtc = builtInRolesSeededAt },
                new Role { Id = 3, TenantId = null, Name = AppRoles.SystemAdmin, Description = "Cross-tenant platform administrator.", IsSystemRole = true, CreatedAtUtc = builtInRolesSeededAt });
        });

        modelBuilder.Entity<RoleRight>(entity =>
        {
            entity.HasKey(rr => new { rr.RoleId, rr.RightId });

            entity.HasOne(rr => rr.Role)
                .WithMany(r => r.RoleRights)
                .HasForeignKey(rr => rr.RoleId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(rr => rr.Right)
                .WithMany()
                .HasForeignKey(rr => rr.RightId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Account>(entity =>
        {
            entity.HasIndex(a => new { a.TenantId, a.Code }).IsUnique();

            entity.Property(a => a.OpeningBalance).HasColumnType("decimal(18,2)");

            entity.HasOne(a => a.ParentAccount)
                .WithMany()
                .HasForeignKey(a => a.ParentAccountId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<CostCenter>(entity =>
        {
            entity.HasIndex(c => c.TenantId);

            entity.HasOne(c => c.ParentCostCenter)
                .WithMany()
                .HasForeignKey(c => c.ParentCostCenterId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<TaxRate>(entity =>
        {
            entity.HasIndex(t => t.TenantId);
            entity.Property(t => t.Percentage).HasColumnType("decimal(5,2)");
        });

        modelBuilder.Entity<FiscalPeriod>(entity =>
        {
            entity.HasIndex(f => f.TenantId);
        });

        modelBuilder.Entity<VoucherHeader>(entity =>
        {
            entity.HasIndex(v => new { v.TenantId, v.BranchId, v.VoucherNo }).IsUnique();
            entity.HasIndex(v => new { v.TenantId, v.BranchId });
            entity.Property(v => v.ExchangeRate).HasColumnType("decimal(18,6)");

            entity.HasOne(v => v.ReversalOfVoucher)
                .WithMany()
                .HasForeignKey(v => v.ReversalOfVoucherId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasMany(v => v.Details)
                .WithOne(d => d.Voucher)
                .HasForeignKey(d => d.VoucherId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasMany(v => v.Attachments)
                .WithOne(a => a.Voucher)
                .HasForeignKey(a => a.VoucherId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<VoucherDetail>(entity =>
        {
            entity.HasIndex(d => new { d.TenantId, d.BranchId });
            entity.Property(d => d.DebitAmount).HasColumnType("decimal(18,2)");
            entity.Property(d => d.CreditAmount).HasColumnType("decimal(18,2)");
            entity.Property(d => d.TaxAmount).HasColumnType("decimal(18,2)");

            entity.HasOne(d => d.Account)
                .WithMany()
                .HasForeignKey(d => d.AccountId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(d => d.CostCenter)
                .WithMany()
                .HasForeignKey(d => d.CostCenterId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(d => d.TaxRate)
                .WithMany()
                .HasForeignKey(d => d.TaxRateId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<VoucherAttachment>(entity =>
        {
            entity.HasIndex(a => new { a.TenantId, a.BranchId });
        });

        modelBuilder.Entity<RecurringVoucherTemplate>(entity =>
        {
            entity.HasIndex(t => new { t.TenantId, t.BranchId });

            entity.HasMany(t => t.Lines)
                .WithOne(l => l.RecurringVoucherTemplate)
                .HasForeignKey(l => l.RecurringVoucherTemplateId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<RecurringVoucherTemplateLine>(entity =>
        {
            entity.HasIndex(l => new { l.TenantId, l.BranchId });
            entity.Property(l => l.DebitAmount).HasColumnType("decimal(18,2)");
            entity.Property(l => l.CreditAmount).HasColumnType("decimal(18,2)");

            entity.HasOne(l => l.Account)
                .WithMany()
                .HasForeignKey(l => l.AccountId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(l => l.CostCenter)
                .WithMany()
                .HasForeignKey(l => l.CostCenterId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<BankStatementLine>(entity =>
        {
            entity.HasIndex(b => new { b.TenantId, b.BranchId });
            entity.Property(b => b.Amount).HasColumnType("decimal(18,2)");

            entity.HasOne(b => b.BankAccount)
                .WithMany()
                .HasForeignKey(b => b.BankAccountId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(b => b.MatchedVoucherDetail)
                .WithMany()
                .HasForeignKey(b => b.MatchedVoucherDetailId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Budget>(entity =>
        {
            entity.Property(b => b.BudgetedAmount).HasColumnType("decimal(18,2)");
            entity.HasIndex(b => new { b.TenantId, b.BranchId, b.AccountId, b.Year, b.Month }).IsUnique();

            entity.HasOne(b => b.Account)
                .WithMany()
                .HasForeignKey(b => b.AccountId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<ReportSchedule>(entity =>
        {
            entity.HasIndex(r => new { r.TenantId, r.BranchId });
        });

        modelBuilder.Entity<AuditLog>(entity =>
        {
            entity.HasIndex(a => a.TenantId);
        });

        modelBuilder.Entity<ProductCategory>(entity =>
        {
            entity.HasIndex(c => c.TenantId);

            entity.HasOne(c => c.ParentProductCategory)
                .WithMany()
                .HasForeignKey(c => c.ParentProductCategoryId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<UnitOfMeasure>(entity =>
        {
            entity.HasIndex(u => new { u.TenantId, u.Code }).IsUnique();
        });

        modelBuilder.Entity<Product>(entity =>
        {
            entity.HasIndex(p => new { p.TenantId, p.SKU }).IsUnique();
            entity.Property(p => p.ReorderLevel).HasColumnType("decimal(18,4)");

            entity.HasOne(p => p.ProductCategory)
                .WithMany()
                .HasForeignKey(p => p.ProductCategoryId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(p => p.BaseUnitOfMeasure)
                .WithMany()
                .HasForeignKey(p => p.BaseUnitOfMeasureId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasMany(p => p.Variants)
                .WithOne(v => v.Product)
                .HasForeignKey(v => v.ProductId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasMany(p => p.UOMConversions)
                .WithOne(c => c.Product)
                .HasForeignKey(c => c.ProductId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<ProductVariant>(entity =>
        {
            entity.HasIndex(v => new { v.TenantId, v.ProductId, v.VariantCode }).IsUnique();

            entity.HasMany(v => v.Prices)
                .WithOne(p => p.ProductVariant)
                .HasForeignKey(p => p.ProductVariantId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<UOMConversion>(entity =>
        {
            entity.HasIndex(c => new { c.TenantId, c.ProductId, c.UnitOfMeasureId }).IsUnique();
            entity.Property(c => c.ConversionFactor).HasColumnType("decimal(18,6)");

            entity.HasOne(c => c.UnitOfMeasure)
                .WithMany()
                .HasForeignKey(c => c.UnitOfMeasureId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<ProductVariantPrice>(entity =>
        {
            entity.HasIndex(p => new { p.TenantId, p.ProductVariantId, p.PriceType }).IsUnique();
            entity.Property(p => p.Amount).HasColumnType("decimal(18,4)");
        });

        modelBuilder.Entity<BusinessPartner>(entity =>
        {
            entity.HasIndex(p => new { p.TenantId, p.Code }).IsUnique();
            entity.Property(p => p.CreditLimit).HasColumnType("decimal(18,2)");
        });

        modelBuilder.Entity<Warehouse>(entity =>
        {
            entity.HasIndex(w => new { w.TenantId, w.BranchId, w.Code }).IsUnique();

            entity.HasOne(w => w.CostCenter)
                .WithMany()
                .HasForeignKey(w => w.CostCenterId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<StockAccountMapping>(entity =>
        {
            entity.HasIndex(m => m.TenantId);

            entity.HasOne(m => m.ProductCategory).WithMany().HasForeignKey(m => m.ProductCategoryId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(m => m.InventoryAssetAccount).WithMany().HasForeignKey(m => m.InventoryAssetAccountId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(m => m.COGSAccount).WithMany().HasForeignKey(m => m.COGSAccountId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(m => m.AccountsPayableAccount).WithMany().HasForeignKey(m => m.AccountsPayableAccountId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(m => m.SalesRevenueAccount).WithMany().HasForeignKey(m => m.SalesRevenueAccountId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(m => m.AccountsReceivableAccount).WithMany().HasForeignKey(m => m.AccountsReceivableAccountId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(m => m.CashOrBankAccount).WithMany().HasForeignKey(m => m.CashOrBankAccountId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(m => m.InputTaxAccount).WithMany().HasForeignKey(m => m.InputTaxAccountId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(m => m.OutputTaxAccount).WithMany().HasForeignKey(m => m.OutputTaxAccountId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(m => m.StockAdjustmentVarianceAccount).WithMany().HasForeignKey(m => m.StockAdjustmentVarianceAccountId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(m => m.OpeningBalanceEquityAccount).WithMany().HasForeignKey(m => m.OpeningBalanceEquityAccountId).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<StockLedgerEntry>(entity =>
        {
            entity.HasIndex(e => new { e.TenantId, e.BranchId, e.ProductVariantId, e.WarehouseId });
            entity.Property(e => e.QuantityIn).HasColumnType("decimal(18,4)");
            entity.Property(e => e.QuantityOut).HasColumnType("decimal(18,4)");
            entity.Property(e => e.UnitCost).HasColumnType("decimal(18,4)");
            entity.Property(e => e.TotalCostSigned).HasColumnType("decimal(18,2)");
            entity.Property(e => e.RunningQuantity).HasColumnType("decimal(18,4)");
            entity.Property(e => e.RunningValue).HasColumnType("decimal(18,2)");

            entity.HasOne(e => e.ProductVariant).WithMany().HasForeignKey(e => e.ProductVariantId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(e => e.Warehouse).WithMany().HasForeignKey(e => e.WarehouseId).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<StockBalance>(entity =>
        {
            entity.HasIndex(b => new { b.TenantId, b.BranchId, b.ProductVariantId, b.WarehouseId }).IsUnique();
            entity.Property(b => b.QuantityOnHand).HasColumnType("decimal(18,4)");
            entity.Property(b => b.AverageCost).HasColumnType("decimal(18,4)");
            entity.Property(b => b.ReorderLevel).HasColumnType("decimal(18,4)");

            entity.HasOne(b => b.ProductVariant).WithMany().HasForeignKey(b => b.ProductVariantId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(b => b.Warehouse).WithMany().HasForeignKey(b => b.WarehouseId).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<InvoiceHeader>(entity =>
        {
            entity.HasIndex(h => new { h.TenantId, h.BranchId, h.InvoiceType, h.InvoiceNo }).IsUnique();
            entity.HasIndex(h => new { h.TenantId, h.BranchId, h.InvoiceType, h.Status });
            entity.Property(h => h.AmountPaid).HasColumnType("decimal(18,2)");
            entity.Property(h => h.OutstandingAmount).HasColumnType("decimal(18,2)");

            entity.HasOne(h => h.Partner).WithMany().HasForeignKey(h => h.PartnerId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(h => h.Warehouse).WithMany().HasForeignKey(h => h.WarehouseId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(h => h.ReferenceInvoice).WithMany().HasForeignKey(h => h.ReferenceInvoiceId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(h => h.LinkedVoucher).WithMany().HasForeignKey(h => h.LinkedVoucherId).OnDelete(DeleteBehavior.Restrict);

            entity.HasMany(h => h.Lines)
                .WithOne(l => l.InvoiceHeader)
                .HasForeignKey(l => l.InvoiceHeaderId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<InvoiceLine>(entity =>
        {
            entity.HasIndex(l => new { l.TenantId, l.BranchId });
            entity.Property(l => l.Qty).HasColumnType("decimal(18,4)");
            entity.Property(l => l.BaseQty).HasColumnType("decimal(18,4)");
            entity.Property(l => l.UnitAmount).HasColumnType("decimal(18,4)");
            entity.Property(l => l.UnitCostAtSale).HasColumnType("decimal(18,4)");
            entity.Property(l => l.TaxAmount).HasColumnType("decimal(18,2)");
            entity.Property(l => l.LineTotal).HasColumnType("decimal(18,2)");
            entity.Property(l => l.FulfilledBaseQty).HasColumnType("decimal(18,4)");

            entity.HasOne(l => l.ProductVariant).WithMany().HasForeignKey(l => l.ProductVariantId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(l => l.UnitOfMeasure).WithMany().HasForeignKey(l => l.UnitOfMeasureId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(l => l.TaxRate).WithMany().HasForeignKey(l => l.TaxRateId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(l => l.ReferenceInvoiceLine).WithMany().HasForeignKey(l => l.ReferenceInvoiceLineId).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<PartnerPaymentHeader>(entity =>
        {
            entity.HasIndex(p => new { p.TenantId, p.BranchId, p.Direction, p.PaymentNo }).IsUnique();
            entity.Property(p => p.TotalAmount).HasColumnType("decimal(18,2)");

            entity.HasOne(p => p.Partner).WithMany().HasForeignKey(p => p.PartnerId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(p => p.BankOrCashAccount).WithMany().HasForeignKey(p => p.BankOrCashAccountId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(p => p.LinkedVoucher).WithMany().HasForeignKey(p => p.LinkedVoucherId).OnDelete(DeleteBehavior.Restrict);

            entity.HasMany(p => p.Allocations)
                .WithOne(a => a.PartnerPaymentHeader)
                .HasForeignKey(a => a.PartnerPaymentHeaderId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<PartnerPaymentAllocation>(entity =>
        {
            entity.HasIndex(a => new { a.TenantId, a.BranchId });
            entity.Property(a => a.AllocatedAmount).HasColumnType("decimal(18,2)");

            entity.HasOne(a => a.InvoiceHeader).WithMany().HasForeignKey(a => a.InvoiceHeaderId).OnDelete(DeleteBehavior.Restrict);
        });

        ApplyTenantBranchQueryFilters(modelBuilder);
    }

    private void ApplyTenantBranchQueryFilters(ModelBuilder modelBuilder)
    {
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            var clrType = entityType.ClrType;

            if (typeof(IBranchScoped).IsAssignableFrom(clrType))
            {
                typeof(AppDbContext)
                    .GetMethod(nameof(SetBranchScopedFilter), BindingFlags.NonPublic | BindingFlags.Instance)!
                    .MakeGenericMethod(clrType)
                    .Invoke(this, [modelBuilder]);
            }
            else if (typeof(ITenantScoped).IsAssignableFrom(clrType))
            {
                typeof(AppDbContext)
                    .GetMethod(nameof(SetTenantScopedFilter), BindingFlags.NonPublic | BindingFlags.Instance)!
                    .MakeGenericMethod(clrType)
                    .Invoke(this, [modelBuilder]);
            }
        }
    }

    private void SetTenantScopedFilter<T>(ModelBuilder modelBuilder) where T : class, ITenantScoped =>
        modelBuilder.Entity<T>().HasQueryFilter(e => e.TenantId == _tenantContext.TenantId);

    private void SetBranchScopedFilter<T>(ModelBuilder modelBuilder) where T : class, IBranchScoped =>
        modelBuilder.Entity<T>().HasQueryFilter(e =>
            e.TenantId == _tenantContext.TenantId &&
            (_tenantContext.BranchId == null || e.BranchId == _tenantContext.BranchId));

    public override int SaveChanges()
    {
        ApplyTenantBranchStamping();
        return base.SaveChanges();
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        ApplyTenantBranchStamping();
        return await base.SaveChangesAsync(cancellationToken);
    }

    private void ApplyTenantBranchStamping()
    {
        foreach (var entry in ChangeTracker.Entries())
        {
            if (entry.Entity is IBranchScoped branchScoped)
            {
                if (entry.State == EntityState.Added)
                {
                    if (branchScoped.TenantId == 0)
                    {
                        branchScoped.TenantId = _tenantContext.TenantId;
                    }

                    if (branchScoped.BranchId == 0)
                    {
                        if (!_tenantContext.IsBranchResolved)
                        {
                            throw new InvalidOperationException(
                                $"Cannot create a {entry.Entity.GetType().Name} without a resolved branch context.");
                        }

                        branchScoped.BranchId = _tenantContext.BranchId!.Value;
                    }
                }
                else if (entry.State == EntityState.Modified)
                {
                    GuardUnchanged(entry, nameof(IBranchScoped.TenantId));
                    GuardUnchanged(entry, nameof(IBranchScoped.BranchId));
                }
            }
            else if (entry.Entity is ITenantScoped tenantScoped)
            {
                if (entry.State == EntityState.Added && tenantScoped.TenantId == 0)
                {
                    tenantScoped.TenantId = _tenantContext.TenantId;
                }
                else if (entry.State == EntityState.Modified)
                {
                    GuardUnchanged(entry, nameof(ITenantScoped.TenantId));
                }
            }
        }
    }

    private static void GuardUnchanged(Microsoft.EntityFrameworkCore.ChangeTracking.EntityEntry entry, string propertyName)
    {
        var property = entry.Property(propertyName);
        if (!Equals(property.OriginalValue, property.CurrentValue))
        {
            throw new InvalidOperationException(
                $"{propertyName} cannot be changed on an existing {entry.Entity.GetType().Name}.");
        }
    }
}
