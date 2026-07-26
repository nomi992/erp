using erp_backend.Auth;
using erp_backend.Exceptions;
using erp_backend.Messages;
using erp_backend.Models;
using erp_backend.Vouchers;
using erp_backend.Vouchers.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace erp_backend.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class RecurringVoucherTemplatesController : ControllerBase
{
    private readonly IRecurringVoucherTemplateRepository _templates;

    public RecurringVoucherTemplatesController(IRecurringVoucherTemplateRepository templates)
    {
        _templates = templates;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<List<RecurringVoucherTemplateResponse>>>> GetAll()
    {
        try
        {
            var templates = await _templates.GetAllAsync();
            return Ok(ApiResponse<List<RecurringVoucherTemplateResponse>>.Ok(templates, ResponseMessage.Success.ToText()));
        }
        catch (AppException ex)
        {
            return StatusCode(ex.StatusCode, ApiResponse<List<RecurringVoucherTemplateResponse>>.Fail(ex.Message, ex.StatusCode));
        }
        catch (Exception)
        {
            return StatusCode(500, ApiResponse<List<RecurringVoucherTemplateResponse>>.Fail(ResponseMessage.UnknownError.ToText(), 500));
        }
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<ApiResponse<RecurringVoucherTemplateResponse>>> GetById(int id)
    {
        try
        {
            var template = await _templates.GetByIdAsync(id);
            return Ok(ApiResponse<RecurringVoucherTemplateResponse>.Ok(template, ResponseMessage.Success.ToText()));
        }
        catch (AppException ex)
        {
            return StatusCode(ex.StatusCode, ApiResponse<RecurringVoucherTemplateResponse>.Fail(ex.Message, ex.StatusCode));
        }
        catch (Exception)
        {
            return StatusCode(500, ApiResponse<RecurringVoucherTemplateResponse>.Fail(ResponseMessage.UnknownError.ToText(), 500));
        }
    }

    [HttpPost]
    [Authorize(Policy = RightPolicies.Prefix + RightCodes.RecurringVoucherTemplatesCreate)]
    public async Task<ActionResult<ApiResponse<RecurringVoucherTemplateResponse>>> Create(RecurringVoucherTemplateRequest request)
    {
        try
        {
            var template = await _templates.CreateAsync(request);
            return Ok(ApiResponse<RecurringVoucherTemplateResponse>.Ok(template, ResponseMessage.TemplateCreated.ToText()));
        }
        catch (AppException ex)
        {
            return StatusCode(ex.StatusCode, ApiResponse<RecurringVoucherTemplateResponse>.Fail(ex.Message, ex.StatusCode));
        }
        catch (Exception)
        {
            return StatusCode(500, ApiResponse<RecurringVoucherTemplateResponse>.Fail(ResponseMessage.UnknownError.ToText(), 500));
        }
    }

    [HttpPut("{id:int}")]
    [Authorize(Policy = RightPolicies.Prefix + RightCodes.RecurringVoucherTemplatesEdit)]
    public async Task<ActionResult<ApiResponse<RecurringVoucherTemplateResponse>>> Update(int id, RecurringVoucherTemplateRequest request)
    {
        try
        {
            var template = await _templates.UpdateAsync(id, request);
            return Ok(ApiResponse<RecurringVoucherTemplateResponse>.Ok(template, ResponseMessage.TemplateUpdated.ToText()));
        }
        catch (AppException ex)
        {
            return StatusCode(ex.StatusCode, ApiResponse<RecurringVoucherTemplateResponse>.Fail(ex.Message, ex.StatusCode));
        }
        catch (Exception)
        {
            return StatusCode(500, ApiResponse<RecurringVoucherTemplateResponse>.Fail(ResponseMessage.UnknownError.ToText(), 500));
        }
    }

    [HttpPatch("{id:int}/deactivate")]
    [Authorize(Policy = RightPolicies.Prefix + RightCodes.RecurringVoucherTemplatesEdit)]
    public async Task<ActionResult<ApiResponse<object>>> Deactivate(int id)
    {
        try
        {
            await _templates.DeactivateAsync(id);
            return Ok(ApiResponse<object>.Ok(null, ResponseMessage.TemplateDeactivated.ToText()));
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
    [Authorize(Policy = RightPolicies.Prefix + RightCodes.RecurringVoucherTemplatesEdit)]
    public async Task<ActionResult<ApiResponse<object>>> Activate(int id)
    {
        try
        {
            await _templates.ActivateAsync(id);
            return Ok(ApiResponse<object>.Ok(null, ResponseMessage.TemplateActivated.ToText()));
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

    [HttpPost("{id:int}/generate-now")]
    [Authorize(Policy = RightPolicies.Prefix + RightCodes.RecurringVoucherTemplatesGenerateNow)]
    public async Task<ActionResult<ApiResponse<object>>> GenerateNow(int id)
    {
        try
        {
            var result = await _templates.GenerateNowAsync(id, User.Identity?.Name ?? "system");
            var message = ResponseMessage.TemplateGenerated.ToText(result.VoucherNo, result.NextRunDate.ToString("yyyy-MM-dd"));
            return Ok(ApiResponse<object>.Ok(new { Id = result.VoucherId, result.VoucherNo }, message));
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
