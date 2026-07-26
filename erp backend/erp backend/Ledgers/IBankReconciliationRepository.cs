using erp_backend.Ledgers.Dtos;

namespace erp_backend.Ledgers;

public interface IBankReconciliationRepository
{
    Task<List<BankStatementLineResponse>> GetLinesAsync(int accountId, bool? matched);
    Task<int> ImportStatementAsync(int accountId, IFormFile file);
    Task<(int MatchedCount, int Remaining)> AutoMatchAsync(int accountId);
    Task ManualMatchAsync(ManualMatchRequest request);
    Task UnmatchAsync(int bankStatementLineId);
}
