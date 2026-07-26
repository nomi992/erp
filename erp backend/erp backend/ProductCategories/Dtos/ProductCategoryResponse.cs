using erp_backend.Models;

namespace erp_backend.ProductCategories.Dtos;

public class ProductCategoryResponse
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int? ParentProductCategoryId { get; set; }
    public string? ParentProductCategoryName { get; set; }
    public bool IsActive { get; set; }

    public static ProductCategoryResponse FromEntity(ProductCategory entity, string? parentName = null) =>
        new()
        {
            Id = entity.Id,
            Name = entity.Name,
            ParentProductCategoryId = entity.ParentProductCategoryId,
            ParentProductCategoryName = parentName,
            IsActive = entity.IsActive,
        };
}
