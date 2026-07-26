using erp_backend.Models;
using erp_backend.Rights.Dtos;

namespace erp_backend.Roles.Dtos;

public class RoleResponse
{
    public int Id { get; set; }
    public int? TenantId { get; set; }
    public string? TenantName { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsSystemRole { get; set; }
    public DateTime CreatedAtUtc { get; set; }
    public List<RightResponse> Rights { get; set; } = [];

    public static RoleResponse FromEntity(Role entity, string? tenantName) =>
        new()
        {
            Id = entity.Id,
            TenantId = entity.TenantId,
            TenantName = tenantName ?? entity.Tenant?.Name,
            Name = entity.Name,
            Description = entity.Description,
            IsSystemRole = entity.IsSystemRole,
            CreatedAtUtc = entity.CreatedAtUtc,
            Rights = entity.RoleRights
                .Select(rr => RightResponse.FromEntity(rr.Right))
                .OrderBy(r => r.Module).ThenBy(r => r.Code)
                .ToList(),
        };
}
