using erp_backend.Models;

namespace erp_backend.Reporting.Dtos;

public class TrialBalanceRowResponse
{
    public int AccountId { get; set; }
    public string AccountCode { get; set; } = string.Empty;
    public string AccountName { get; set; } = string.Empty;
    public decimal Debit { get; set; }
    public decimal Credit { get; set; }
}

public class TrialBalanceResponse
{
    public DateTime AsOf { get; set; }
    public List<TrialBalanceRowResponse> Rows { get; set; } = [];
    public decimal TotalDebit { get; set; }
    public decimal TotalCredit { get; set; }
    public bool IsBalanced { get; set; }
}

public class ProfitLossRowResponse
{
    public int AccountId { get; set; }
    public string AccountCode { get; set; } = string.Empty;
    public string AccountName { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public decimal? PriorAmount { get; set; }
}

public class ProfitLossResponse
{
    public DateTime From { get; set; }
    public DateTime To { get; set; }
    public List<ProfitLossRowResponse> Income { get; set; } = [];
    public List<ProfitLossRowResponse> Expenses { get; set; } = [];
    public decimal TotalIncome { get; set; }
    public decimal TotalExpenses { get; set; }
    public decimal NetProfit { get; set; }
    public decimal? PriorTotalIncome { get; set; }
    public decimal? PriorTotalExpenses { get; set; }
    public decimal? PriorNetProfit { get; set; }
}

public class BalanceSheetRowResponse
{
    public int AccountId { get; set; }
    public string AccountCode { get; set; } = string.Empty;
    public string AccountName { get; set; } = string.Empty;
    public decimal Amount { get; set; }
}

public class BalanceSheetResponse
{
    public DateTime AsOf { get; set; }
    public List<BalanceSheetRowResponse> Assets { get; set; } = [];
    public List<BalanceSheetRowResponse> Liabilities { get; set; } = [];
    public List<BalanceSheetRowResponse> Equity { get; set; } = [];
    public decimal TotalAssets { get; set; }
    public decimal TotalLiabilities { get; set; }
    public decimal TotalEquity { get; set; }
    public bool IsBalanced { get; set; }
}

public class CashFlowLineResponse
{
    public string Category { get; set; } = string.Empty;
    public int AccountId { get; set; }
    public string AccountCode { get; set; } = string.Empty;
    public string AccountName { get; set; } = string.Empty;
    public decimal Amount { get; set; }
}

public class CashFlowResponse
{
    public DateTime From { get; set; }
    public DateTime To { get; set; }
    public decimal OperatingActivities { get; set; }
    public decimal InvestingActivities { get; set; }
    public decimal FinancingActivities { get; set; }
    public decimal NetCashFlow { get; set; }
    public List<CashFlowLineResponse> Lines { get; set; } = [];
}

public class DayBookLineResponse
{
    public string AccountCode { get; set; } = string.Empty;
    public string AccountName { get; set; } = string.Empty;
    public decimal DebitAmount { get; set; }
    public decimal CreditAmount { get; set; }
}

public class DayBookEntryResponse
{
    public int VoucherId { get; set; }
    public string VoucherNo { get; set; } = string.Empty;
    public VoucherType VoucherType { get; set; }
    public DateTime Date { get; set; }
    public string Narration { get; set; } = string.Empty;
    public List<DayBookLineResponse> Lines { get; set; } = [];
}

public class BudgetVsActualRowResponse
{
    public int AccountId { get; set; }
    public string AccountCode { get; set; } = string.Empty;
    public string AccountName { get; set; } = string.Empty;
    public decimal Budgeted { get; set; }
    public decimal Actual { get; set; }
    public decimal Variance { get; set; }
}
