using System.Globalization;
using erp_backend.Data;
using erp_backend.Exceptions;
using erp_backend.Ledgers.Dtos;
using erp_backend.Messages;
using erp_backend.Models;
using Microsoft.EntityFrameworkCore;

namespace erp_backend.Ledgers;

public class BankReconciliationRepository : IBankReconciliationRepository
{
    private readonly AppDbContext _context;

    public BankReconciliationRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<BankStatementLineResponse>> GetLinesAsync(int accountId, bool? matched)
    {
        var query = _context.BankStatementLines
            .Include(b => b.MatchedVoucherDetail).ThenInclude(d => d!.Voucher)
            .Where(b => b.BankAccountId == accountId);

        if (matched is not null) query = query.Where(b => b.IsMatched == matched);

        var lines = await query.OrderByDescending(b => b.TransactionDate).ToListAsync();
        return lines.Select(BankStatementLineResponse.FromEntity).ToList();
    }

    public async Task<int> ImportStatementAsync(int accountId, IFormFile file)
    {
        var account = await _context.Accounts.FindAsync(accountId) ?? throw new NotFoundException(ResponseMessage.BankAccountNotFound);

        using var reader = new StreamReader(file.OpenReadStream());
        var isHeaderRow = true;
        var imported = 0;

        while (await reader.ReadLineAsync() is { } line)
        {
            if (isHeaderRow)
            {
                isHeaderRow = false;
                continue;
            }

            if (string.IsNullOrWhiteSpace(line))
            {
                continue;
            }

            var columns = line.Split(',');
            if (columns.Length < 4)
            {
                continue;
            }

            if (!DateTime.TryParse(columns[0], CultureInfo.InvariantCulture, DateTimeStyles.None, out var transactionDate))
            {
                continue;
            }

            if (!decimal.TryParse(columns[3], NumberStyles.Any, CultureInfo.InvariantCulture, out var amount))
            {
                continue;
            }

            _context.BankStatementLines.Add(new BankStatementLine
            {
                BankAccountId = accountId,
                TransactionDate = transactionDate,
                Description = columns[1],
                ReferenceNo = columns[2],
                Amount = amount,
            });
            imported++;
        }

        await _context.SaveChangesAsync();

        return imported;
    }

    public async Task<(int MatchedCount, int Remaining)> AutoMatchAsync(int accountId)
    {
        var unmatchedLines = await _context.BankStatementLines
            .Where(b => b.BankAccountId == accountId && !b.IsMatched)
            .ToListAsync();

        var alreadyMatchedDetailIds = (await _context.BankStatementLines
            .Where(b => b.BankAccountId == accountId && b.IsMatched && b.MatchedVoucherDetailId != null)
            .Select(b => b.MatchedVoucherDetailId!.Value)
            .ToListAsync())
            .ToHashSet();

        var candidateDetails = await _context.VoucherDetails
            .Include(d => d.Voucher)
            .Where(d => d.AccountId == accountId && d.Voucher!.Status == VoucherStatus.Posted)
            .ToListAsync();

        var matchedCount = 0;

        foreach (var line in unmatchedLines)
        {
            var match = candidateDetails
                .Where(d => !alreadyMatchedDetailIds.Contains(d.Id))
                .Where(d => Math.Abs((d.Voucher!.Date.Date - line.TransactionDate.Date).Days) <= 3)
                .Where(d => line.Amount >= 0 ? d.DebitAmount == line.Amount : d.CreditAmount == -line.Amount)
                .OrderByDescending(d => !string.IsNullOrWhiteSpace(line.ReferenceNo) &&
                    d.Voucher!.Narration.Contains(line.ReferenceNo, StringComparison.OrdinalIgnoreCase))
                .FirstOrDefault();

            if (match is null)
            {
                continue;
            }

            line.IsMatched = true;
            line.MatchedVoucherDetailId = match.Id;
            alreadyMatchedDetailIds.Add(match.Id);
            matchedCount++;
        }

        await _context.SaveChangesAsync();

        return (matchedCount, unmatchedLines.Count - matchedCount);
    }

    public async Task ManualMatchAsync(ManualMatchRequest request)
    {
        var line = await _context.BankStatementLines.FindAsync(request.BankStatementLineId)
            ?? throw new NotFoundException(ResponseMessage.BankStatementLineNotFound);

        var detail = await _context.VoucherDetails.FindAsync(request.VoucherDetailId)
            ?? throw new NotFoundException(ResponseMessage.VoucherLineNotFound);

        line.IsMatched = true;
        line.MatchedVoucherDetailId = detail.Id;
        await _context.SaveChangesAsync();
    }

    public async Task UnmatchAsync(int bankStatementLineId)
    {
        var line = await _context.BankStatementLines.FindAsync(bankStatementLineId)
            ?? throw new NotFoundException(ResponseMessage.BankStatementLineNotFound);

        line.IsMatched = false;
        line.MatchedVoucherDetailId = null;
        await _context.SaveChangesAsync();
    }
}
