using erp_backend.Models;

namespace erp_backend.CostCenters.Dtos;

public class CostCenterResponse
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int? ParentCostCenterId { get; set; }
    public string? ParentCostCenterName { get; set; }
    public bool IsActive { get; set; }

    public static CostCenterResponse FromEntity(CostCenter entity, string? parentName = null) =>
        new()
        {
            Id = entity.Id,
            Name = entity.Name,
            ParentCostCenterId = entity.ParentCostCenterId,
            ParentCostCenterName = parentName,
            IsActive = entity.IsActive,
        };
}
