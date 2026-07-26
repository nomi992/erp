using System.ComponentModel.DataAnnotations;

namespace erp_backend.Roles.Dtos;

public class RoleRequest
{
    /// <summary>Only honored for a SystemAdmin caller; a tenant Admin always creates in their own tenant.</summary>
    public int? TenantId { get; set; }

    [Required]
    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }

    public List<int> RightIds { get; set; } = [];
}
