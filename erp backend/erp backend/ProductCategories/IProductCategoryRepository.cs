using erp_backend.ProductCategories.Dtos;

namespace erp_backend.ProductCategories;

public interface IProductCategoryRepository
{
    Task<List<ProductCategoryResponse>> GetAllAsync();
    Task<ProductCategoryResponse> GetByIdAsync(int id);
    Task<ProductCategoryResponse> CreateAsync(ProductCategoryRequest request);
    Task<ProductCategoryResponse> UpdateAsync(int id, ProductCategoryRequest request);
    Task ActivateAsync(int id);
    Task DeactivateAsync(int id);
}
