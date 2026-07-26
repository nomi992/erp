using erp_backend.Accounts.Dtos;

namespace erp_backend.Accounts;

public interface IAccountRepository
{
    Task<List<AccountResponse>> GetAllAsync(bool? isActive);
    Task<List<AccountTreeNodeResponse>> GetTreeAsync();
    Task<AccountResponse> GetByIdAsync(int id);
    Task<AccountResponse> CreateAsync(CreateAccountRequest request);
    Task<AccountResponse> UpdateAsync(int id, UpdateAccountRequest request);
    Task ActivateAsync(int id);
    Task DeactivateAsync(int id);
    Task DeleteAsync(int id);
}
