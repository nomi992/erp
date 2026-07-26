using erp_backend.Models;
using erp_backend.Repositories;
using erp_backend.Rights.Dtos;

namespace erp_backend.Rights;

public class RightRepository : IRightRepository
{
    private readonly IRepository<Right> _rights;

    public RightRepository(IRepository<Right> rights)
    {
        _rights = rights;
    }

    public async Task<List<RightModuleGroupResponse>> GetAllGroupedAsync()
    {
        var rights = await _rights.GetAllAsync();

        return rights
            .GroupBy(r => r.Module)
            .Select(g => new RightModuleGroupResponse
            {
                Module = g.Key,
                Rights = g.Select(RightResponse.FromEntity).OrderBy(r => r.Code).ToList(),
            })
            .OrderBy(g => g.Module)
            .ToList();
    }
}
