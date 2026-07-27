using erp_backend.Auth;
using erp_backend.Exceptions;
using erp_backend.Messages;
using erp_backend.Models;
using erp_backend.StockAdjustments;
using erp_backend.StockAdjustments.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace erp_backend.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class StockAdjustmentsController : ControllerBase
{
    private readonly IStockAdjustmentRepository _adjustments;

    public StockAdjustmentsController(IStockAdjustmentRepository adjustments)
    {
        _adjustments = adjustments;
    }

    private string CurrentUsername => User.Identity?.Name ?? "system";

    [HttpGet]
    public async Task<ActionResult<ApiResponse<PagedResult<StockAdjustmentListItemResponse>>>> GetAll(
        [FromQuery] AdjustmentReasonCode? reasonCode, [FromQuery] StockAdjustmentStatus? status,
        [FromQuery] DateTime? from, [FromQuery] DateTime? to,
        [FromQuery] string? search, [FromQuery] string? sortBy, [FromQuery] string? sortDirection,
        [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 25)
    {
        try
        {
            var adjustments = await _adjustments.GetAllAsync(reasonCode, status, from, to, search, sortBy, sortDirection, pageNumber, pageSize);
            return Ok(ApiResponse<PagedResult<StockAdjustmentListItemResponse>>.Ok(adjustments, ResponseMessage.Success.ToText()));
        }
        catch (AppException ex)
        {
            return StatusCode(ex.StatusCode, ApiResponse<PagedResult<StockAdjustmentListItemResponse>>.Fail(ex.Message, ex.StatusCode));
        }
        catch (Exception)
        {
            return StatusCode(500, ApiResponse<PagedResult<StockAdjustmentListItemResponse>>.Fail(ResponseMessage.UnknownError.ToText(), 500));
        }
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<ApiResponse<StockAdjustmentResponse>>> GetById(int id)
    {
        try
        {
            var adjustment = await _adjustments.GetByIdAsync(id);
            return Ok(ApiResponse<StockAdjustmentResponse>.Ok(adjustment, ResponseMessage.Success.ToText()));
        }
        catch (AppException ex)
        {
            return StatusCode(ex.StatusCode, ApiResponse<StockAdjustmentResponse>.Fail(ex.Message, ex.StatusCode));
        }
        catch (Exception)
        {
            return StatusCode(500, ApiResponse<StockAdjustmentResponse>.Fail(ResponseMessage.UnknownError.ToText(), 500));
        }
    }

    [HttpPost]
    [Authorize(Policy = RightPolicies.Prefix + RightCodes.StockAdjustmentsCreate)]
    public async Task<ActionResult<ApiResponse<StockAdjustmentResponse>>> Create(StockAdjustmentRequest request)
    {
        try
        {
            var adjustment = await _adjustments.CreateAsync(request, CurrentUsername);
            return Ok(ApiResponse<StockAdjustmentResponse>.Ok(adjustment, ResponseMessage.StockAdjustmentCreated.ToText()));
        }
        catch (AppException ex)
        {
            return StatusCode(ex.StatusCode, ApiResponse<StockAdjustmentResponse>.Fail(ex.Message, ex.StatusCode));
        }
        catch (Exception)
        {
            return StatusCode(500, ApiResponse<StockAdjustmentResponse>.Fail(ResponseMessage.UnknownError.ToText(), 500));
        }
    }

    [HttpPost("{id:int}/submit")]
    [Authorize(Policy = RightPolicies.Prefix + RightCodes.StockAdjustmentsSubmit)]
    public async Task<ActionResult<ApiResponse<object>>> Submit(int id)
    {
        try
        {
            await _adjustments.SubmitAsync(id);
            return Ok(ApiResponse<object>.Ok(null, ResponseMessage.StockAdjustmentSubmitted.ToText()));
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
    [Authorize(Policy = RightPolicies.Prefix + RightCodes.StockAdjustmentsApprove)]
    public async Task<ActionResult<ApiResponse<object>>> Approve(int id)
    {
        try
        {
            await _adjustments.ApproveAsync(id, CurrentUsername);
            return Ok(ApiResponse<object>.Ok(null, ResponseMessage.StockAdjustmentApproved.ToText()));
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
    [Authorize(Policy = RightPolicies.Prefix + RightCodes.StockAdjustmentsReject)]
    public async Task<ActionResult<ApiResponse<object>>> Reject(int id)
    {
        try
        {
            await _adjustments.RejectAsync(id);
            return Ok(ApiResponse<object>.Ok(null, ResponseMessage.StockAdjustmentRejected.ToText()));
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
