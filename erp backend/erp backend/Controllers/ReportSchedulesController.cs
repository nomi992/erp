using erp_backend.Auth;
using erp_backend.Exceptions;
using erp_backend.Messages;
using erp_backend.Models;
using erp_backend.Reporting;
using erp_backend.Reporting.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace erp_backend.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ReportSchedulesController : ControllerBase
{
    private readonly IReportScheduleRepository _schedules;

    public ReportSchedulesController(IReportScheduleRepository schedules)
    {
        _schedules = schedules;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<List<ReportSchedule>>>> GetAll()
    {
        try
        {
            var schedules = await _schedules.GetAllAsync();
            return Ok(ApiResponse<List<ReportSchedule>>.Ok(schedules, ResponseMessage.Success.ToText()));
        }
        catch (AppException ex)
        {
            return StatusCode(ex.StatusCode, ApiResponse<List<ReportSchedule>>.Fail(ex.Message, ex.StatusCode));
        }
        catch (Exception)
        {
            return StatusCode(500, ApiResponse<List<ReportSchedule>>.Fail(ResponseMessage.UnknownError.ToText(), 500));
        }
    }

    [HttpPost]
    [Authorize(Policy = RightPolicies.Prefix + RightCodes.ReportSchedulesCreate)]
    public async Task<ActionResult<ApiResponse<ReportSchedule>>> Create(ReportScheduleRequest request)
    {
        try
        {
            var schedule = await _schedules.CreateAsync(request);
            return Ok(ApiResponse<ReportSchedule>.Ok(schedule, ResponseMessage.ReportScheduleCreated.ToText()));
        }
        catch (AppException ex)
        {
            return StatusCode(ex.StatusCode, ApiResponse<ReportSchedule>.Fail(ex.Message, ex.StatusCode));
        }
        catch (Exception)
        {
            return StatusCode(500, ApiResponse<ReportSchedule>.Fail(ResponseMessage.UnknownError.ToText(), 500));
        }
    }

    [HttpPut("{id:int}")]
    [Authorize(Policy = RightPolicies.Prefix + RightCodes.ReportSchedulesEdit)]
    public async Task<ActionResult<ApiResponse<ReportSchedule>>> Update(int id, ReportScheduleRequest request)
    {
        try
        {
            var schedule = await _schedules.UpdateAsync(id, request);
            return Ok(ApiResponse<ReportSchedule>.Ok(schedule, ResponseMessage.ReportScheduleUpdated.ToText()));
        }
        catch (AppException ex)
        {
            return StatusCode(ex.StatusCode, ApiResponse<ReportSchedule>.Fail(ex.Message, ex.StatusCode));
        }
        catch (Exception)
        {
            return StatusCode(500, ApiResponse<ReportSchedule>.Fail(ResponseMessage.UnknownError.ToText(), 500));
        }
    }

    [HttpPatch("{id:int}/deactivate")]
    [Authorize(Policy = RightPolicies.Prefix + RightCodes.ReportSchedulesEdit)]
    public async Task<ActionResult<ApiResponse<object>>> Deactivate(int id)
    {
        try
        {
            await _schedules.DeactivateAsync(id);
            return Ok(ApiResponse<object>.Ok(null, ResponseMessage.ReportScheduleDeactivated.ToText()));
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

    [HttpPatch("{id:int}/activate")]
    [Authorize(Policy = RightPolicies.Prefix + RightCodes.ReportSchedulesEdit)]
    public async Task<ActionResult<ApiResponse<object>>> Activate(int id)
    {
        try
        {
            await _schedules.ActivateAsync(id);
            return Ok(ApiResponse<object>.Ok(null, ResponseMessage.ReportScheduleActivated.ToText()));
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
    [Authorize(Policy = RightPolicies.Prefix + RightCodes.ReportSchedulesDelete)]
    public async Task<ActionResult<ApiResponse<object>>> Delete(int id)
    {
        try
        {
            await _schedules.DeleteAsync(id);
            return Ok(ApiResponse<object>.Ok(null, ResponseMessage.ReportScheduleDeleted.ToText()));
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

    [HttpPost("{id:int}/run-now")]
    [Authorize(Policy = RightPolicies.Prefix + RightCodes.ReportSchedulesRunNow)]
    public async Task<IActionResult> RunNow(int id)
    {
        try
        {
            var (bytes, contentType, fileName) = await _schedules.RunNowAsync(id);
            return File(bytes, contentType, fileName);
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
