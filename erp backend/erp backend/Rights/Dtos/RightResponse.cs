using erp_backend.Models;

namespace erp_backend.Rights.Dtos;

public class RightResponse
{
    public int Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Module { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;

    public static RightResponse FromEntity(Right entity) =>
        new()
        {
            Id = entity.Id,
            Code = entity.Code,
            Module = entity.Module,
            Description = entity.Description,
        };
}
