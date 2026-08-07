using erp_backend.Data;
using erp_backend.Exceptions;
using erp_backend.Inventory;
using erp_backend.Messages;
using erp_backend.Models;

namespace erp_backend.Partners;

/// <summary>
/// Provisions each customer's/vendor's own sub-ledger Account (a child of the tenant's Debtors/
/// Creditors control account, per FR-COA-05/FR-LED-05) so AR/AP postings and the sub-ledger/account-
/// ledger screens can be scoped to one partner instead of the shared control account. Provisioning is
/// lazy: called eagerly from BusinessPartnerRepository on create/update, and defensively from the
/// invoice/partner-payment posting sites, rather than backfilled by migration — both existing partners
/// and StockAccountMapping setup can predate this feature.
/// </summary>
public class PartnerLedgerAccountResolver : IPartnerLedgerAccountResolver
{
    private readonly AppDbContext _context;
    private readonly IStockAccountMappingRepository _stockAccountMappings;

    public PartnerLedgerAccountResolver(AppDbContext context, IStockAccountMappingRepository stockAccountMappings)
    {
        _context = context;
        _stockAccountMappings = stockAccountMappings;
    }

    public async Task EnsureLedgerAccountsAsync(BusinessPartner partner)
    {
        if (partner.PartnerType is PartnerType.Customer or PartnerType.Both)
        {
            await GetOrCreateReceivableAccountIdAsync(partner);
        }

        if (partner.PartnerType is PartnerType.Supplier or PartnerType.Both)
        {
            await GetOrCreatePayableAccountIdAsync(partner);
        }
    }

    public async Task<int> GetOrCreateReceivableAccountIdAsync(BusinessPartner partner)
    {
        if (partner.ReceivableAccountId is int existingId)
        {
            return existingId;
        }

        var controlMapping = await _stockAccountMappings.ResolveForCategoryAsync(null);
        var controlAccount = await _context.Accounts.FindAsync(controlMapping.AccountsReceivableAccountId)
            ?? throw new BadRequestException(ResponseMessage.ParentAccountNotFound);

        partner.ReceivableAccount = BuildSubAccount(partner, controlAccount);
        _context.Accounts.Add(partner.ReceivableAccount);
        await _context.SaveChangesAsync();

        return partner.ReceivableAccountId!.Value;
    }

    public async Task<int> GetOrCreatePayableAccountIdAsync(BusinessPartner partner)
    {
        if (partner.PayableAccountId is int existingId)
        {
            return existingId;
        }

        var controlMapping = await _stockAccountMappings.ResolveForCategoryAsync(null);
        var controlAccount = await _context.Accounts.FindAsync(controlMapping.AccountsPayableAccountId)
            ?? throw new BadRequestException(ResponseMessage.ParentAccountNotFound);

        partner.PayableAccount = BuildSubAccount(partner, controlAccount);
        _context.Accounts.Add(partner.PayableAccount);
        await _context.SaveChangesAsync();

        return partner.PayableAccountId!.Value;
    }

    private static Account BuildSubAccount(BusinessPartner partner, Account controlAccount) => new()
    {
        Code = $"{controlAccount.Code}-{partner.Code}",
        Name = partner.Name,
        Type = controlAccount.Type,
        Nature = controlAccount.Nature,
        ParentAccountId = controlAccount.Id,
        IsControlAccount = false,
    };
}
