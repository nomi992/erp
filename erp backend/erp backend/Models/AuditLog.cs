using erp_backend.Models.Abstractions;

namespace erp_backend.Models;

public class AuditLog : ITenantScoped
{
    public int Id { get; set; }
    public int TenantId { get; set; }
    public int? BranchId { get; set; }
    public string EntityName { get; set; } = string.Empty;
    public int EntityId { get; set; }
    public string Action { get; set; } = string.Empty;
    public string? OldValue { get; set; }
    public string? NewValue { get; set; }
    public int? UserId { get; set; }
    public string Username { get; set; } = string.Empty;
    public DateTime TimestampUtc { get; set; } = DateTime.UtcNow;
}
