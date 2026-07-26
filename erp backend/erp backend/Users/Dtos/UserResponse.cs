using erp_backend.Auth.Dtos;
using erp_backend.Models;

namespace erp_backend.Users.Dtos;

public class UserResponse
{
    public int Id { get; set; }
    public int TenantId { get; set; }
    public string TenantName { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public int RoleId { get; set; }
    public string Role { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public DateTime CreatedAtUtc { get; set; }
    public List<BranchSummary> Branches { get; set; } = [];

    public static UserResponse FromEntity(User entity, string? tenantName, List<BranchSummary> branches) =>
        new()
        {
            Id = entity.Id,
            TenantId = entity.TenantId,
            TenantName = tenantName ?? entity.Tenant?.Name ?? string.Empty,
            Username = entity.Username,
            RoleId = entity.RoleId,
            Role = entity.Role?.Name ?? string.Empty,
            IsActive = entity.IsActive,
            CreatedAtUtc = entity.CreatedAtUtc,
            Branches = branches,
        };
}
