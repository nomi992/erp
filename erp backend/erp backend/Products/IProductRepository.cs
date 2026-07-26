using erp_backend.Products.Dtos;

namespace erp_backend.Products;

public interface IProductRepository
{
    Task<List<ProductResponse>> GetAllAsync();
    Task<ProductResponse> GetByIdAsync(int id);
    Task<ProductResponse> CreateAsync(ProductRequest request);
    Task<ProductResponse> UpdateAsync(int id, ProductRequest request);
    Task ActivateAsync(int id);
    Task DeactivateAsync(int id);

    Task<ProductVariantResponse> AddVariantAsync(int productId, ProductVariantRequest request);
    Task<ProductVariantResponse> UpdateVariantAsync(int productId, int variantId, ProductVariantRequest request);
    Task ActivateVariantAsync(int productId, int variantId);
    Task DeactivateVariantAsync(int productId, int variantId);

    Task<UOMConversionResponse> AddUOMConversionAsync(int productId, UOMConversionRequest request);
    Task<UOMConversionResponse> UpdateUOMConversionAsync(int productId, int conversionId, UOMConversionRequest request);
    Task DeleteUOMConversionAsync(int productId, int conversionId);

    Task<List<ProductVariantPriceResponse>> SetVariantPricesAsync(int productId, int variantId, List<ProductVariantPriceRequest> prices);
}
