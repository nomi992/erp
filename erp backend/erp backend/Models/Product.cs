using erp_backend.Models.Abstractions;

namespace erp_backend.Models;

public class Product : ITenantScoped
{
    public int Id { get; set; }
    public int TenantId { get; set; }
    public string SKU { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public int? ProductCategoryId { get; set; }
    public ProductCategory? ProductCategory { get; set; }
    public int BaseUnitOfMeasureId { get; set; }
    public UnitOfMeasure? BaseUnitOfMeasure { get; set; }
    public bool HasVariants { get; set; }
    public bool IsStockTracked { get; set; } = true;

    // Reserved for a future batch/lot + expiry retrofit; not yet surfaced in any repository or UI.
    public bool TracksBatches { get; set; }
    public bool TracksExpiry { get; set; }

    public decimal? ReorderLevel { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

    public List<ProductVariant> Variants { get; set; } = [];
    public List<UOMConversion> UOMConversions { get; set; } = [];
}
