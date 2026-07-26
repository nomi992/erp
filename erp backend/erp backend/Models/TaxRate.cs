using erp_backend.Models.Abstractions;

namespace erp_backend.Models;

public class TaxRate : ITenantScoped
{
    public int Id { get; set; }
    public int TenantId { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal Percentage { get; set; }
    public bool IsActive { get; set; } = true;
}
