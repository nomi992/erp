using erp_backend.Data;
using erp_backend.Exceptions;
using erp_backend.Messages;
using erp_backend.Models;
using erp_backend.Products.Dtos;
using Microsoft.EntityFrameworkCore;

namespace erp_backend.Products;

public class ProductRepository : IProductRepository
{
    private const string DefaultVariantCode = "DEFAULT";

    private readonly AppDbContext _context;

    public ProductRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<ProductResponse>> GetAllAsync()
    {
        var products = await LoadProductsQuery().OrderBy(p => p.Name).ToListAsync();
        return products.Select(ProductResponse.FromEntity).ToList();
    }

    public async Task<ProductResponse> GetByIdAsync(int id)
    {
        var product = await LoadProductsQuery().FirstOrDefaultAsync(p => p.Id == id)
            ?? throw new NotFoundException(ResponseMessage.ProductNotFound);

        return ProductResponse.FromEntity(product);
    }

    public async Task<ProductResponse> CreateAsync(ProductRequest request)
    {
        if (await _context.Products.AnyAsync(p => p.SKU == request.SKU))
        {
            throw new BadRequestException(ResponseMessage.ProductSKUExists);
        }

        if (await _context.UnitsOfMeasure.FindAsync(request.BaseUnitOfMeasureId) is null)
        {
            throw new BadRequestException(ResponseMessage.BaseUnitOfMeasureNotFound);
        }

        if (request.ProductCategoryId is int categoryId && await _context.ProductCategories.FindAsync(categoryId) is null)
        {
            throw new BadRequestException(ResponseMessage.ParentProductCategoryNotFound);
        }

        var product = new Product
        {
            SKU = request.SKU,
            Name = request.Name,
            ProductCategoryId = request.ProductCategoryId,
            BaseUnitOfMeasureId = request.BaseUnitOfMeasureId,
            HasVariants = request.HasVariants,
            IsStockTracked = request.IsStockTracked,
            ReorderLevel = request.ReorderLevel,
        };

        if (request.HasVariants)
        {
            if (request.Variants.Count == 0)
            {
                throw new BadRequestException(ResponseMessage.ProductRequiresAtLeastOneVariant);
            }

            ValidateVariantCodesUnique(request.Variants);

            var hasDefault = request.Variants.Any(v => v.IsDefault);
            foreach (var (variantRequest, index) in request.Variants.Select((v, i) => (v, i)))
            {
                product.Variants.Add(new ProductVariant
                {
                    Name = variantRequest.Name,
                    VariantCode = variantRequest.VariantCode,
                    Barcode = variantRequest.Barcode,
                    IsDefault = hasDefault ? variantRequest.IsDefault : index == 0,
                });
            }
        }
        else
        {
            product.Variants.Add(new ProductVariant { Name = "Default", VariantCode = DefaultVariantCode, IsDefault = true });
        }

        foreach (var conversionRequest in request.UOMConversions)
        {
            await ValidateConversionAsync(product.BaseUnitOfMeasureId, conversionRequest, existingConversions: []);
            product.UOMConversions.Add(new UOMConversion
            {
                UnitOfMeasureId = conversionRequest.UnitOfMeasureId,
                ConversionFactor = conversionRequest.ConversionFactor,
            });
        }

        _context.Products.Add(product);
        await _context.SaveChangesAsync();

        return await GetByIdAsync(product.Id);
    }

    public async Task<ProductResponse> UpdateAsync(int id, ProductRequest request)
    {
        var product = await _context.Products.FindAsync(id) ?? throw new NotFoundException(ResponseMessage.ProductNotFound);

        if (await _context.Products.AnyAsync(p => p.SKU == request.SKU && p.Id != id))
        {
            throw new BadRequestException(ResponseMessage.ProductSKUExists);
        }

        if (await _context.UnitsOfMeasure.FindAsync(request.BaseUnitOfMeasureId) is null)
        {
            throw new BadRequestException(ResponseMessage.BaseUnitOfMeasureNotFound);
        }

        if (request.ProductCategoryId is int categoryId && await _context.ProductCategories.FindAsync(categoryId) is null)
        {
            throw new BadRequestException(ResponseMessage.ParentProductCategoryNotFound);
        }

        product.SKU = request.SKU;
        product.Name = request.Name;
        product.ProductCategoryId = request.ProductCategoryId;
        product.BaseUnitOfMeasureId = request.BaseUnitOfMeasureId;
        product.IsStockTracked = request.IsStockTracked;
        product.ReorderLevel = request.ReorderLevel;
        // HasVariants and the variant/conversion collections themselves are managed through their
        // own dedicated endpoints, not through a bulk product update, once a product already exists.

        await _context.SaveChangesAsync();

        return await GetByIdAsync(id);
    }

