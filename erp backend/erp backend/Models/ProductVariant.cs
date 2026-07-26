using erp_backend.Models.Abstractions;

namespace erp_backend.Models;

public class ProductVariant : ITenantScoped
{
    public int Id { get; set; }
    public int TenantId { get; set; }
    public int ProductId { get; set; }
    public Product? Product { get; set; }
    public string Name { get; set; } = string.Empty;
    public string VariantCode { get; set; } = string.Empty;
    public string? Barcode { get; set; }
    public bool IsDefault { get; set; }
    public bool IsActive { get; set; } = true;

    public List<ProductVariantPrice> Prices { get; set; } = [];
}
