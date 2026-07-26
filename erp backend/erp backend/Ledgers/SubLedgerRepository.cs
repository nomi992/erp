using erp_backend.Data;
using erp_backend.Exceptions;
using erp_backend.Ledgers.Dtos;
using erp_backend.Messages;
using erp_backend.Models;
using Microsoft.EntityFrameworkCore;

namespace erp_backend.Ledgers;

public class SubLedgerRepository : ISubLedgerRepository
{
    private readonly AppDbContext _context;

    public SubLedgerRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<SubLedgerEntryResponse>> GetAgingAsync(string type, DateTime asOf, int? controlAccountId)
    {
        if (type is not ("Receivable" or "Payable"))
        {
            throw new BadRequestException(ResponseMessage.SubLedgerTypeInvalid);
        }

        var controlAccount = controlAccountId is int id
            ? await _context.Accounts.FindAsync(id)
            : await FindControlAccountByConventionAsync(type);

        if (controlAccount is null)
        {
            var suffix = type == "Receivable" ? "'Debtor'/'Receivable'." : "'Creditor'/'Payable'.";
            throw new BadRequestException(ResponseMessage.SubLedgerControlAccountNotFound, type, suffix);
        }

        var subAccounts = await _context.Accounts
            .Where(a => a.ParentAccountId == controlAccount.Id && a.IsActive)
            .ToListAsync();

        var results = new List<SubLedgerEntryResponse>();

        foreach (var account in subAccounts)
        {
            var details = await _context.VoucherDetails
                .Include(d => d.Voucher)
                .Where(d => d.AccountId == account.Id && d.Voucher!.Status == VoucherStatus.Posted && d.Voucher!.Date <= asOf)
                .ToListAsync();

            if (details.Count == 0)
            {
                continue;
            }

            var signedBalance = BalanceMath.SignedOpeningBalance(account) +
                details.Sum(d => BalanceMath.SignedDelta(account.Nature, d.DebitAmount, d.CreditAmount));

            if (signedBalance == 0)
            {
                continue;
            }

            var (amount, nature) = BalanceMath.ToDisplayBalance(signedBalance, account.Nature);
            var lastActivity = details.Max(d => d.Voucher!.Date);
            var ageInDays = (asOf.Date - lastActivity.Date).Days;

            results.Add(new SubLedgerEntryResponse
            {
                AccountId = account.Id,
                AccountCode = account.Code,
                AccountName = account.Name,
                OutstandingBalance = amount,
                OutstandingBalanceNature = nature,
                LastActivityDate = lastActivity,
                AgeInDays = ageInDays,
                AgeBucket = ToAgeBucket(ageInDays),
            });
        }

        return results.OrderByDescending(r => r.AgeInDays).ToList();
    }

    private async Task<Account?> FindControlAccountByConventionAsync(string type)
    {
        var keywords = type == "Receivable"
            ? new[] { "Debtor", "Receivable" }
            : new[] { "Creditor", "Payable" };

        var controlAccounts = await _context.Accounts.Where(a => a.IsControlAccount).ToListAsync();
        return controlAccounts.FirstOrDefault(a => keywords.Any(k => a.Name.Contains(k, StringComparison.OrdinalIgnoreCase)));
    }

    private static string ToAgeBucket(int ageInDays) => ageInDays switch
    {
        <= 30 => "0-30",
        <= 60 => "31-60",
        <= 90 => "61-90",
        <= 120 => "91-120",
        _ => "120+",
    };
}