    public Task ActivateAsync(int id) => SetActiveAsync(id, true);

    public Task DeactivateAsync(int id) => SetActiveAsync(id, false);

    public async Task<ProductVariantResponse> AddVariantAsync(int productId, ProductVariantRequest request)
    {
        var product = await _context.Products.Include(p => p.Variants).FirstOrDefaultAsync(p => p.Id == productId)
            ?? throw new NotFoundException(ResponseMessage.ProductNotFound);

        if (product.Variants.Any(v => v.VariantCode == request.VariantCode))
        {
            throw new BadRequestException(ResponseMessage.ProductVariantCodeExists);
        }

        var variant = new ProductVariant
        {
            ProductId = productId,
            Name = request.Name,
            VariantCode = request.VariantCode,
            Barcode = request.Barcode,
            IsDefault = request.IsDefault,
        };

        if (request.IsDefault)
        {
            foreach (var existing in product.Variants)
            {
                existing.IsDefault = false;
            }
        }

        _context.ProductVariants.Add(variant);

        if (!product.HasVariants)
        {
            product.HasVariants = true;
        }

        await _context.SaveChangesAsync();

        return ProductVariantResponse.FromEntity(variant);
    }

    public async Task<ProductVariantResponse> UpdateVariantAsync(int productId, int variantId, ProductVariantRequest request)
    {
        var product = await _context.Products.Include(p => p.Variants).FirstOrDefaultAsync(p => p.Id == productId)
            ?? throw new NotFoundException(ResponseMessage.ProductNotFound);

        var variant = product.Variants.FirstOrDefault(v => v.Id == variantId)
            ?? throw new NotFoundException(ResponseMessage.ProductVariantNotFound);

        if (product.Variants.Any(v => v.VariantCode == request.VariantCode && v.Id != variantId))
        {
            throw new BadRequestException(ResponseMessage.ProductVariantCodeExists);
        }

        variant.Name = request.Name;
        variant.VariantCode = request.VariantCode;
        variant.Barcode = request.Barcode;

        if (request.IsDefault && !variant.IsDefault)
        {
            foreach (var existing in product.Variants)
            {
                existing.IsDefault = existing.Id == variantId;
            }
        }

        await _context.SaveChangesAsync();

        return ProductVariantResponse.FromEntity(variant);
    }

    public Task ActivateVariantAsync(int productId, int variantId) => SetVariantActiveAsync(productId, variantId, true);

    public Task DeactivateVariantAsync(int productId, int variantId) => SetVariantActiveAsync(productId, variantId, false);

    public async Task<UOMConversionResponse> AddUOMConversionAsync(int productId, UOMConversionRequest request)
    {
        var product = await _context.Products.Include(p => p.UOMConversions).FirstOrDefaultAsync(p => p.Id == productId)
            ?? throw new NotFoundException(ResponseMessage.ProductNotFound);

        await ValidateConversionAsync(product.BaseUnitOfMeasureId, request, product.UOMConversions);

        var conversion = new UOMConversion
        {
            ProductId = productId,
            UnitOfMeasureId = request.UnitOfMeasureId,
            ConversionFactor = request.ConversionFactor,
        };

        _context.UOMConversions.Add(conversion);
        await _context.SaveChangesAsync();

        var unit = await _context.UnitsOfMeasure.FindAsync(request.UnitOfMeasureId);
        return new UOMConversionResponse
        {
            Id = conversion.Id,
            UnitOfMeasureId = conversion.UnitOfMeasureId,
            UnitOfMeasureCode = unit?.Code ?? string.Empty,
            ConversionFactor = conversion.ConversionFactor,
        };
    }

