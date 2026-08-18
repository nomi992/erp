using erp_backend.Models.Abstractions;

namespace erp_backend.Models;

public enum PartnerType
{
    Supplier,
    Customer,
    Both,
}

public class BusinessPartner : ITenantScoped
{
    public int Id { get; set; }
    public int TenantId { get; set; }
    public PartnerType PartnerType { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? ContactPerson { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public string? Address { get; set; }
    public int DefaultPaymentTermDays { get; set; }
    public decimal? CreditLimit { get; set; }
    public bool IsActive { get; set; } = true;

    // At most one default Customer-ish and one default Supplier-ish partner per tenant (enforced in
    // BusinessPartnerRepository, mirroring Warehouse.IsDefault). The default customer is what invoice
    // entry pre-selects, so walk-in/counter sales don't need a partner picked every time.
    public bool IsDefault { get; set; }
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

    // Sub-ledger accounts (children of the Debtors/Creditors control account) that AR/AP postings
    // for this partner's invoices and payments hit, instead of the shared tenant-wide control account.
    // Provisioned lazily by IPartnerLedgerAccountResolver, not backfilled by migration.
    public int? ReceivableAccountId { get; set; }
    public Account? ReceivableAccount { get; set; }
    public int? PayableAccountId { get; set; }
    public Account? PayableAccount { get; set; }
}
