using erp_backend.Auth;
using erp_backend.Exceptions;
using erp_backend.Messages;
using erp_backend.Models;
using erp_backend.Transfers;
using erp_backend.Transfers.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace erp_backend.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class StockTransfersController : ControllerBase
{
    private readonly IStockTransferRepository _transfers;

    public StockTransfersController(IStockTransferRepository transfers)
    {
        _transfers = transfers;
    }

    private string CurrentUsername => User.Identity?.Name ?? "system";

    [HttpGet]
    public async Task<ActionResult<ApiResponse<PagedResult<StockTransferListItemResponse>>>> GetAll(
        [FromQuery] StockTransferStatus? status, [FromQuery] DateTime? from, [FromQuery] DateTime? to,
        [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 25)
    {
        try
        {
            var transfers = await _transfers.GetAllAsync(status, from, to, pageNumber, pageSize);
            return Ok(ApiResponse<PagedResult<StockTransferListItemResponse>>.Ok(transfers, ResponseMessage.Success.ToText()));
        }
        catch (AppException ex)
        {
            return StatusCode(ex.StatusCode, ApiResponse<PagedResult<StockTransferListItemResponse>>.Fail(ex.Message, ex.StatusCode));
        }
        catch (Exception)
        {
            return StatusCode(500, ApiResponse<PagedResult<StockTransferListItemResponse>>.Fail(ResponseMessage.UnknownError.ToText(), 500));
        }
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<ApiResponse<StockTransferResponse>>> GetById(int id)
    {
        try
        {
            var transfer = await _transfers.GetByIdAsync(id);
            return Ok(ApiResponse<StockTransferResponse>.Ok(transfer, ResponseMessage.Success.ToText()));
        }
        catch (AppException ex)
        {
            return StatusCode(ex.StatusCode, ApiResponse<StockTransferResponse>.Fail(ex.Message, ex.StatusCode));
        }
        catch (Exception)
        {
            return StatusCode(500, ApiResponse<StockTransferResponse>.Fail(ResponseMessage.UnknownError.ToText(), 500));
        }
    }

    [HttpPost]
    [Authorize(Policy = RightPolicies.Prefix + RightCodes.StockTransfersCreate)]
    public async Task<ActionResult<ApiResponse<StockTransferResponse>>> Create(StockTransferRequest request)
    {
        try
        {
            var transfer = await _transfers.CreateAsync(request, CurrentUsername);
            return Ok(ApiResponse<StockTransferResponse>.Ok(transfer, ResponseMessage.StockTransferCreated.ToText()));
        }
        catch (AppException ex)
        {
            return StatusCode(ex.StatusCode, ApiResponse<StockTransferResponse>.Fail(ex.Message, ex.StatusCode));
        }
        catch (Exception)
        {
            return StatusCode(500, ApiResponse<StockTransferResponse>.Fail(ResponseMessage.UnknownError.ToText(), 500));
        }
    }

    [HttpPost("{id:int}/submit")]
    [Authorize(Policy = RightPolicies.Prefix + RightCodes.StockTransfersSubmit)]
    public async Task<ActionResult<ApiResponse<object>>> Submit(int id)
    {
        try
        {
            await _transfers.SubmitAsync(id);
            return Ok(ApiResponse<object>.Ok(null, ResponseMessage.StockTransferSubmitted.ToText()));
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
    [Authorize(Policy = RightPolicies.Prefix + RightCodes.StockTransfersApprove)]
    public async Task<ActionResult<ApiResponse<object>>> Approve(int id)
    {
        try
        {
            await _transfers.ApproveAsync(id, CurrentUsername);
            return Ok(ApiResponse<object>.Ok(null, ResponseMessage.StockTransferApproved.ToText()));
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
    [Authorize(Policy = RightPolicies.Prefix + RightCodes.StockTransfersReject)]
    public async Task<ActionResult<ApiResponse<object>>> Reject(int id)
    {
        try
        {
            await _transfers.RejectAsync(id);
            return Ok(ApiResponse<object>.Ok(null, ResponseMessage.StockTransferRejected.ToText()));
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

    [HttpPost("{id:int}/receive")]
    [Authorize(Policy = RightPolicies.Prefix + RightCodes.StockTransfersReceive)]
    public async Task<ActionResult<ApiResponse<StockTransferResponse>>> Receive(int id, StockTransferReceiveRequest request)
    {
        try
        {
            var transfer = await _transfers.ReceiveAsync(id, request, CurrentUsername);
            return Ok(ApiResponse<StockTransferResponse>.Ok(transfer, ResponseMessage.StockTransferReceived.ToText()));
        }
        catch (AppException ex)
        {
            return StatusCode(ex.StatusCode, ApiResponse<StockTransferResponse>.Fail(ex.Message, ex.StatusCode));
        }
        catch (Exception)
        {
            return StatusCode(500, ApiResponse<StockTransferResponse>.Fail(ResponseMessage.UnknownError.ToText(), 500));
        }
    }
}
