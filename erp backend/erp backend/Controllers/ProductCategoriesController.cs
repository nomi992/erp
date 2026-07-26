using erp_backend.Auth;
using erp_backend.Exceptions;
using erp_backend.Messages;
using erp_backend.Models;
using erp_backend.ProductCategories;
using erp_backend.ProductCategories.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace erp_backend.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ProductCategoriesController : ControllerBase
{
    private readonly IProductCategoryRepository _categories;

    public ProductCategoriesController(IProductCategoryRepository categories)
    {
        _categories = categories;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<List<ProductCategoryResponse>>>> GetAll()
    {
        try
        {
            var categories = await _categories.GetAllAsync();
            return Ok(ApiResponse<List<ProductCategoryResponse>>.Ok(categories, ResponseMessage.Success.ToText()));
        }
        catch (AppException ex)
        {
            return StatusCode(ex.StatusCode, ApiResponse<List<ProductCategoryResponse>>.Fail(ex.Message, ex.StatusCode));
        }
        catch (Exception)
        {
            return StatusCode(500, ApiResponse<List<ProductCategoryResponse>>.Fail(ResponseMessage.UnknownError.ToText(), 500));
        }
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<ApiResponse<ProductCategoryResponse>>> GetById(int id)
    {
        try
        {
            var category = await _categories.GetByIdAsync(id);
            return Ok(ApiResponse<ProductCategoryResponse>.Ok(category, ResponseMessage.Success.ToText()));
        }
        catch (AppException ex)
        {
            return StatusCode(ex.StatusCode, ApiResponse<ProductCategoryResponse>.Fail(ex.Message, ex.StatusCode));
        }
        catch (Exception)
        {
            return StatusCode(500, ApiResponse<ProductCategoryResponse>.Fail(ResponseMessage.UnknownError.ToText(), 500));
        }
    }

    [HttpPost]
    [Authorize(Policy = RightPolicies.Prefix + RightCodes.ProductCategoriesCreate)]
    public async Task<ActionResult<ApiResponse<ProductCategoryResponse>>> Create(ProductCategoryRequest request)
    {
        try
        {
            var category = await _categories.CreateAsync(request);
            return Ok(ApiResponse<ProductCategoryResponse>.Ok(category, ResponseMessage.ProductCategoryCreated.ToText()));
        }
        catch (AppException ex)
        {
            return StatusCode(ex.StatusCode, ApiResponse<ProductCategoryResponse>.Fail(ex.Message, ex.StatusCode));
        }
        catch (Exception)
        {
            return StatusCode(500, ApiResponse<ProductCategoryResponse>.Fail(ResponseMessage.UnknownError.ToText(), 500));
        }
    }

    [HttpPut("{id:int}")]
    [Authorize(Policy = RightPolicies.Prefix + RightCodes.ProductCategoriesEdit)]
    public async Task<ActionResult<ApiResponse<ProductCategoryResponse>>> Update(int id, ProductCategoryRequest request)
    {
        try
        {
            var category = await _categories.UpdateAsync(id, request);
            return Ok(ApiResponse<ProductCategoryResponse>.Ok(category, ResponseMessage.ProductCategoryUpdated.ToText()));
        }
        catch (AppException ex)
        {
            return StatusCode(ex.StatusCode, ApiResponse<ProductCategoryResponse>.Fail(ex.Message, ex.StatusCode));
        }
        catch (Exception)
        {
            return StatusCode(500, ApiResponse<ProductCategoryResponse>.Fail(ResponseMessage.UnknownError.ToText(), 500));
        }
    }

    [HttpPatch("{id:int}/activate")]
    [Authorize(Policy = RightPolicies.Prefix + RightCodes.ProductCategoriesEdit)]
    public async Task<ActionResult<ApiResponse<object>>> Activate(int id)
    {
        try
        {
            await _categories.ActivateAsync(id);
            return Ok(ApiResponse<object>.Ok(null, ResponseMessage.ProductCategoryActivated.ToText()));
        }
        catch (AppException ex)
        {
            return StatusCode(ex.StatusCode, ApiResponse<object>.Fail(ex.Message, ex.StatusCode));
        }
        catch (Exception)
        {
            return StatusCode(500, ApiResponse<object>.Fail(ResponseMessage.UnknownError.ToText(), 500));
        }
    }

    [HttpPatch("{id:int}/deactivate")]
    [Authorize(Policy = RightPolicies.Prefix + RightCodes.ProductCategoriesEdit)]
    public async Task<ActionResult<ApiResponse<object>>> Deactivate(int id)
    {
        try
        {
            await _categories.DeactivateAsync(id);
            return Ok(ApiResponse<object>.Ok(null, ResponseMessage.ProductCategoryDeactivated.ToText()));
        }
        catch (AppException ex)
        {
            return StatusCode(ex.StatusCode, ApiResponse<object>.Fail(ex.Message, ex.StatusCode));
        }
        catch (Exception)
        {
            return StatusCode(500, ApiResponse<object>.Fail(ResponseMessage.UnknownError.ToText(), 500));
        }
    }
}
