using erp_backend.Models.Abstractions;

namespace erp_backend.Models;

public enum PriceType
{
    Purchase,
    Retail,
    Sale,
}

public class ProductVariantPrice : ITenantScoped
{
    public int Id { get; set; }
    public int TenantId { get; set; }
    public int ProductVariantId { get; set; }
    public ProductVariant? ProductVariant { get; set; }
    public PriceType PriceType { get; set; }
    public decimal Amount { get; set; }
}
