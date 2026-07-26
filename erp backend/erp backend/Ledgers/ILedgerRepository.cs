using erp_backend.Ledgers.Dtos;
using erp_backend.Models;

namespace erp_backend.Ledgers;

public interface ILedgerRepository
{
    Task<List<GeneralLedgerEntryResponse>> GetGeneralLedgerAsync(DateTime? from, DateTime? to, VoucherType? voucherType);
    Task<AccountLedgerResponse> GetAccountLedgerAsync(int accountId, DateTime? from, DateTime? to);
}
