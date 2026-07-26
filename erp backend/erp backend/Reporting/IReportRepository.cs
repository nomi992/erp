using erp_backend.Reporting.Dtos;

namespace erp_backend.Reporting;

public interface IReportRepository
{
    Task<TrialBalanceResponse> GetTrialBalanceAsync(DateTime asOf, int? costCenterId);
    Task<ProfitLossResponse> GetProfitLossAsync(DateTime from, DateTime to, bool compare, int? costCenterId);
    Task<BalanceSheetResponse> GetBalanceSheetAsync(DateTime asOf, int? costCenterId);
    Task<CashFlowResponse> GetCashFlowAsync(DateTime from, DateTime to);
    Task<List<DayBookEntryResponse>> GetDayBookAsync(DateTime from, DateTime to);
    Task<List<BudgetVsActualRowResponse>> GetBudgetVsActualAsync(int year, int month);

    Task<(byte[] Bytes, string ContentType, string FileName)> ExportAsync(
        string type, string format, DateTime? asOf, DateTime? from, DateTime? to, bool compare,
        int? costCenterId, string? subLedgerType, int? controlAccountId, int? year, int? month);
}
