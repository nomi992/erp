using erp_backend.Auth;
using erp_backend.CostCenters;
using erp_backend.CostCenters.Dtos;
using erp_backend.Exceptions;
using erp_backend.Messages;
using erp_backend.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace erp_backend.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class CostCentersController : ControllerBase
{
    private readonly ICostCenterRepository _costCenters;

    public CostCentersController(ICostCenterRepository costCenters)
    {
        _costCenters = costCenters;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<List<CostCenterResponse>>>> GetAll()
    {
        try
        {
            var costCenters = await _costCenters.GetAllAsync();
            return Ok(ApiResponse<List<CostCenterResponse>>.Ok(costCenters, ResponseMessage.Success.ToText()));
        }
        catch (AppException ex)
        {
            return StatusCode(ex.StatusCode, ApiResponse<List<CostCenterResponse>>.Fail(ex.Message, ex.StatusCode));
        }
        catch (Exception)
        {
            return StatusCode(500, ApiResponse<List<CostCenterResponse>>.Fail(ResponseMessage.UnknownError.ToText(), 500));
        }
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<ApiResponse<CostCenterResponse>>> GetById(int id)
    {
        try
        {
            var costCenter = await _costCenters.GetByIdAsync(id);
            return Ok(ApiResponse<CostCenterResponse>.Ok(costCenter, ResponseMessage.Success.ToText()));
        }
        catch (AppException ex)
        {
            return StatusCode(ex.StatusCode, ApiResponse<CostCenterResponse>.Fail(ex.Message, ex.StatusCode));
        }
        catch (Exception)
        {
            return StatusCode(500, ApiResponse<CostCenterResponse>.Fail(ResponseMessage.UnknownError.ToText(), 500));
        }
    }

    [HttpPost]
    [Authorize(Policy = RightPolicies.Prefix + RightCodes.CostCentersCreate)]
    public async Task<ActionResult<ApiResponse<CostCenterResponse>>> Create(CostCenterRequest request)
    {
        try
        {
            var costCenter = await _costCenters.CreateAsync(request);
            return Ok(ApiResponse<CostCenterResponse>.Ok(costCenter, ResponseMessage.CostCenterCreated.ToText()));
        }
        catch (AppException ex)
        {
            return StatusCode(ex.StatusCode, ApiResponse<CostCenterResponse>.Fail(ex.Message, ex.StatusCode));
        }
        catch (Exception)
        {
            return StatusCode(500, ApiResponse<CostCenterResponse>.Fail(ResponseMessage.UnknownError.ToText(), 500));
        }
    }

    [HttpPut("{id:int}")]
    [Authorize(Policy = RightPolicies.Prefix + RightCodes.CostCentersEdit)]
    public async Task<ActionResult<ApiResponse<CostCenterResponse>>> Update(int id, CostCenterRequest request)
    {
        try
        {
            var costCenter = await _costCenters.UpdateAsync(id, request);
            return Ok(ApiResponse<CostCenterResponse>.Ok(costCenter, ResponseMessage.CostCenterUpdated.ToText()));
        }
        catch (AppException ex)
        {
            return StatusCode(ex.StatusCode, ApiResponse<CostCenterResponse>.Fail(ex.Message, ex.StatusCode));
        }
        catch (Exception)
        {
            return StatusCode(500, ApiResponse<CostCenterResponse>.Fail(ResponseMessage.UnknownError.ToText(), 500));
        }
    }

    [HttpPatch("{id:int}/activate")]
    [Authorize(Policy = RightPolicies.Prefix + RightCodes.CostCentersEdit)]
    public async Task<ActionResult<ApiResponse<object>>> Activate(int id)
    {
        try
        {
            await _costCenters.ActivateAsync(id);
            return Ok(ApiResponse<object>.Ok(null, ResponseMessage.CostCenterActivated.ToText()));
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
    [Authorize(Policy = RightPolicies.Prefix + RightCodes.CostCentersEdit)]
    public async Task<ActionResult<ApiResponse<object>>> Deactivate(int id)
    {
        try
        {
            await _costCenters.DeactivateAsync(id);
            return Ok(ApiResponse<object>.Ok(null, ResponseMessage.CostCenterDeactivated.ToText()));
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

    [HttpDelete("{id:int}")]
    [Authorize(Policy = RightPolicies.Prefix + RightCodes.CostCentersDelete)]
    public async Task<ActionResult<ApiResponse<object>>> Delete(int id)
    {
        try
        {
            await _costCenters.DeleteAsync(id);
            return Ok(ApiResponse<object>.Ok(null, ResponseMessage.CostCenterDeleted.ToText()));
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
