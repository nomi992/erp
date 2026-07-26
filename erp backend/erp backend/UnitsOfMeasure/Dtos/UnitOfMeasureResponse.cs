using erp_backend.Models;

namespace erp_backend.UnitsOfMeasure.Dtos;

public class UnitOfMeasureResponse
{
    public int Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public bool IsActive { get; set; }

    public static UnitOfMeasureResponse FromEntity(UnitOfMeasure entity) =>
        new()
        {
            Id = entity.Id,
            Code = entity.Code,
            Name = entity.Name,
            IsActive = entity.IsActive,
        };
}
