using erp_backend.Ledgers.Dtos;

namespace erp_backend.Ledgers;

public interface ISubLedgerRepository
{
    Task<List<SubLedgerEntryResponse>> GetAgingAsync(string type, DateTime asOf, int? controlAccountId);
}
