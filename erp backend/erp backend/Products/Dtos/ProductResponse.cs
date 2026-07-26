using erp_backend.Models;

namespace erp_backend.Products.Dtos;

public class ProductResponse
{
    public int Id { get; set; }
    public string SKU { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public int? ProductCategoryId { get; set; }
    public string? ProductCategoryName { get; set; }
    public int BaseUnitOfMeasureId { get; set; }
    public string BaseUnitOfMeasureCode { get; set; } = string.Empty;
    public bool HasVariants { get; set; }
    public bool IsStockTracked { get; set; }
    public decimal? ReorderLevel { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAtUtc { get; set; }
    public List<ProductVariantResponse> Variants { get; set; } = [];
    public List<UOMConversionResponse> UOMConversions { get; set; } = [];

    public static ProductResponse FromEntity(Product entity) =>
        new()
        {
            Id = entity.Id,
            SKU = entity.SKU,
            Name = entity.Name,
            ProductCategoryId = entity.ProductCategoryId,
            ProductCategoryName = entity.ProductCategory?.Name,
            BaseUnitOfMeasureId = entity.BaseUnitOfMeasureId,
            BaseUnitOfMeasureCode = entity.BaseUnitOfMeasure?.Code ?? string.Empty,
            HasVariants = entity.HasVariants,
            IsStockTracked = entity.IsStockTracked,
            ReorderLevel = entity.ReorderLevel,
            IsActive = entity.IsActive,
            CreatedAtUtc = entity.CreatedAtUtc,
            Variants = entity.Variants.Select(ProductVariantResponse.FromEntity).ToList(),
            UOMConversions = entity.UOMConversions.Select(UOMConversionResponse.FromEntity).ToList(),
        };
}

public class ProductVariantResponse
{
    public int Id { get; set; }
    public int ProductId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string VariantCode { get; set; } = string.Empty;
    public string? Barcode { get; set; }
    public bool IsDefault { get; set; }
    public bool IsActive { get; set; }
    public List<ProductVariantPriceResponse> Prices { get; set; } = [];

    public static ProductVariantResponse FromEntity(ProductVariant entity) =>
        new()
        {
            Id = entity.Id,
            ProductId = entity.ProductId,
            Name = entity.Name,
            VariantCode = entity.VariantCode,
            Barcode = entity.Barcode,
            IsDefault = entity.IsDefault,
            IsActive = entity.IsActive,
            Prices = entity.Prices.Select(ProductVariantPriceResponse.FromEntity).ToList(),
        };
}

public class UOMConversionResponse
{
    public int Id { get; set; }
    public int UnitOfMeasureId { get; set; }
    public string UnitOfMeasureCode { get; set; } = string.Empty;
    public decimal ConversionFactor { get; set; }

    public static UOMConversionResponse FromEntity(UOMConversion entity) =>
        new()
        {
            Id = entity.Id,
            UnitOfMeasureId = entity.UnitOfMeasureId,
            UnitOfMeasureCode = entity.UnitOfMeasure?.Code ?? string.Empty,
            ConversionFactor = entity.ConversionFactor,
        };
}

public class ProductVariantPriceResponse
{
    public int Id { get; set; }
    public PriceType PriceType { get; set; }
    public decimal Amount { get; set; }

    public static ProductVariantPriceResponse FromEntity(ProductVariantPrice entity) =>
        new()
        {
            Id = entity.Id,
            PriceType = entity.PriceType,
            Amount = entity.Amount,
        };
}
