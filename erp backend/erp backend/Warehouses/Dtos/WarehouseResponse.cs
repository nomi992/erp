using erp_backend.Models;

namespace erp_backend.Warehouses.Dtos;

public class WarehouseResponse
{
    public int Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public int? CostCenterId { get; set; }
    public string? CostCenterName { get; set; }
    public bool IsDefault { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAtUtc { get; set; }

    public static WarehouseResponse FromEntity(Warehouse entity) =>
        new()
        {
            Id = entity.Id,
            Code = entity.Code,
            Name = entity.Name,
            CostCenterId = entity.CostCenterId,
            CostCenterName = entity.CostCenter?.Name,
            IsDefault = entity.IsDefault,
            IsActive = entity.IsActive,
            CreatedAtUtc = entity.CreatedAtUtc,
        };
}
