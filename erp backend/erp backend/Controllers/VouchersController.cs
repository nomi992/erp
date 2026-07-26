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
public class VouchersController : ControllerBase
{
    private readonly IVoucherRepository _vouchers;

    public VouchersController(IVoucherRepository vouchers)
    {
        _vouchers = vouchers;
    }

    private string CurrentUsername => User.Identity?.Name ?? "system";

    [HttpGet]
    public async Task<ActionResult<ApiResponse<List<VoucherListItemResponse>>>> GetAll(
        [FromQuery] VoucherType? type, [FromQuery] VoucherStatus? status, [FromQuery] DateTime? from, [FromQuery] DateTime? to)
    {
        try
        {
            var vouchers = await _vouchers.GetAllAsync(type, status, from, to);
            return Ok(ApiResponse<List<VoucherListItemResponse>>.Ok(vouchers, ResponseMessage.Success.ToText()));
        }
        catch (AppException ex)
        {
            return StatusCode(ex.StatusCode, ApiResponse<List<VoucherListItemResponse>>.Fail(ex.Message, ex.StatusCode));
        }
        catch (Exception)
        {
            return StatusCode(500, ApiResponse<List<VoucherListItemResponse>>.Fail(ResponseMessage.UnknownError.ToText(), 500));
        }
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<ApiResponse<VoucherResponse>>> GetById(int id)
    {
        try
        {
            var voucher = await _vouchers.GetByIdAsync(id);
            return Ok(ApiResponse<VoucherResponse>.Ok(voucher, ResponseMessage.Success.ToText()));
        }
        catch (AppException ex)
        {
            return StatusCode(ex.StatusCode, ApiResponse<VoucherResponse>.Fail(ex.Message, ex.StatusCode));
        }
        catch (Exception)
        {
            return StatusCode(500, ApiResponse<VoucherResponse>.Fail(ResponseMessage.UnknownError.ToText(), 500));
        }
    }

    [HttpPost]
    [Authorize(Policy = RightPolicies.Prefix + RightCodes.VouchersCreate)]
    public async Task<ActionResult<ApiResponse<VoucherResponse>>> Create(CreateVoucherRequest request)
    {
        try
        {
            var voucher = await _vouchers.CreateAsync(request, CurrentUsername);
            return Ok(ApiResponse<VoucherResponse>.Ok(voucher, ResponseMessage.VoucherCreated.ToText()));
        }
        catch (AppException ex)
        {
            return StatusCode(ex.StatusCode, ApiResponse<VoucherResponse>.Fail(ex.Message, ex.StatusCode));
        }
        catch (Exception)
        {
            return StatusCode(500, ApiResponse<VoucherResponse>.Fail(ResponseMessage.UnknownError.ToText(), 500));
        }
    }

    [HttpPut("{id:int}")]
    [Authorize(Policy = RightPolicies.Prefix + RightCodes.VouchersEdit)]
    public async Task<ActionResult<ApiResponse<VoucherResponse>>> Update(int id, CreateVoucherRequest request)
    {
        try
        {
            var voucher = await _vouchers.UpdateAsync(id, request);
            return Ok(ApiResponse<VoucherResponse>.Ok(voucher, ResponseMessage.VoucherUpdated.ToText()));
        }
        catch (AppException ex)
        {
            return StatusCode(ex.StatusCode, ApiResponse<VoucherResponse>.Fail(ex.Message, ex.StatusCode));
        }
        catch (Exception)
        {
            return StatusCode(500, ApiResponse<VoucherResponse>.Fail(ResponseMessage.UnknownError.ToText(), 500));
        }
    }

    [HttpPost("{id:int}/submit")]
    [Authorize(Policy = RightPolicies.Prefix + RightCodes.VouchersSubmit)]
    public async Task<ActionResult<ApiResponse<object>>> Submit(int id)
    {
        try
        {
            await _vouchers.SubmitAsync(id);
            return Ok(ApiResponse<object>.Ok(null, ResponseMessage.VoucherSubmitted.ToText()));
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

    [HttpPost("{id:int}/approve")]
    [Authorize(Policy = RightPolicies.Prefix + RightCodes.VouchersApprove)]
    public async Task<ActionResult<ApiResponse<object>>> Approve(int id)
    {
        try
        {
            await _vouchers.ApproveAsync(id, CurrentUsername);
            return Ok(ApiResponse<object>.Ok(null, ResponseMessage.VoucherApproved.ToText()));
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

    [HttpPost("{id:int}/reject")]
    [Authorize(Policy = RightPolicies.Prefix + RightCodes.VouchersReject)]
    public async Task<ActionResult<ApiResponse<object>>> Reject(int id)
    {
        try
        {
            await _vouchers.RejectAsync(id);
            return Ok(ApiResponse<object>.Ok(null, ResponseMessage.VoucherRejected.ToText()));
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

    [HttpPost("{id:int}/reverse")]
    [Authorize(Policy = RightPolicies.Prefix + RightCodes.VouchersReverse)]
    public async Task<ActionResult<ApiResponse<VoucherResponse>>> Reverse(int id)
    {
        try
        {
            var voucher = await _vouchers.ReverseAsync(id, CurrentUsername);
            return Ok(ApiResponse<VoucherResponse>.Ok(voucher, ResponseMessage.VoucherReversed.ToText()));
        }
        catch (AppException ex)
        {
            return StatusCode(ex.StatusCode, ApiResponse<VoucherResponse>.Fail(ex.Message, ex.StatusCode));
        }
        catch (Exception)
        {
            return StatusCode(500, ApiResponse<VoucherResponse>.Fail(ResponseMessage.UnknownError.ToText(), 500));
        }
    }

    [HttpPost("{id:int}/attachments")]
    [RequestSizeLimit(20_000_000)]
    [Authorize(Policy = RightPolicies.Prefix + RightCodes.VouchersManageAttachments)]
    public async Task<ActionResult<ApiResponse<VoucherAttachmentResponse>>> UploadAttachment(int id, IFormFile file)
    {
        try
        {
            var attachment = await _vouchers.UploadAttachmentAsync(id, file);
            return Ok(ApiResponse<VoucherAttachmentResponse>.Ok(attachment, ResponseMessage.AttachmentUploaded.ToText()));
        }
        catch (AppException ex)
        {
            return StatusCode(ex.StatusCode, ApiResponse<VoucherAttachmentResponse>.Fail(ex.Message, ex.StatusCode));
        }
        catch (Exception)
        {
            return StatusCode(500, ApiResponse<VoucherAttachmentResponse>.Fail(ResponseMessage.UnknownError.ToText(), 500));
        }
    }

    [HttpGet("{id:int}/attachments/{attachmentId:int}/download")]
    public async Task<IActionResult> DownloadAttachment(int id, int attachmentId)
    {
        try
        {
            var (bytes, contentType, fileName) = await _vouchers.DownloadAttachmentAsync(id, attachmentId);
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

    [HttpDelete("{id:int}/attachments/{attachmentId:int}")]
    [Authorize(Policy = RightPolicies.Prefix + RightCodes.VouchersManageAttachments)]
    public async Task<ActionResult<ApiResponse<object>>> DeleteAttachment(int id, int attachmentId)
    {
        try
        {
            await _vouchers.DeleteAttachmentAsync(id, attachmentId);
            return Ok(ApiResponse<object>.Ok(null, ResponseMessage.AttachmentDeleted.ToText()));
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
