using erp_backend.Models;

namespace erp_backend.Branches.Dtos;

public class BranchResponse
{
    public int Id { get; set; }
    public int TenantId { get; set; }
    public string TenantName { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public DateTime CreatedAtUtc { get; set; }

    public static BranchResponse FromEntity(Branch entity, string? tenantName = null) =>
        new()
        {
            Id = entity.Id,
            TenantId = entity.TenantId,
            TenantName = tenantName ?? entity.Tenant?.Name ?? string.Empty,
            Name = entity.Name,
            Code = entity.Code,
            IsActive = entity.IsActive,
            CreatedAtUtc = entity.CreatedAtUtc,
        };
}
