using erp_backend.Data;
using erp_backend.Exceptions;
using erp_backend.Ledgers.Dtos;
using erp_backend.Messages;
using erp_backend.Models;
using Microsoft.EntityFrameworkCore;

namespace erp_backend.Ledgers;

public class LedgerRepository : ILedgerRepository
{
    private readonly AppDbContext _context;

    public LedgerRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<GeneralLedgerEntryResponse>> GetGeneralLedgerAsync(
        DateTime? from, DateTime? to, VoucherType? voucherType)
    {
        var query = _context.VoucherDetails
            .Include(d => d.Voucher)
            .Include(d => d.Account)
            .Where(d => d.Voucher!.Status == VoucherStatus.Posted);

        if (from is not null) query = query.Where(d => d.Voucher!.Date >= from);
        if (to is not null) query = query.Where(d => d.Voucher!.Date <= to);
        if (voucherType is not null) query = query.Where(d => d.Voucher!.VoucherType == voucherType);

        return await query
            .OrderBy(d => d.Voucher!.Date).ThenBy(d => d.VoucherId)
            .Select(d => new GeneralLedgerEntryResponse
            {
                VoucherId = d.VoucherId,
                VoucherNo = d.Voucher!.VoucherNo,
                VoucherType = d.Voucher!.VoucherType,
                Date = d.Voucher!.Date,
                Narration = d.Voucher!.Narration,
                AccountId = d.AccountId,
                AccountCode = d.Account!.Code,
                AccountName = d.Account!.Name,
                DebitAmount = d.DebitAmount,
                CreditAmount = d.CreditAmount,
            })
            .ToListAsync();
    }

    public async Task<AccountLedgerResponse> GetAccountLedgerAsync(int accountId, DateTime? from, DateTime? to)
    {
        var account = await _context.Accounts.FindAsync(accountId) ?? throw new NotFoundException(ResponseMessage.AccountNotFound);

        var effectiveFrom = from ?? DateTime.MinValue;
        var effectiveTo = to ?? DateTime.MaxValue;

        var priorDetails = await _context.VoucherDetails
            .Include(d => d.Voucher)
            .Where(d => d.AccountId == accountId && d.Voucher!.Status == VoucherStatus.Posted && d.Voucher!.Date < effectiveFrom)
            .ToListAsync();

        var openingSigned = BalanceMath.SignedOpeningBalance(account) +
            priorDetails.Sum(d => BalanceMath.SignedDelta(account.Nature, d.DebitAmount, d.CreditAmount));

        var periodDetails = await _context.VoucherDetails
            .Include(d => d.Voucher)
            .Where(d => d.AccountId == accountId && d.Voucher!.Status == VoucherStatus.Posted
                && d.Voucher!.Date >= effectiveFrom && d.Voucher!.Date <= effectiveTo)
            .OrderBy(d => d.Voucher!.Date).ThenBy(d => d.VoucherId)
            .ToListAsync();

        var running = openingSigned;
        var entries = new List<AccountLedgerEntryResponse>();

        foreach (var detail in periodDetails)
        {
            running += BalanceMath.SignedDelta(account.Nature, detail.DebitAmount, detail.CreditAmount);
            var (amount, nature) = BalanceMath.ToDisplayBalance(running, account.Nature);

            entries.Add(new AccountLedgerEntryResponse
            {
                VoucherId = detail.VoucherId,
                VoucherNo = detail.Voucher!.VoucherNo,
                Date = detail.Voucher!.Date,
                Narration = detail.Voucher!.Narration,
                DebitAmount = detail.DebitAmount,
                CreditAmount = detail.CreditAmount,
                RunningBalance = amount,
                RunningBalanceNature = nature,
            });
        }

        var (openingAmount, openingNature) = BalanceMath.ToDisplayBalance(openingSigned, account.Nature);
        var (closingAmount, closingNature) = BalanceMath.ToDisplayBalance(running, account.Nature);

        return new AccountLedgerResponse
        {
            AccountId = account.Id,
            AccountCode = account.Code,
            AccountName = account.Name,
            OpeningBalance = openingAmount,
            OpeningBalanceNature = openingNature,
            ClosingBalance = closingAmount,
            ClosingBalanceNature = closingNature,
            Entries = entries,
        };
    }
}
