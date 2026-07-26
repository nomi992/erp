using erp_backend.Auth;
using erp_backend.Exceptions;
using erp_backend.Messages;
using erp_backend.Models;
using erp_backend.UnitsOfMeasure;
using erp_backend.UnitsOfMeasure.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace erp_backend.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class UnitsOfMeasureController : ControllerBase
{
    private readonly IUnitOfMeasureRepository _units;

    public UnitsOfMeasureController(IUnitOfMeasureRepository units)
    {
        _units = units;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<List<UnitOfMeasureResponse>>>> GetAll()
    {
        try
        {
            var units = await _units.GetAllAsync();
            return Ok(ApiResponse<List<UnitOfMeasureResponse>>.Ok(units, ResponseMessage.Success.ToText()));
        }
        catch (AppException ex)
        {
            return StatusCode(ex.StatusCode, ApiResponse<List<UnitOfMeasureResponse>>.Fail(ex.Message, ex.StatusCode));
        }
        catch (Exception)
        {
            return StatusCode(500, ApiResponse<List<UnitOfMeasureResponse>>.Fail(ResponseMessage.UnknownError.ToText(), 500));
        }
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<ApiResponse<UnitOfMeasureResponse>>> GetById(int id)
    {
        try
        {
            var unit = await _units.GetByIdAsync(id);
            return Ok(ApiResponse<UnitOfMeasureResponse>.Ok(unit, ResponseMessage.Success.ToText()));
        }
        catch (AppException ex)
        {
            return StatusCode(ex.StatusCode, ApiResponse<UnitOfMeasureResponse>.Fail(ex.Message, ex.StatusCode));
        }
        catch (Exception)
        {
            return StatusCode(500, ApiResponse<UnitOfMeasureResponse>.Fail(ResponseMessage.UnknownError.ToText(), 500));
        }
    }

    [HttpPost]
    [Authorize(Policy = RightPolicies.Prefix + RightCodes.UnitsOfMeasureCreate)]
    public async Task<ActionResult<ApiResponse<UnitOfMeasureResponse>>> Create(UnitOfMeasureRequest request)
    {
        try
        {
            var unit = await _units.CreateAsync(request);
            return Ok(ApiResponse<UnitOfMeasureResponse>.Ok(unit, ResponseMessage.UnitOfMeasureCreated.ToText()));
        }
        catch (AppException ex)
        {
            return StatusCode(ex.StatusCode, ApiResponse<UnitOfMeasureResponse>.Fail(ex.Message, ex.StatusCode));
        }
        catch (Exception)
        {
            return StatusCode(500, ApiResponse<UnitOfMeasureResponse>.Fail(ResponseMessage.UnknownError.ToText(), 500));
        }
    }

    [HttpPut("{id:int}")]
    [Authorize(Policy = RightPolicies.Prefix + RightCodes.UnitsOfMeasureEdit)]
    public async Task<ActionResult<ApiResponse<UnitOfMeasureResponse>>> Update(int id, UnitOfMeasureRequest request)
    {
        try
        {
            var unit = await _units.UpdateAsync(id, request);
            return Ok(ApiResponse<UnitOfMeasureResponse>.Ok(unit, ResponseMessage.UnitOfMeasureUpdated.ToText()));
        }
        catch (AppException ex)
        {
            return StatusCode(ex.StatusCode, ApiResponse<UnitOfMeasureResponse>.Fail(ex.Message, ex.StatusCode));
        }
        catch (Exception)
        {
            return StatusCode(500, ApiResponse<UnitOfMeasureResponse>.Fail(ResponseMessage.UnknownError.ToText(), 500));
        }
    }

    [HttpPatch("{id:int}/activate")]
    [Authorize(Policy = RightPolicies.Prefix + RightCodes.UnitsOfMeasureEdit)]
    public async Task<ActionResult<ApiResponse<object>>> Activate(int id)
    {
        try
        {
            await _units.ActivateAsync(id);
            return Ok(ApiResponse<object>.Ok(null, ResponseMessage.UnitOfMeasureActivated.ToText()));
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
    [Authorize(Policy = RightPolicies.Prefix + RightCodes.UnitsOfMeasureEdit)]
    public async Task<ActionResult<ApiResponse<object>>> Deactivate(int id)
    {
        try
        {
            await _units.DeactivateAsync(id);
            return Ok(ApiResponse<object>.Ok(null, ResponseMessage.UnitOfMeasureDeactivated.ToText()));
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
