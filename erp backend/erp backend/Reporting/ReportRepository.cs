using erp_backend.Data;
using erp_backend.Exceptions;
using erp_backend.Ledgers;
using erp_backend.Messages;
using erp_backend.Models;
using erp_backend.Reporting.Dtos;
using Microsoft.EntityFrameworkCore;

namespace erp_backend.Reporting;

public class ReportRepository : IReportRepository
{
    private readonly AppDbContext _context;
    private readonly ISubLedgerRepository _subLedgers;
    private readonly IReportExporter _exporter;

    public ReportRepository(AppDbContext context, ISubLedgerRepository subLedgers, IReportExporter exporter)
    {
        _context = context;
        _subLedgers = subLedgers;
        _exporter = exporter;
    }

    public async Task<TrialBalanceResponse> GetTrialBalanceAsync(DateTime asOf, int? costCenterId)
    {
        var accounts = await _context.Accounts.ToListAsync();
        var rows = new List<TrialBalanceRowResponse>();

        foreach (var account in accounts)
        {
            var query = _context.VoucherDetails
                .Include(d => d.Voucher)
                .Where(d => d.AccountId == account.Id && d.Voucher!.Status == VoucherStatus.Posted && d.Voucher!.Date <= asOf);

            if (costCenterId is int cc) query = query.Where(d => d.CostCenterId == cc);

            var details = await query.ToListAsync();

            // When filtering by cost center, opening balances (which aren't cost-center-tagged) are excluded.
            var signed = (costCenterId is null ? BalanceMath.SignedOpeningBalance(account) : 0m) +
                details.Sum(d => BalanceMath.SignedDelta(account.Nature, d.DebitAmount, d.CreditAmount));

            if (signed == 0)
            {
                continue;
            }

            var (amount, nature) = BalanceMath.ToDisplayBalance(signed, account.Nature);

            rows.Add(new TrialBalanceRowResponse
            {
                AccountId = account.Id,
                AccountCode = account.Code,
                AccountName = account.Name,
                Debit = nature == AccountNature.Debit ? amount : 0,
                Credit = nature == AccountNature.Credit ? amount : 0,
            });
        }

        var totalDebit = rows.Sum(r => r.Debit);
        var totalCredit = rows.Sum(r => r.Credit);

        return new TrialBalanceResponse
        {
            AsOf = asOf,
            Rows = rows.OrderBy(r => r.AccountCode).ToList(),
            TotalDebit = totalDebit,
            TotalCredit = totalCredit,
            IsBalanced = totalDebit == totalCredit,
        };
    }

    public async Task<ProfitLossResponse> GetProfitLossAsync(DateTime from, DateTime to, bool compare, int? costCenterId)
    {
        async Task<(List<ProfitLossRowResponse> Income, List<ProfitLossRowResponse> Expenses, decimal TotalIncome, decimal TotalExpenses)>
            ComputeAsync(DateTime periodFrom, DateTime periodTo)
        {
            var accounts = await _context.Accounts.Where(a => a.Type == AccountType.Income || a.Type == AccountType.Expense).ToListAsync();
            var income = new List<ProfitLossRowResponse>();
            var expenses = new List<ProfitLossRowResponse>();

            foreach (var account in accounts)
            {
                var query = _context.VoucherDetails
                    .Include(d => d.Voucher)
                    .Where(d => d.AccountId == account.Id && d.Voucher!.Status == VoucherStatus.Posted
                        && d.Voucher!.Date >= periodFrom && d.Voucher!.Date <= periodTo);

                if (costCenterId is int cc) query = query.Where(d => d.CostCenterId == cc);

                var details = await query.ToListAsync();
                var amount = details.Sum(d => BalanceMath.SignedDelta(account.Nature, d.DebitAmount, d.CreditAmount));

                if (amount == 0)
                {
                    continue;
                }

                var row = new ProfitLossRowResponse
                {
                    AccountId = account.Id,
                    AccountCode = account.Code,
                    AccountName = account.Name,
                    Amount = amount,
                };

                if (account.Type == AccountType.Income) income.Add(row); else expenses.Add(row);
            }

            return (income, expenses, income.Sum(r => r.Amount), expenses.Sum(r => r.Amount));
        }

        var (income, expenses, totalIncome, totalExpenses) = await ComputeAsync(from, to);

        var response = new ProfitLossResponse
        {
            From = from,
            To = to,
            Income = income.OrderBy(r => r.AccountCode).ToList(),
            Expenses = expenses.OrderBy(r => r.AccountCode).ToList(),
            TotalIncome = totalIncome,
            TotalExpenses = totalExpenses,
            NetProfit = totalIncome - totalExpenses,
        };

        if (compare)
        {
            var periodLength = to - from;
            var priorTo = from.AddDays(-1);
            var priorFrom = priorTo - periodLength;

            var (priorIncome, priorExpenses, priorTotalIncome, priorTotalExpenses) = await ComputeAsync(priorFrom, priorTo);

            foreach (var row in response.Income)
            {
                row.PriorAmount = priorIncome.FirstOrDefault(p => p.AccountId == row.AccountId)?.Amount ?? 0;
            }

            foreach (var row in response.Expenses)
            {
                row.PriorAmount = priorExpenses.FirstOrDefault(p => p.AccountId == row.AccountId)?.Amount ?? 0;
            }

            response.PriorTotalIncome = priorTotalIncome;
            response.PriorTotalExpenses = priorTotalExpenses;
            response.PriorNetProfit = priorTotalIncome - priorTotalExpenses;
        }

        return response;
    }

