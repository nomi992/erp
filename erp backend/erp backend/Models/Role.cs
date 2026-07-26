namespace erp_backend.Models;

public class Role
{
    public int Id { get; set; }
    public int? TenantId { get; set; }
    public Tenant? Tenant { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsSystemRole { get; set; }
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

    public List<RoleRight> RoleRights { get; set; } = [];
}
