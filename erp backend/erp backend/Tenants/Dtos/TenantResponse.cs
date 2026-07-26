using erp_backend.Models;

namespace erp_backend.Tenants.Dtos;

public class TenantResponse
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public DateTime CreatedAtUtc { get; set; }
    public int BranchCount { get; set; }
    public int UserCount { get; set; }

    public static TenantResponse FromEntity(Tenant entity, int branchCount = 0, int userCount = 0) =>
        new()
        {
            Id = entity.Id,
            Name = entity.Name,
            Code = entity.Code,
            IsActive = entity.IsActive,
            CreatedAtUtc = entity.CreatedAtUtc,
            BranchCount = branchCount,
            UserCount = userCount,
        };
}