    public async Task<BalanceSheetResponse> GetBalanceSheetAsync(DateTime asOf, int? costCenterId)
    {
        var accounts = await _context.Accounts
            .Where(a => a.Type == AccountType.Asset || a.Type == AccountType.Liability || a.Type == AccountType.Equity)
            .ToListAsync();

        var assets = new List<BalanceSheetRowResponse>();
        var liabilities = new List<BalanceSheetRowResponse>();
        var equity = new List<BalanceSheetRowResponse>();

        foreach (var account in accounts)
        {
            var query = _context.VoucherDetails
                .Include(d => d.Voucher)
                .Where(d => d.AccountId == account.Id && d.Voucher!.Status == VoucherStatus.Posted && d.Voucher!.Date <= asOf);

            if (costCenterId is int cc) query = query.Where(d => d.CostCenterId == cc);

            var details = await query.ToListAsync();

            var signed = (costCenterId is null ? BalanceMath.SignedOpeningBalance(account) : 0m) +
                details.Sum(d => BalanceMath.SignedDelta(account.Nature, d.DebitAmount, d.CreditAmount));

            if (signed == 0)
            {
                continue;
            }

            var (amount, _) = BalanceMath.ToDisplayBalance(signed, account.Nature);
            var row = new BalanceSheetRowResponse
            {
                AccountId = account.Id,
                AccountCode = account.Code,
                AccountName = account.Name,
                Amount = amount,
            };

            switch (account.Type)
            {
                case AccountType.Asset: assets.Add(row); break;
                case AccountType.Liability: liabilities.Add(row); break;
                default: equity.Add(row); break;
            }
        }

        var totalAssets = assets.Sum(r => r.Amount);
        var totalLiabilities = liabilities.Sum(r => r.Amount);
        var totalEquity = equity.Sum(r => r.Amount);

        return new BalanceSheetResponse
        {
            AsOf = asOf,
            Assets = assets.OrderBy(r => r.AccountCode).ToList(),
            Liabilities = liabilities.OrderBy(r => r.AccountCode).ToList(),
            Equity = equity.OrderBy(r => r.AccountCode).ToList(),
            TotalAssets = totalAssets,
            TotalLiabilities = totalLiabilities,
            TotalEquity = totalEquity,
            IsBalanced = totalAssets == totalLiabilities + totalEquity,
        };
    }

    public async Task<CashFlowResponse> GetCashFlowAsync(DateTime from, DateTime to)
    {
        var cashAccountIds = (await _context.Accounts.Where(a => a.IsCashAccount).Select(a => a.Id).ToListAsync()).ToHashSet();

        var response = new CashFlowResponse { From = from, To = to };

        if (cashAccountIds.Count == 0)
        {
            return response;
        }

        var vouchers = await _context.VoucherHeaders
            .Include(v => v.Details).ThenInclude(d => d.Account)
            .Where(v => v.Status == VoucherStatus.Posted && v.Date >= from && v.Date <= to)
            .Where(v => v.Details.Any(d => cashAccountIds.Contains(d.AccountId)))
            .ToListAsync();

        foreach (var voucher in vouchers)
        {
            var cashLines = voucher.Details.Where(d => cashAccountIds.Contains(d.AccountId)).ToList();
            var contraLines = voucher.Details.Where(d => !cashAccountIds.Contains(d.AccountId)).ToList();

            if (cashLines.Count == 0 || contraLines.Count == 0)
            {
                continue;
            }

            // Simplification: a voucher's net cash movement is attributed to the type of its first
            // non-cash contra line. Multi-leg vouchers spanning several categories are not split proportionally.
            var cashNetSigned = cashLines.Sum(d => BalanceMath.SignedDelta(AccountNature.Debit, d.DebitAmount, d.CreditAmount));
            var contraAccount = contraLines[0].Account!;

            var category = contraAccount.Type switch
            {
                AccountType.Income or AccountType.Expense => "Operating",
                AccountType.Liability or AccountType.Equity => "Financing",
                _ => "Investing",
            };

            response.Lines.Add(new CashFlowLineResponse
            {
                Category = category,
                AccountId = contraAccount.Id,
                AccountCode = contraAccount.Code,
                AccountName = contraAccount.Name,
                Amount = cashNetSigned,
            });
        }

        response.OperatingActivities = response.Lines.Where(l => l.Category == "Operating").Sum(l => l.Amount);
        response.InvestingActivities = response.Lines.Where(l => l.Category == "Investing").Sum(l => l.Amount);
        response.FinancingActivities = response.Lines.Where(l => l.Category == "Financing").Sum(l => l.Amount);
        response.NetCashFlow = response.OperatingActivities + response.InvestingActivities + response.FinancingActivities;

        return response;
    }

