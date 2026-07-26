using erp_backend.Models.Abstractions;

namespace erp_backend.Models;

public class UnitOfMeasure : ITenantScoped
{
    public int Id { get; set; }
    public int TenantId { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
}