    public async Task<UOMConversionResponse> UpdateUOMConversionAsync(int productId, int conversionId, UOMConversionRequest request)
    {
        var product = await _context.Products.Include(p => p.UOMConversions).FirstOrDefaultAsync(p => p.Id == productId)
            ?? throw new NotFoundException(ResponseMessage.ProductNotFound);

        var conversion = product.UOMConversions.FirstOrDefault(c => c.Id == conversionId)
            ?? throw new NotFoundException(ResponseMessage.UOMConversionNotFound);

        await ValidateConversionAsync(product.BaseUnitOfMeasureId, request, product.UOMConversions.Where(c => c.Id != conversionId));

        conversion.UnitOfMeasureId = request.UnitOfMeasureId;
        conversion.ConversionFactor = request.ConversionFactor;
        await _context.SaveChangesAsync();

        var unit = await _context.UnitsOfMeasure.FindAsync(request.UnitOfMeasureId);
        return new UOMConversionResponse
        {
            Id = conversion.Id,
            UnitOfMeasureId = conversion.UnitOfMeasureId,
            UnitOfMeasureCode = unit?.Code ?? string.Empty,
            ConversionFactor = conversion.ConversionFactor,
        };
    }

    public async Task DeleteUOMConversionAsync(int productId, int conversionId)
    {
        var conversion = await _context.UOMConversions.FirstOrDefaultAsync(c => c.Id == conversionId && c.ProductId == productId)
            ?? throw new NotFoundException(ResponseMessage.UOMConversionNotFound);

        _context.UOMConversions.Remove(conversion);
        await _context.SaveChangesAsync();
    }

    public async Task<List<ProductVariantPriceResponse>> SetVariantPricesAsync(int productId, int variantId, List<ProductVariantPriceRequest> prices)
    {
        var variant = await _context.ProductVariants
            .Include(v => v.Prices)
            .FirstOrDefaultAsync(v => v.Id == variantId && v.ProductId == productId)
            ?? throw new NotFoundException(ResponseMessage.ProductVariantNotFound);

        foreach (var priceRequest in prices)
        {
            if (priceRequest.Amount < 0)
            {
                throw new BadRequestException(ResponseMessage.ProductVariantPriceAmountNegative);
            }

            var existing = variant.Prices.FirstOrDefault(p => p.PriceType == priceRequest.PriceType);
            if (existing is not null)
            {
                existing.Amount = priceRequest.Amount;
            }
            else
            {
                variant.Prices.Add(new ProductVariantPrice { PriceType = priceRequest.PriceType, Amount = priceRequest.Amount });
            }
        }

        await _context.SaveChangesAsync();

        return variant.Prices.Select(ProductVariantPriceResponse.FromEntity).ToList();
    }

    private IQueryable<Product> LoadProductsQuery() =>
        _context.Products
            .Include(p => p.ProductCategory)
            .Include(p => p.BaseUnitOfMeasure)
            .Include(p => p.Variants).ThenInclude(v => v.Prices)
            .Include(p => p.UOMConversions).ThenInclude(c => c.UnitOfMeasure);

    private async Task SetActiveAsync(int id, bool isActive)
    {
        var product = await _context.Products.FindAsync(id) ?? throw new NotFoundException(ResponseMessage.ProductNotFound);
        product.IsActive = isActive;
        await _context.SaveChangesAsync();
    }

    private async Task SetVariantActiveAsync(int productId, int variantId, bool isActive)
    {
        var variant = await _context.ProductVariants.FirstOrDefaultAsync(v => v.Id == variantId && v.ProductId == productId)
            ?? throw new NotFoundException(ResponseMessage.ProductVariantNotFound);

        variant.IsActive = isActive;
        await _context.SaveChangesAsync();
    }

    private static void ValidateVariantCodesUnique(List<ProductVariantRequest> variants)
    {
        var codes = variants.Select(v => v.VariantCode).ToList();
        if (codes.Distinct().Count() != codes.Count)
        {
            throw new BadRequestException(ResponseMessage.ProductVariantCodeExists);
        }
    }

    private async Task ValidateConversionAsync(int baseUnitOfMeasureId, UOMConversionRequest request, IEnumerable<UOMConversion> existingConversions)
    {
        if (request.ConversionFactor <= 0)
        {
            throw new BadRequestException(ResponseMessage.UOMConversionFactorInvalid);
        }

        if (request.UnitOfMeasureId == baseUnitOfMeasureId)
        {
            throw new BadRequestException(ResponseMessage.UOMConversionCannotBeBaseUnit);
        }

        if (await _context.UnitsOfMeasure.FindAsync(request.UnitOfMeasureId) is null)
        {
            throw new BadRequestException(ResponseMessage.UnitOfMeasureNotFound);
        }

        if (existingConversions.Any(c => c.UnitOfMeasureId == request.UnitOfMeasureId))
        {
            throw new BadRequestException(ResponseMessage.UOMConversionExists);
        }
    }
}
