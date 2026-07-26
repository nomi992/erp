using erp_backend.Auth;
using erp_backend.Exceptions;
using erp_backend.Messages;
using erp_backend.Models;
using erp_backend.Products;
using erp_backend.Products.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace erp_backend.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ProductsController : ControllerBase
{
    private readonly IProductRepository _products;

    public ProductsController(IProductRepository products)
    {
        _products = products;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<List<ProductResponse>>>> GetAll()
    {
        try
        {
            var products = await _products.GetAllAsync();
            return Ok(ApiResponse<List<ProductResponse>>.Ok(products, ResponseMessage.Success.ToText()));
        }
        catch (AppException ex)
        {
            return StatusCode(ex.StatusCode, ApiResponse<List<ProductResponse>>.Fail(ex.Message, ex.StatusCode));
        }
        catch (Exception)
        {
            return StatusCode(500, ApiResponse<List<ProductResponse>>.Fail(ResponseMessage.UnknownError.ToText(), 500));
        }
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<ApiResponse<ProductResponse>>> GetById(int id)
    {
        try
        {
            var product = await _products.GetByIdAsync(id);
            return Ok(ApiResponse<ProductResponse>.Ok(product, ResponseMessage.Success.ToText()));
        }
        catch (AppException ex)
        {
            return StatusCode(ex.StatusCode, ApiResponse<ProductResponse>.Fail(ex.Message, ex.StatusCode));
        }
        catch (Exception)
        {
            return StatusCode(500, ApiResponse<ProductResponse>.Fail(ResponseMessage.UnknownError.ToText(), 500));
        }
    }

    [HttpPost]
    [Authorize(Policy = RightPolicies.Prefix + RightCodes.ProductsCreate)]
    public async Task<ActionResult<ApiResponse<ProductResponse>>> Create(ProductRequest request)
    {
        try
        {
            var product = await _products.CreateAsync(request);
            return Ok(ApiResponse<ProductResponse>.Ok(product, ResponseMessage.ProductCreated.ToText()));
        }
        catch (AppException ex)
        {
            return StatusCode(ex.StatusCode, ApiResponse<ProductResponse>.Fail(ex.Message, ex.StatusCode));
        }
        catch (Exception)
        {
            return StatusCode(500, ApiResponse<ProductResponse>.Fail(ResponseMessage.UnknownError.ToText(), 500));
        }
    }

    [HttpPut("{id:int}")]
    [Authorize(Policy = RightPolicies.Prefix + RightCodes.ProductsEdit)]
    public async Task<ActionResult<ApiResponse<ProductResponse>>> Update(int id, ProductRequest request)
    {
        try
        {
            var product = await _products.UpdateAsync(id, request);
            return Ok(ApiResponse<ProductResponse>.Ok(product, ResponseMessage.ProductUpdated.ToText()));
        }
        catch (AppException ex)
        {
            return StatusCode(ex.StatusCode, ApiResponse<ProductResponse>.Fail(ex.Message, ex.StatusCode));
        }
        catch (Exception)
        {
            return StatusCode(500, ApiResponse<ProductResponse>.Fail(ResponseMessage.UnknownError.ToText(), 500));
        }
    }

    [HttpPatch("{id:int}/activate")]
    [Authorize(Policy = RightPolicies.Prefix + RightCodes.ProductsEdit)]
    public async Task<ActionResult<ApiResponse<object>>> Activate(int id)
    {
        try
        {
            await _products.ActivateAsync(id);
            return Ok(ApiResponse<object>.Ok(null, ResponseMessage.ProductActivated.ToText()));
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
    [Authorize(Policy = RightPolicies.Prefix + RightCodes.ProductsEdit)]
    public async Task<ActionResult<ApiResponse<object>>> Deactivate(int id)
    {
        try
        {
            await _products.DeactivateAsync(id);
            return Ok(ApiResponse<object>.Ok(null, ResponseMessage.ProductDeactivated.ToText()));
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

    [HttpPost("{id:int}/variants")]
    [Authorize(Policy = RightPolicies.Prefix + RightCodes.ProductsEdit)]
    public async Task<ActionResult<ApiResponse<ProductVariantResponse>>> AddVariant(int id, ProductVariantRequest request)
    {
        try
        {
            var variant = await _products.AddVariantAsync(id, request);
            return Ok(ApiResponse<ProductVariantResponse>.Ok(variant, ResponseMessage.ProductUpdated.ToText()));
        }
        catch (AppException ex)
        {
            return StatusCode(ex.StatusCode, ApiResponse<ProductVariantResponse>.Fail(ex.Message, ex.StatusCode));
        }
        catch (Exception)
        {
            return StatusCode(500, ApiResponse<ProductVariantResponse>.Fail(ResponseMessage.UnknownError.ToText(), 500));
        }
    }

    [HttpPut("{id:int}/variants/{variantId:int}")]
    [Authorize(Policy = RightPolicies.Prefix + RightCodes.ProductsEdit)]
    public async Task<ActionResult<ApiResponse<ProductVariantResponse>>> UpdateVariant(int id, int variantId, ProductVariantRequest request)
    {
        try
        {
            var variant = await _products.UpdateVariantAsync(id, variantId, request);
            return Ok(ApiResponse<ProductVariantResponse>.Ok(variant, ResponseMessage.ProductUpdated.ToText()));
        }
        catch (AppException ex)
        {
            return StatusCode(ex.StatusCode, ApiResponse<ProductVariantResponse>.Fail(ex.Message, ex.StatusCode));
        }
        catch (Exception)
        {
            return StatusCode(500, ApiResponse<ProductVariantResponse>.Fail(ResponseMessage.UnknownError.ToText(), 500));
        }
    }

    [HttpPatch("{id:int}/variants/{variantId:int}/activate")]
    [Authorize(Policy = RightPolicies.Prefix + RightCodes.ProductsEdit)]
    public async Task<ActionResult<ApiResponse<object>>> ActivateVariant(int id, int variantId)
    {
        try
        {
            await _products.ActivateVariantAsync(id, variantId);
            return Ok(ApiResponse<object>.Ok(null, ResponseMessage.ProductActivated.ToText()));
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

    [HttpPatch("{id:int}/variants/{variantId:int}/deactivate")]
    [Authorize(Policy = RightPolicies.Prefix + RightCodes.ProductsEdit)]
    public async Task<ActionResult<ApiResponse<object>>> DeactivateVariant(int id, int variantId)
    {
        try
        {
            await _products.DeactivateVariantAsync(id, variantId);
            return Ok(ApiResponse<object>.Ok(null, ResponseMessage.ProductDeactivated.ToText()));
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

    [HttpPut("{id:int}/variants/{variantId:int}/prices")]
    [Authorize(Policy = RightPolicies.Prefix + RightCodes.ProductsEdit)]
    public async Task<ActionResult<ApiResponse<List<ProductVariantPriceResponse>>>> SetVariantPrices(
        int id, int variantId, List<ProductVariantPriceRequest> prices)
    {
        try
        {
            var result = await _products.SetVariantPricesAsync(id, variantId, prices);
            return Ok(ApiResponse<List<ProductVariantPriceResponse>>.Ok(result, ResponseMessage.ProductUpdated.ToText()));
        }
        catch (AppException ex)
        {
            return StatusCode(ex.StatusCode, ApiResponse<List<ProductVariantPriceResponse>>.Fail(ex.Message, ex.StatusCode));
        }
        catch (Exception)
        {
            return StatusCode(500, ApiResponse<List<ProductVariantPriceResponse>>.Fail(ResponseMessage.UnknownError.ToText(), 500));
        }
    }

    [HttpPost("{id:int}/conversions")]
    [Authorize(Policy = RightPolicies.Prefix + RightCodes.ProductsEdit)]
    public async Task<ActionResult<ApiResponse<UOMConversionResponse>>> AddConversion(int id, UOMConversionRequest request)
    {
        try
        {
            var conversion = await _products.AddUOMConversionAsync(id, request);
            return Ok(ApiResponse<UOMConversionResponse>.Ok(conversion, ResponseMessage.ProductUpdated.ToText()));
        }
        catch (AppException ex)
        {
            return StatusCode(ex.StatusCode, ApiResponse<UOMConversionResponse>.Fail(ex.Message, ex.StatusCode));
        }
        catch (Exception)
        {
            return StatusCode(500, ApiResponse<UOMConversionResponse>.Fail(ResponseMessage.UnknownError.ToText(), 500));
        }
    }

    [HttpPut("{id:int}/conversions/{conversionId:int}")]
    [Authorize(Policy = RightPolicies.Prefix + RightCodes.ProductsEdit)]
    public async Task<ActionResult<ApiResponse<UOMConversionResponse>>> UpdateConversion(int id, int conversionId, UOMConversionRequest request)
    {
        try
        {
            var conversion = await _products.UpdateUOMConversionAsync(id, conversionId, request);
            return Ok(ApiResponse<UOMConversionResponse>.Ok(conversion, ResponseMessage.ProductUpdated.ToText()));
        }
        catch (AppException ex)
        {
            return StatusCode(ex.StatusCode, ApiResponse<UOMConversionResponse>.Fail(ex.Message, ex.StatusCode));
        }
        catch (Exception)
        {
            return StatusCode(500, ApiResponse<UOMConversionResponse>.Fail(ResponseMessage.UnknownError.ToText(), 500));
        }
    }

    [HttpDelete("{id:int}/conversions/{conversionId:int}")]
    [Authorize(Policy = RightPolicies.Prefix + RightCodes.ProductsEdit)]
    public async Task<ActionResult<ApiResponse<object>>> DeleteConversion(int id, int conversionId)
    {
        try
        {
            await _products.DeleteUOMConversionAsync(id, conversionId);
            return Ok(ApiResponse<object>.Ok(null, ResponseMessage.ProductUpdated.ToText()));
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
