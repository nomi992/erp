using erp_backend.Rights.Dtos;

namespace erp_backend.Rights;

public interface IRightRepository
{
    Task<List<RightModuleGroupResponse>> GetAllGroupedAsync();
}
