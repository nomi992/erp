using erp_backend.Models.Abstractions;

namespace erp_backend.Models;

public class ProductCategory : ITenantScoped
{
    public int Id { get; set; }
    public int TenantId { get; set; }
    public string Name { get; set; } = string.Empty;
    public int? ParentProductCategoryId { get; set; }
    public ProductCategory? ParentProductCategory { get; set; }
    public bool IsActive { get; set; } = true;
}
