using erp_backend.Branches.Dtos;

namespace erp_backend.Branches;

public interface IBranchRepository
{
    Task<List<BranchResponse>> GetAllAsync(int? tenantId);
    Task<BranchResponse> GetByIdAsync(int id);
    Task<BranchResponse> CreateAsync(CreateBranchRequest request);
    Task<BranchResponse> UpdateAsync(int id, UpdateBranchRequest request);
    Task ActivateAsync(int id);
    Task DeactivateAsync(int id);
}
