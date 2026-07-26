using erp_backend.Auth;
using erp_backend.Exceptions;
using erp_backend.Messages;
using erp_backend.Models;
using erp_backend.Warehouses;
using erp_backend.Warehouses.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace erp_backend.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class WarehousesController : ControllerBase
{
    private readonly IWarehouseRepository _warehouses;

    public WarehousesController(IWarehouseRepository warehouses)
    {
        _warehouses = warehouses;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<List<WarehouseResponse>>>> GetAll()
    {
        try
        {
            var warehouses = await _warehouses.GetAllAsync();
            return Ok(ApiResponse<List<WarehouseResponse>>.Ok(warehouses, ResponseMessage.Success.ToText()));
        }
        catch (AppException ex)
        {
            return StatusCode(ex.StatusCode, ApiResponse<List<WarehouseResponse>>.Fail(ex.Message, ex.StatusCode));
        }
        catch (Exception)
        {
            return StatusCode(500, ApiResponse<List<WarehouseResponse>>.Fail(ResponseMessage.UnknownError.ToText(), 500));
        }
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<ApiResponse<WarehouseResponse>>> GetById(int id)
    {
        try
        {
            var warehouse = await _warehouses.GetByIdAsync(id);
            return Ok(ApiResponse<WarehouseResponse>.Ok(warehouse, ResponseMessage.Success.ToText()));
        }
        catch (AppException ex)
        {
            return StatusCode(ex.StatusCode, ApiResponse<WarehouseResponse>.Fail(ex.Message, ex.StatusCode));
        }
        catch (Exception)
        {
            return StatusCode(500, ApiResponse<WarehouseResponse>.Fail(ResponseMessage.UnknownError.ToText(), 500));
        }
    }

    [HttpPost]
    [Authorize(Policy = RightPolicies.Prefix + RightCodes.WarehousesCreate)]
    public async Task<ActionResult<ApiResponse<WarehouseResponse>>> Create(WarehouseRequest request)
    {
        try
        {
            var warehouse = await _warehouses.CreateAsync(request);
            return Ok(ApiResponse<WarehouseResponse>.Ok(warehouse, ResponseMessage.WarehouseCreated.ToText()));
        }
        catch (AppException ex)
        {
            return StatusCode(ex.StatusCode, ApiResponse<WarehouseResponse>.Fail(ex.Message, ex.StatusCode));
        }
        catch (Exception)
        {
            return StatusCode(500, ApiResponse<WarehouseResponse>.Fail(ResponseMessage.UnknownError.ToText(), 500));
        }
    }

    [HttpPut("{id:int}")]
    [Authorize(Policy = RightPolicies.Prefix + RightCodes.WarehousesEdit)]
    public async Task<ActionResult<ApiResponse<WarehouseResponse>>> Update(int id, WarehouseRequest request)
    {
        try
        {
            var warehouse = await _warehouses.UpdateAsync(id, request);
            return Ok(ApiResponse<WarehouseResponse>.Ok(warehouse, ResponseMessage.WarehouseUpdated.ToText()));
        }
        catch (AppException ex)
        {
            return StatusCode(ex.StatusCode, ApiResponse<WarehouseResponse>.Fail(ex.Message, ex.StatusCode));
        }
        catch (Exception)
        {
            return StatusCode(500, ApiResponse<WarehouseResponse>.Fail(ResponseMessage.UnknownError.ToText(), 500));
        }
    }

    [HttpPatch("{id:int}/activate")]
    [Authorize(Policy = RightPolicies.Prefix + RightCodes.WarehousesEdit)]
    public async Task<ActionResult<ApiResponse<object>>> Activate(int id)
    {
        try
        {
            await _warehouses.ActivateAsync(id);
            return Ok(ApiResponse<object>.Ok(null, ResponseMessage.WarehouseActivated.ToText()));
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
    [Authorize(Policy = RightPolicies.Prefix + RightCodes.WarehousesEdit)]
    public async Task<ActionResult<ApiResponse<object>>> Deactivate(int id)
    {
        try
        {
            await _warehouses.DeactivateAsync(id);
            return Ok(ApiResponse<object>.Ok(null, ResponseMessage.WarehouseDeactivated.ToText()));
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
