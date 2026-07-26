using System.ComponentModel.DataAnnotations;

namespace erp_backend.ProductCategories.Dtos;

public class ProductCategoryRequest
{
    [Required]
    public string Name { get; set; } = string.Empty;

    public int? ParentProductCategoryId { get; set; }
}
