using erp_backend.Models;

namespace erp_backend.Partners;

public interface IPartnerLedgerAccountResolver
{
    /// <summary>
    /// Provisions whichever sub-ledger account(s) the partner's PartnerType requires (Receivable for
    /// Customer, Payable for Supplier, both for Both) that it doesn't already have. No-op for sides
    /// that already have an account.
    /// </summary>
    Task EnsureLedgerAccountsAsync(BusinessPartner partner);

    /// <summary>Returns the partner's Receivable sub-account id, provisioning it first if missing.</summary>
    Task<int> GetOrCreateReceivableAccountIdAsync(BusinessPartner partner);

    /// <summary>Returns the partner's Payable sub-account id, provisioning it first if missing.</summary>
    Task<int> GetOrCreatePayableAccountIdAsync(BusinessPartner partner);
}
