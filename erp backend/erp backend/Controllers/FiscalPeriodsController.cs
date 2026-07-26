using erp_backend.Auth;
using erp_backend.Exceptions;
using erp_backend.FiscalPeriods;
using erp_backend.FiscalPeriods.Dtos;
using erp_backend.Messages;
using erp_backend.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace erp_backend.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class FiscalPeriodsController : ControllerBase
{
    private readonly IFiscalPeriodRepository _periods;

    public FiscalPeriodsController(IFiscalPeriodRepository periods)
    {
        _periods = periods;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<List<FiscalPeriod>>>> GetAll()
    {
        try
        {
            var periods = await _periods.GetAllAsync();
            return Ok(ApiResponse<List<FiscalPeriod>>.Ok(periods, ResponseMessage.Success.ToText()));
        }
        catch (AppException ex)
        {
            return StatusCode(ex.StatusCode, ApiResponse<List<FiscalPeriod>>.Fail(ex.Message, ex.StatusCode));
        }
        catch (Exception)
        {
            return StatusCode(500, ApiResponse<List<FiscalPeriod>>.Fail(ResponseMessage.UnknownError.ToText(), 500));
        }
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<ApiResponse<FiscalPeriod>>> GetById(int id)
    {
        try
        {
            var period = await _periods.GetByIdAsync(id);
            return Ok(ApiResponse<FiscalPeriod>.Ok(period, ResponseMessage.Success.ToText()));
        }
        catch (AppException ex)
        {
            return StatusCode(ex.StatusCode, ApiResponse<FiscalPeriod>.Fail(ex.Message, ex.StatusCode));
        }
        catch (Exception)
        {
            return StatusCode(500, ApiResponse<FiscalPeriod>.Fail(ResponseMessage.UnknownError.ToText(), 500));
        }
    }

    [HttpPost]
    [Authorize(Policy = RightPolicies.Prefix + RightCodes.FiscalPeriodsCreate)]
    public async Task<ActionResult<ApiResponse<FiscalPeriod>>> Create(FiscalPeriodRequest request)
    {
        try
        {
            var period = await _periods.CreateAsync(request);
            return Ok(ApiResponse<FiscalPeriod>.Ok(period, ResponseMessage.FiscalPeriodCreated.ToText()));
        }
        catch (AppException ex)
        {
            return StatusCode(ex.StatusCode, ApiResponse<FiscalPeriod>.Fail(ex.Message, ex.StatusCode));
        }
        catch (Exception)
        {
            return StatusCode(500, ApiResponse<FiscalPeriod>.Fail(ResponseMessage.UnknownError.ToText(), 500));
        }
    }

    [HttpPatch("{id:int}/close")]
    [Authorize(Policy = RightPolicies.Prefix + RightCodes.FiscalPeriodsClose)]
    public async Task<ActionResult<ApiResponse<object>>> Close(int id)
    {
        try
        {
            await _periods.CloseAsync(id);
            return Ok(ApiResponse<object>.Ok(null, ResponseMessage.FiscalPeriodClosed.ToText()));
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

    [HttpPatch("{id:int}/open")]
    [Authorize(Policy = RightPolicies.Prefix + RightCodes.FiscalPeriodsReopen)]
    public async Task<ActionResult<ApiResponse<object>>> Open(int id)
    {
        try
        {
            await _periods.OpenAsync(id);
            return Ok(ApiResponse<object>.Ok(null, ResponseMessage.FiscalPeriodReopened.ToText()));
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