    public async Task<List<DayBookEntryResponse>> GetDayBookAsync(DateTime from, DateTime to)
    {
        var vouchers = await _context.VoucherHeaders
            .Include(v => v.Details).ThenInclude(d => d.Account)
            .Where(v => v.Status == VoucherStatus.Posted && v.Date >= from && v.Date <= to)
            .OrderBy(v => v.Date).ThenBy(v => v.VoucherNo)
            .ToListAsync();

        return vouchers.Select(v => new DayBookEntryResponse
        {
            VoucherId = v.Id,
            VoucherNo = v.VoucherNo,
            VoucherType = v.VoucherType,
            Date = v.Date,
            Narration = v.Narration,
            Lines = v.Details.Select(d => new DayBookLineResponse
            {
                AccountCode = d.Account!.Code,
                AccountName = d.Account!.Name,
                DebitAmount = d.DebitAmount,
                CreditAmount = d.CreditAmount,
            }).ToList(),
        }).ToList();
    }

    public async Task<List<BudgetVsActualRowResponse>> GetBudgetVsActualAsync(int year, int month)
    {
        var budgets = await _context.Budgets
            .Include(b => b.Account)
            .Where(b => b.Year == year && b.Month == month)
            .ToListAsync();

        var monthStart = new DateTime(year, month, 1);
        var monthEnd = monthStart.AddMonths(1).AddDays(-1);

        var rows = new List<BudgetVsActualRowResponse>();

        foreach (var budget in budgets)
        {
            var account = budget.Account!;
            var details = await _context.VoucherDetails
                .Include(d => d.Voucher)
                .Where(d => d.AccountId == account.Id && d.Voucher!.Status == VoucherStatus.Posted
                    && d.Voucher!.Date >= monthStart && d.Voucher!.Date <= monthEnd)
                .ToListAsync();

            var actual = details.Sum(d => BalanceMath.SignedDelta(account.Nature, d.DebitAmount, d.CreditAmount));

            rows.Add(new BudgetVsActualRowResponse
            {
                AccountId = account.Id,
                AccountCode = account.Code,
                AccountName = account.Name,
                Budgeted = budget.BudgetedAmount,
                Actual = actual,
                Variance = actual - budget.BudgetedAmount,
            });
        }

        return rows.OrderBy(r => r.AccountCode).ToList();
    }

    public async Task<(byte[] Bytes, string ContentType, string FileName)> ExportAsync(
        string type, string format, DateTime? asOf, DateTime? from, DateTime? to, bool compare,
        int? costCenterId, string? subLedgerType, int? controlAccountId, int? year, int? month)
    {
        if (format is not ("pdf" or "excel"))
        {
            throw new BadRequestException(ResponseMessage.ExportFormatInvalid);
        }

        var (title, columns, rows) = await BuildExportDataAsync(
            type, asOf, from, to, compare, costCenterId, subLedgerType, controlAccountId, year, month);

        if (columns is null)
        {
            throw new BadRequestException(ResponseMessage.ReportTypeUnknown, type);
        }

        var bytes = format == "pdf" ? _exporter.ToPdf(title, columns, rows!) : _exporter.ToExcel(title, columns, rows!);
        var contentType = format == "pdf" ? "application/pdf" : "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
        var fileName = $"{type}.{(format == "pdf" ? "pdf" : "xlsx")}";

        return (bytes, contentType, fileName);
    }

