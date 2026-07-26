using erp_backend.Exceptions;
using erp_backend.Messages;
using erp_backend.Models;
using erp_backend.ProductCategories.Dtos;
using erp_backend.Repositories;

namespace erp_backend.ProductCategories;

public class ProductCategoryRepository : IProductCategoryRepository
{
    private readonly IRepository<ProductCategory> _categories;

    public ProductCategoryRepository(IRepository<ProductCategory> categories)
    {
        _categories = categories;
    }

    public async Task<List<ProductCategoryResponse>> GetAllAsync()
    {
        var categories = await _categories.GetAllAsync();
        var byId = categories.ToDictionary(c => c.Id);

        return categories
            .Select(c => ProductCategoryResponse.FromEntity(
                c, c.ParentProductCategoryId is int parentId && byId.TryGetValue(parentId, out var parent) ? parent.Name : null))
            .OrderBy(c => c.Name)
            .ToList();
    }

    public async Task<ProductCategoryResponse> GetByIdAsync(int id)
    {
        var category = await _categories.GetByIdAsync(id) ?? throw new NotFoundException(ResponseMessage.ProductCategoryNotFound);

        string? parentName = null;
        if (category.ParentProductCategoryId is int parentId)
        {
            parentName = (await _categories.GetByIdAsync(parentId))?.Name;
        }

        return ProductCategoryResponse.FromEntity(category, parentName);
    }

    public async Task<ProductCategoryResponse> CreateAsync(ProductCategoryRequest request)
    {
        ProductCategory? parent = null;
        if (request.ParentProductCategoryId is int parentId)
        {
            parent = await _categories.GetByIdAsync(parentId) ?? throw new BadRequestException(ResponseMessage.ParentProductCategoryNotFound);
        }

        var category = new ProductCategory { Name = request.Name, ParentProductCategoryId = request.ParentProductCategoryId };
        await _categories.AddAsync(category);
        await _categories.SaveChangesAsync();

        return ProductCategoryResponse.FromEntity(category, parent?.Name);
    }

    public async Task<ProductCategoryResponse> UpdateAsync(int id, ProductCategoryRequest request)
    {
        var category = await _categories.GetByIdAsync(id) ?? throw new NotFoundException(ResponseMessage.ProductCategoryNotFound);

        ProductCategory? parent = null;
        if (request.ParentProductCategoryId is int parentId)
        {
            if (parentId == id)
            {
                throw new BadRequestException(ResponseMessage.ProductCategoryCannotBeOwnParent);
            }

            parent = await _categories.GetByIdAsync(parentId) ?? throw new BadRequestException(ResponseMessage.ParentProductCategoryNotFound);

            if (await CreatesCircularReferenceAsync(id, parentId))
            {
                throw new BadRequestException(ResponseMessage.ProductCategoryCircularReference);
            }
        }

        category.Name = request.Name;
        category.ParentProductCategoryId = request.ParentProductCategoryId;

        _categories.Update(category);
        await _categories.SaveChangesAsync();

        return ProductCategoryResponse.FromEntity(category, parent?.Name);
    }

    public Task ActivateAsync(int id) => SetActiveAsync(id, true);

    public Task DeactivateAsync(int id) => SetActiveAsync(id, false);

    private async Task SetActiveAsync(int id, bool isActive)
    {
        var category = await _categories.GetByIdAsync(id) ?? throw new NotFoundException(ResponseMessage.ProductCategoryNotFound);

        category.IsActive = isActive;
        _categories.Update(category);
        await _categories.SaveChangesAsync();
    }

    private async Task<bool> CreatesCircularReferenceAsync(int categoryId, int proposedParentId)
    {
        var currentId = (int?)proposedParentId;
        var visited = new HashSet<int>();

        while (currentId is int id)
        {
            if (id == categoryId)
            {
                return true;
            }

            if (!visited.Add(id))
            {
                break;
            }

            var current = await _categories.GetByIdAsync(id);
            currentId = current?.ParentProductCategoryId;
        }

        return false;
    }
}
