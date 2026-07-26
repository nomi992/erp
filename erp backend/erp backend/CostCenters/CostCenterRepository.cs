using erp_backend.CostCenters.Dtos;
using erp_backend.Exceptions;
using erp_backend.Messages;
using erp_backend.Models;
using erp_backend.Repositories;

namespace erp_backend.CostCenters;

public class CostCenterRepository : ICostCenterRepository
{
    private readonly IRepository<CostCenter> _costCenters;
    private readonly IRepository<VoucherDetail> _voucherDetails;

    public CostCenterRepository(IRepository<CostCenter> costCenters, IRepository<VoucherDetail> voucherDetails)
    {
        _costCenters = costCenters;
        _voucherDetails = voucherDetails;
    }

    public async Task<List<CostCenterResponse>> GetAllAsync()
    {
        var costCenters = await _costCenters.GetAllAsync();
        var byId = costCenters.ToDictionary(c => c.Id);

        return costCenters
            .Select(c => CostCenterResponse.FromEntity(
                c, c.ParentCostCenterId is int parentId && byId.TryGetValue(parentId, out var parent) ? parent.Name : null))
            .OrderBy(c => c.Name)
            .ToList();
    }

    public async Task<CostCenterResponse> GetByIdAsync(int id)
    {
        var costCenter = await _costCenters.GetByIdAsync(id) ?? throw new NotFoundException(ResponseMessage.CostCenterNotFound);

        string? parentName = null;
        if (costCenter.ParentCostCenterId is int parentId)
        {
            parentName = (await _costCenters.GetByIdAsync(parentId))?.Name;
        }

        return CostCenterResponse.FromEntity(costCenter, parentName);
    }

    public async Task<CostCenterResponse> CreateAsync(CostCenterRequest request)
    {
        CostCenter? parent = null;
        if (request.ParentCostCenterId is int parentId)
        {
            parent = await _costCenters.GetByIdAsync(parentId) ?? throw new BadRequestException(ResponseMessage.ParentCostCenterNotFound);
        }

        var costCenter = new CostCenter { Name = request.Name, ParentCostCenterId = request.ParentCostCenterId };
        await _costCenters.AddAsync(costCenter);
        await _costCenters.SaveChangesAsync();

        return CostCenterResponse.FromEntity(costCenter, parent?.Name);
    }

    public async Task<CostCenterResponse> UpdateAsync(int id, CostCenterRequest request)
    {
        var costCenter = await _costCenters.GetByIdAsync(id) ?? throw new NotFoundException(ResponseMessage.CostCenterNotFound);

        CostCenter? parent = null;
        if (request.ParentCostCenterId is int parentId)
        {
            if (parentId == id)
            {
                throw new BadRequestException(ResponseMessage.CostCenterCannotBeOwnParent);
            }

            parent = await _costCenters.GetByIdAsync(parentId) ?? throw new BadRequestException(ResponseMessage.ParentCostCenterNotFound);

            if (await CreatesCircularReferenceAsync(id, parentId))
            {
                throw new BadRequestException(ResponseMessage.CircularReference);
            }
        }

        costCenter.Name = request.Name;
        costCenter.ParentCostCenterId = request.ParentCostCenterId;

        _costCenters.Update(costCenter);
        await _costCenters.SaveChangesAsync();

        return CostCenterResponse.FromEntity(costCenter, parent?.Name);
    }

    public Task ActivateAsync(int id) => SetActiveAsync(id, true);

    public Task DeactivateAsync(int id) => SetActiveAsync(id, false);

    public async Task DeleteAsync(int id)
    {
        var costCenter = await _costCenters.GetByIdAsync(id) ?? throw new NotFoundException(ResponseMessage.CostCenterNotFound);

        var hasChildren = (await _costCenters.FindAsync(c => c.ParentCostCenterId == id)).Count > 0;
        if (hasChildren)
        {
            throw new BadRequestException(ResponseMessage.CostCenterHasChildren);
        }

        var isReferenced = (await _voucherDetails.FindAsync(d => d.CostCenterId == id)).Count > 0;
        if (isReferenced)
        {
            throw new BadRequestException(ResponseMessage.CostCenterHasTransactions);
        }

        _costCenters.Remove(costCenter);
        await _costCenters.SaveChangesAsync();
    }

    private async Task SetActiveAsync(int id, bool isActive)
    {
        var costCenter = await _costCenters.GetByIdAsync(id) ?? throw new NotFoundException(ResponseMessage.CostCenterNotFound);

        costCenter.IsActive = isActive;
        _costCenters.Update(costCenter);
        await _costCenters.SaveChangesAsync();
    }

    private async Task<bool> CreatesCircularReferenceAsync(int costCenterId, int proposedParentId)
    {
        var currentId = (int?)proposedParentId;
        var visited = new HashSet<int>();

        while (currentId is int id)
        {
            if (id == costCenterId)
            {
                return true;
            }

            if (!visited.Add(id))
            {
                break;
            }

            var current = await _costCenters.GetByIdAsync(id);
            currentId = current?.ParentCostCenterId;
        }

        return false;
    }
}