    private async Task<(string Title, List<string>? Columns, List<IReadOnlyList<string>>? Rows)> BuildExportDataAsync(
        string type, DateTime? asOf, DateTime? from, DateTime? to, bool compare, int? costCenterId,
        string? subLedgerType, int? controlAccountId, int? year, int? month)
    {
        switch (type)
        {
            case "trial-balance":
            {
                var data = await GetTrialBalanceAsync(asOf ?? DateTime.UtcNow, costCenterId);
                var columns = new List<string> { "Code", "Account", "Debit", "Credit" };
                var rows = data.Rows.Select(r => (IReadOnlyList<string>)new List<string>
                {
                    r.AccountCode, r.AccountName, r.Debit.ToString("0.00"), r.Credit.ToString("0.00"),
                }).ToList();
                rows.Add(new List<string> { "", "TOTAL", data.TotalDebit.ToString("0.00"), data.TotalCredit.ToString("0.00") });
                return ("Trial Balance", columns, rows);
            }
            case "profit-loss":
            {
                var data = await GetProfitLossAsync(from!.Value, to!.Value, compare, costCenterId);
                var columns = new List<string> { "Section", "Code", "Account", "Amount" };
                var rows = new List<IReadOnlyList<string>>();
                rows.AddRange(data.Income.Select(r => (IReadOnlyList<string>)new List<string> { "Income", r.AccountCode, r.AccountName, r.Amount.ToString("0.00") }));
                rows.AddRange(data.Expenses.Select(r => (IReadOnlyList<string>)new List<string> { "Expense", r.AccountCode, r.AccountName, r.Amount.ToString("0.00") }));
                rows.Add(new List<string> { "", "", "Net Profit", data.NetProfit.ToString("0.00") });
                return ("Profit & Loss", columns, rows);
            }
            case "balance-sheet":
            {
                var data = await GetBalanceSheetAsync(asOf ?? DateTime.UtcNow, costCenterId);
                var columns = new List<string> { "Section", "Code", "Account", "Amount" };
                var rows = new List<IReadOnlyList<string>>();
                rows.AddRange(data.Assets.Select(r => (IReadOnlyList<string>)new List<string> { "Asset", r.AccountCode, r.AccountName, r.Amount.ToString("0.00") }));
                rows.AddRange(data.Liabilities.Select(r => (IReadOnlyList<string>)new List<string> { "Liability", r.AccountCode, r.AccountName, r.Amount.ToString("0.00") }));
                rows.AddRange(data.Equity.Select(r => (IReadOnlyList<string>)new List<string> { "Equity", r.AccountCode, r.AccountName, r.Amount.ToString("0.00") }));
                return ("Balance Sheet", columns, rows);
            }
            case "cash-flow":
            {
                var data = await GetCashFlowAsync(from!.Value, to!.Value);
                var columns = new List<string> { "Category", "Code", "Account", "Amount" };
                var rows = data.Lines.Select(l => (IReadOnlyList<string>)new List<string>
                {
                    l.Category, l.AccountCode, l.AccountName, l.Amount.ToString("0.00"),
                }).ToList();
                rows.Add(new List<string> { "", "", "Net Cash Flow", data.NetCashFlow.ToString("0.00") });
                return ("Cash Flow Statement", columns, rows);
            }
            case "aging":
            {
                var results = await _subLedgers.GetAgingAsync(subLedgerType ?? "Receivable", asOf ?? DateTime.UtcNow, controlAccountId);
                var columns = new List<string> { "Code", "Account", "Outstanding", "Age (days)", "Bucket" };
                var rows = results.Select(r => (IReadOnlyList<string>)new List<string>
                {
                    r.AccountCode, r.AccountName, r.OutstandingBalance.ToString("0.00"), r.AgeInDays.ToString(), r.AgeBucket,
                }).ToList();
                return ($"{subLedgerType ?? "Receivable"} Aging", columns, rows);
            }
            case "day-book":
            {
                var data = await GetDayBookAsync(from!.Value, to!.Value);
                var columns = new List<string> { "Voucher No", "Type", "Date", "Account", "Debit", "Credit" };
                var rows = new List<IReadOnlyList<string>>();
                foreach (var voucher in data)
                {
                    foreach (var line in voucher.Lines)
                    {
                        rows.Add(new List<string>
                        {
                            voucher.VoucherNo, voucher.VoucherType.ToString(), voucher.Date.ToString("yyyy-MM-dd"),
                            line.AccountName, line.DebitAmount.ToString("0.00"), line.CreditAmount.ToString("0.00"),
                        });
                    }
                }
                return ("Day Book", columns, rows);
            }
            case "budget-vs-actual":
            {
                var data = await GetBudgetVsActualAsync(year!.Value, month!.Value);
                var columns = new List<string> { "Code", "Account", "Budgeted", "Actual", "Variance" };
                var rows = data.Select(r => (IReadOnlyList<string>)new List<string>
                {
                    r.AccountCode, r.AccountName, r.Budgeted.ToString("0.00"), r.Actual.ToString("0.00"), r.Variance.ToString("0.00"),
                }).ToList();
                return ("Budget vs Actual", columns, rows);
            }
            default:
                return (type, null, null);
        }
    }
}
