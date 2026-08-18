using erp_backend.Models;

namespace erp_backend.Tenants;

public interface ITenantDefaultsProvisioner
{
    /// <summary>
    /// Seeds a brand-new tenant's default Chart of Accounts (one control account per AccountType),
    /// the tenant-wide StockAccountMapping those accounts back, and a default "Walk-in Customer"
    /// partner, so the tenant is immediately usable (vouchers/invoices postable) without a manual
    /// setup step. See TenantDefaultsProvisioner for why this bypasses the usual
    /// repository/resolver classes instead of calling them directly.
    /// </summary>
    Task ProvisionAsync(Tenant tenant);
}
