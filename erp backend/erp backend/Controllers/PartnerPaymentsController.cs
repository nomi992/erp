using erp_backend.Auth;
using erp_backend.Exceptions;
using erp_backend.Messages;
using erp_backend.Models;
using erp_backend.PartnerPayments;
using erp_backend.PartnerPayments.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace erp_backend.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class PartnerPaymentsController : ControllerBase
{
    private readonly IPartnerPaymentRepository _payments;
    private readonly IAuthorizationService _authorizationService;

    public PartnerPaymentsController(IPartnerPaymentRepository payments, IAuthorizationService authorizationService)
    {
        _payments = payments;
        _authorizationService = authorizationService;
    }

    private string CurrentUsername => User.Identity?.Name ?? "system";

    [HttpGet]
    public async Task<ActionResult<ApiResponse<PagedResult<PartnerPaymentListItemResponse>>>> GetAll(
        [FromQuery] PaymentDirection? direction, [FromQuery] int? partnerId, [FromQuery] PartnerPaymentStatus? status,
        [FromQuery] DateTime? from, [FromQuery] DateTime? to,
        [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 25)
    {
        try
        {
            var payments = await _payments.GetAllAsync(direction, partnerId, status, from, to, pageNumber, pageSize);
            return Ok(ApiResponse<PagedResult<PartnerPaymentListItemResponse>>.Ok(payments, ResponseMessage.Success.ToText()));
        }
        catch (AppException ex)
        {
            return StatusCode(ex.StatusCode, ApiResponse<PagedResult<PartnerPaymentListItemResponse>>.Fail(ex.Message, ex.StatusCode));
        }
        catch (Exception)
        {
            return StatusCode(500, ApiResponse<PagedResult<PartnerPaymentListItemResponse>>.Fail(ResponseMessage.UnknownError.ToText(), 500));
        }
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<ApiResponse<PartnerPaymentResponse>>> GetById(int id)
    {
        try
        {
            var payment = await _payments.GetByIdAsync(id);
            return Ok(ApiResponse<PartnerPaymentResponse>.Ok(payment, ResponseMessage.Success.ToText()));
        }
        catch (AppException ex)
        {
            return StatusCode(ex.StatusCode, ApiResponse<PartnerPaymentResponse>.Fail(ex.Message, ex.StatusCode));
        }
        catch (Exception)
        {
            return StatusCode(500, ApiResponse<PartnerPaymentResponse>.Fail(ResponseMessage.UnknownError.ToText(), 500));
        }
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<PartnerPaymentResponse>>> Create(PartnerPaymentRequest request)
    {
        try
        {
            if (!await HasRightAsync(PartnerPaymentRightResolver.Resolve(request.Direction, PaymentAction.Create)))
            {
                return Forbid();
            }

            var payment = await _payments.CreateAsync(request, CurrentUsername);
            return Ok(ApiResponse<PartnerPaymentResponse>.Ok(payment, CreatedMessageFor(request.Direction).ToText()));
        }
        catch (AppException ex)
        {
            return StatusCode(ex.StatusCode, ApiResponse<PartnerPaymentResponse>.Fail(ex.Message, ex.StatusCode));
        }
        catch (Exception)
        {
            return StatusCode(500, ApiResponse<PartnerPaymentResponse>.Fail(ResponseMessage.UnknownError.ToText(), 500));
        }
    }

    [HttpPost("{id:int}/submit")]
    public async Task<ActionResult<ApiResponse<object>>> Submit(int id)
    {
        try
        {
            var direction = await _payments.GetDirectionAsync(id);
            if (!await HasRightAsync(PartnerPaymentRightResolver.Resolve(direction, PaymentAction.Submit)))
            {
                return Forbid();
            }

            await _payments.SubmitAsync(id);
            return Ok(ApiResponse<object>.Ok(null, SubmittedMessageFor(direction).ToText()));
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
    public async Task<ActionResult<ApiResponse<object>>> Approve(int id)
    {
        try
        {
            var direction = await _payments.GetDirectionAsync(id);
            if (!await HasRightAsync(PartnerPaymentRightResolver.Resolve(direction, PaymentAction.Approve)))
            {
                return Forbid();
            }

            await _payments.ApproveAsync(id, CurrentUsername);
            return Ok(ApiResponse<object>.Ok(null, ApprovedMessageFor(direction).ToText()));
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
    public async Task<ActionResult<ApiResponse<object>>> Reject(int id)
    {
        try
        {
            var direction = await _payments.GetDirectionAsync(id);
            if (!await HasRightAsync(PartnerPaymentRightResolver.Resolve(direction, PaymentAction.Reject)))
            {
                return Forbid();
            }

            await _payments.RejectAsync(id);
            return Ok(ApiResponse<object>.Ok(null, RejectedMessageFor(direction).ToText()));
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

    private async Task<bool> HasRightAsync(string rightCode)
    {
        var result = await _authorizationService.AuthorizeAsync(User, RightPolicies.Prefix + rightCode);
        return result.Succeeded;
    }

    private static ResponseMessage CreatedMessageFor(PaymentDirection direction) =>
        direction == PaymentDirection.CustomerReceipt ? ResponseMessage.CustomerReceiptCreated : ResponseMessage.SupplierPaymentCreated;

    private static ResponseMessage SubmittedMessageFor(PaymentDirection direction) =>
        direction == PaymentDirection.CustomerReceipt ? ResponseMessage.CustomerReceiptSubmitted : ResponseMessage.SupplierPaymentSubmitted;

    private static ResponseMessage ApprovedMessageFor(PaymentDirection direction) =>
        direction == PaymentDirection.CustomerReceipt ? ResponseMessage.CustomerReceiptApproved : ResponseMessage.SupplierPaymentApproved;

    private static ResponseMessage RejectedMessageFor(PaymentDirection direction) =>
        direction == PaymentDirection.CustomerReceipt ? ResponseMessage.CustomerReceiptRejected : ResponseMessage.SupplierPaymentRejected;
}
