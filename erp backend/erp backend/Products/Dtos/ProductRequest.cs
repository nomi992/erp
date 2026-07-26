using System.ComponentModel.DataAnnotations;

namespace erp_backend.Products.Dtos;

public class ProductRequest
{
    [Required]
    public string SKU { get; set; } = string.Empty;

    [Required]
    public string Name { get; set; } = string.Empty;

    public int? ProductCategoryId { get; set; }

    public int BaseUnitOfMeasureId { get; set; }

    public bool HasVariants { get; set; }

    public bool IsStockTracked { get; set; } = true;

    public decimal? ReorderLevel { get; set; }

    // Only consulted on create: when HasVariants is false the repository ignores this and seeds one
    // implicit default variant instead. Variant lifecycle after creation goes through the dedicated
    // variant endpoints.
    public List<ProductVariantRequest> Variants { get; set; } = [];

    public List<UOMConversionRequest> UOMConversions { get; set; } = [];
}

public class ProductVariantRequest
{
    [Required]
    public string Name { get; set; } = string.Empty;

    [Required]
    public string VariantCode { get; set; } = string.Empty;

    public string? Barcode { get; set; }

    public bool IsDefault { get; set; }
}

public class UOMConversionRequest
{
    public int UnitOfMeasureId { get; set; }

    public decimal ConversionFactor { get; set; }
}

public class ProductVariantPriceRequest
{
    public erp_backend.Models.PriceType PriceType { get; set; }

    public decimal Amount { get; set; }
}
