using erp_backend.Vouchers.Dtos;

namespace erp_backend.Vouchers;

public interface IRecurringVoucherTemplateRepository
{
    Task<List<RecurringVoucherTemplateResponse>> GetAllAsync();
    Task<RecurringVoucherTemplateResponse> GetByIdAsync(int id);
    Task<RecurringVoucherTemplateResponse> CreateAsync(RecurringVoucherTemplateRequest request);
    Task<RecurringVoucherTemplateResponse> UpdateAsync(int id, RecurringVoucherTemplateRequest request);
    Task ActivateAsync(int id);
    Task DeactivateAsync(int id);
    Task<(int VoucherId, string VoucherNo, DateTime NextRunDate)> GenerateNowAsync(int id, string username);
}
