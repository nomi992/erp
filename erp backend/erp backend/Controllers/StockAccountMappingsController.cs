using erp_backend.Auth;
using erp_backend.Exceptions;
using erp_backend.Inventory;
using erp_backend.Inventory.Dtos;
using erp_backend.Messages;
using erp_backend.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace erp_backend.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class StockAccountMappingsController : ControllerBase
{
    private readonly IStockAccountMappingRepository _mappings;

    public StockAccountMappingsController(IStockAccountMappingRepository mappings)
    {
        _mappings = mappings;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<List<StockAccountMappingResponse>>>> GetAll()
    {
        try
        {
            var mappings = await _mappings.GetAllAsync();
            return Ok(ApiResponse<List<StockAccountMappingResponse>>.Ok(mappings, ResponseMessage.Success.ToText()));
        }
        catch (AppException ex)
        {
            return StatusCode(ex.StatusCode, ApiResponse<List<StockAccountMappingResponse>>.Fail(ex.Message, ex.StatusCode));
        }
        catch (Exception)
        {
            return StatusCode(500, ApiResponse<List<StockAccountMappingResponse>>.Fail(ResponseMessage.UnknownError.ToText(), 500));
        }
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<ApiResponse<StockAccountMappingResponse>>> GetById(int id)
    {
        try
        {
            var mapping = await _mappings.GetByIdAsync(id);
            return Ok(ApiResponse<StockAccountMappingResponse>.Ok(mapping, ResponseMessage.Success.ToText()));
        }
        catch (AppException ex)
        {
            return StatusCode(ex.StatusCode, ApiResponse<StockAccountMappingResponse>.Fail(ex.Message, ex.StatusCode));
        }
        catch (Exception)
        {
            return StatusCode(500, ApiResponse<StockAccountMappingResponse>.Fail(ResponseMessage.UnknownError.ToText(), 500));
        }
    }

    [HttpPost]
    [Authorize(Policy = RightPolicies.Prefix + RightCodes.StockAccountMappingsManage)]
    public async Task<ActionResult<ApiResponse<StockAccountMappingResponse>>> Create(StockAccountMappingRequest request)
    {
        try
        {
            var mapping = await _mappings.CreateAsync(request);
            return Ok(ApiResponse<StockAccountMappingResponse>.Ok(mapping, ResponseMessage.StockAccountMappingCreated.ToText()));
        }
        catch (AppException ex)
        {
            return StatusCode(ex.StatusCode, ApiResponse<StockAccountMappingResponse>.Fail(ex.Message, ex.StatusCode));
        }
        catch (Exception)
        {
            return StatusCode(500, ApiResponse<StockAccountMappingResponse>.Fail(ResponseMessage.UnknownError.ToText(), 500));
        }
    }

    [HttpPut("{id:int}")]
    [Authorize(Policy = RightPolicies.Prefix + RightCodes.StockAccountMappingsManage)]
    public async Task<ActionResult<ApiResponse<StockAccountMappingResponse>>> Update(int id, StockAccountMappingRequest request)
    {
        try
        {
            var mapping = await _mappings.UpdateAsync(id, request);
            return Ok(ApiResponse<StockAccountMappingResponse>.Ok(mapping, ResponseMessage.StockAccountMappingUpdated.ToText()));
        }
        catch (AppException ex)
        {
            return StatusCode(ex.StatusCode, ApiResponse<StockAccountMappingResponse>.Fail(ex.Message, ex.StatusCode));
        }
        catch (Exception)
        {
            return StatusCode(500, ApiResponse<StockAccountMappingResponse>.Fail(ResponseMessage.UnknownError.ToText(), 500));
        }
    }
}
