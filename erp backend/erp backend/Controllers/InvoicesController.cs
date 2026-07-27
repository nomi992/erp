using erp_backend.Auth;
using erp_backend.Exceptions;
using erp_backend.Invoices;
using erp_backend.Invoices.Dtos;
using erp_backend.Messages;
using erp_backend.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace erp_backend.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class InvoicesController : ControllerBase
{
    private readonly IInvoiceRepository _invoices;
    private readonly IAuthorizationService _authorizationService;

    public InvoicesController(IInvoiceRepository invoices, IAuthorizationService authorizationService)
    {
        _invoices = invoices;
        _authorizationService = authorizationService;
    }

    private string CurrentUsername => User.Identity?.Name ?? "system";

    [HttpGet]
    public async Task<ActionResult<ApiResponse<PagedResult<InvoiceListItemResponse>>>> GetAll(
        [FromQuery] InvoiceType? invoiceType, [FromQuery] int? partnerId, [FromQuery] InvoiceStatus? status,
        [FromQuery] DateTime? from, [FromQuery] DateTime? to,
        [FromQuery] string? search, [FromQuery] string? sortBy, [FromQuery] string? sortDirection,
        [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 25)
    {
        try
        {
            var invoices = await _invoices.GetAllAsync(invoiceType, partnerId, status, from, to, search, sortBy, sortDirection, pageNumber, pageSize);
            return Ok(ApiResponse<PagedResult<InvoiceListItemResponse>>.Ok(invoices, ResponseMessage.Success.ToText()));
        }
        catch (AppException ex)
        {
            return StatusCode(ex.StatusCode, ApiResponse<PagedResult<InvoiceListItemResponse>>.Fail(ex.Message, ex.StatusCode));
        }
        catch (Exception)
        {
            return StatusCode(500, ApiResponse<PagedResult<InvoiceListItemResponse>>.Fail(ResponseMessage.UnknownError.ToText(), 500));
        }
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<ApiResponse<InvoiceResponse>>> GetById(int id)
    {
        try
        {
            var invoice = await _invoices.GetByIdAsync(id);
            return Ok(ApiResponse<InvoiceResponse>.Ok(invoice, ResponseMessage.Success.ToText()));
        }
        catch (AppException ex)
        {
            return StatusCode(ex.StatusCode, ApiResponse<InvoiceResponse>.Fail(ex.Message, ex.StatusCode));
        }
        catch (Exception)
        {
            return StatusCode(500, ApiResponse<InvoiceResponse>.Fail(ResponseMessage.UnknownError.ToText(), 500));
        }
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<InvoiceResponse>>> Create(InvoiceRequest request)
    {
        try
        {
            if (!await HasRightAsync(InvoiceRightResolver.Resolve(request.InvoiceType, InvoiceAction.Create)))
            {
                return Forbid();
            }

            var invoice = await _invoices.CreateAsync(request, CurrentUsername);
            return Ok(ApiResponse<InvoiceResponse>.Ok(invoice, CreatedMessageFor(request.InvoiceType).ToText()));
        }
        catch (AppException ex)
        {
            return StatusCode(ex.StatusCode, ApiResponse<InvoiceResponse>.Fail(ex.Message, ex.StatusCode));
        }
        catch (Exception)
        {
            return StatusCode(500, ApiResponse<InvoiceResponse>.Fail(ResponseMessage.UnknownError.ToText(), 500));
        }
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<ApiResponse<InvoiceResponse>>> Update(int id, InvoiceRequest request)
    {
        try
        {
            var persistedType = await _invoices.GetInvoiceTypeAsync(id);
            if (!await HasRightAsync(InvoiceRightResolver.Resolve(persistedType, InvoiceAction.Edit)))
            {
                return Forbid();
            }

            var invoice = await _invoices.UpdateAsync(id, request);
            return Ok(ApiResponse<InvoiceResponse>.Ok(invoice, UpdatedMessageFor(persistedType).ToText()));
        }
        catch (AppException ex)
        {
            return StatusCode(ex.StatusCode, ApiResponse<InvoiceResponse>.Fail(ex.Message, ex.StatusCode));
        }
        catch (Exception)
        {
            return StatusCode(500, ApiResponse<InvoiceResponse>.Fail(ResponseMessage.UnknownError.ToText(), 500));
        }
    }

    [HttpPost("{id:int}/submit")]
    public async Task<ActionResult<ApiResponse<object>>> Submit(int id)
    {
        try
        {
            var persistedType = await _invoices.GetInvoiceTypeAsync(id);
            if (!await HasRightAsync(InvoiceRightResolver.Resolve(persistedType, InvoiceAction.Submit)))
            {
                return Forbid();
            }

            await _invoices.SubmitAsync(id);
            return Ok(ApiResponse<object>.Ok(null, SubmittedMessageFor(persistedType).ToText()));
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
            var persistedType = await _invoices.GetInvoiceTypeAsync(id);
            if (!await HasRightAsync(InvoiceRightResolver.Resolve(persistedType, InvoiceAction.Approve)))
            {
                return Forbid();
            }

            await _invoices.ApproveAsync(id, CurrentUsername);
            return Ok(ApiResponse<object>.Ok(null, ApprovedMessageFor(persistedType).ToText()));
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
            var persistedType = await _invoices.GetInvoiceTypeAsync(id);
            if (!await HasRightAsync(InvoiceRightResolver.Resolve(persistedType, InvoiceAction.Reject)))
            {
                return Forbid();
            }

            await _invoices.RejectAsync(id);
            return Ok(ApiResponse<object>.Ok(null, RejectedMessageFor(persistedType).ToText()));
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

    [HttpPost("{id:int}/cancel")]
    public async Task<ActionResult<ApiResponse<object>>> Cancel(int id)
    {
        try
        {
            var persistedType = await _invoices.GetInvoiceTypeAsync(id);
            if (!await HasRightAsync(InvoiceRightResolver.Resolve(persistedType, InvoiceAction.Cancel)))
            {
                return Forbid();
            }

            await _invoices.CancelAsync(id);
            return Ok(ApiResponse<object>.Ok(null, CancelledMessageFor(persistedType).ToText()));
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

    private static ResponseMessage CreatedMessageFor(InvoiceType type) => type switch
    {
        InvoiceType.PurchaseOrder => ResponseMessage.PurchaseOrderCreated,
        InvoiceType.SalesOrder => ResponseMessage.SalesOrderCreated,
        InvoiceType.PurchaseInvoice => ResponseMessage.PurchaseInvoiceCreated,
        InvoiceType.SalesInvoice => ResponseMessage.SalesInvoiceCreated,
        InvoiceType.PurchaseReturn => ResponseMessage.PurchaseReturnCreated,
        InvoiceType.SaleReturn => ResponseMessage.SaleReturnCreated,
        _ => ResponseMessage.Success,
    };

    private static ResponseMessage UpdatedMessageFor(InvoiceType type) => type switch
    {
        InvoiceType.PurchaseOrder => ResponseMessage.PurchaseOrderUpdated,
        InvoiceType.SalesOrder => ResponseMessage.SalesOrderUpdated,
        InvoiceType.PurchaseInvoice => ResponseMessage.PurchaseInvoiceUpdated,
        InvoiceType.SalesInvoice => ResponseMessage.SalesInvoiceUpdated,
        InvoiceType.PurchaseReturn => ResponseMessage.PurchaseReturnUpdated,
        InvoiceType.SaleReturn => ResponseMessage.SaleReturnUpdated,
        _ => ResponseMessage.Success,
    };

    private static ResponseMessage SubmittedMessageFor(InvoiceType type) => type switch
    {
        InvoiceType.PurchaseOrder => ResponseMessage.PurchaseOrderSubmitted,
        InvoiceType.SalesOrder => ResponseMessage.SalesOrderSubmitted,
        InvoiceType.PurchaseInvoice => ResponseMessage.PurchaseInvoiceSubmitted,
        InvoiceType.SalesInvoice => ResponseMessage.SalesInvoiceSubmitted,
        InvoiceType.PurchaseReturn => ResponseMessage.PurchaseReturnSubmitted,
        InvoiceType.SaleReturn => ResponseMessage.SaleReturnSubmitted,
        _ => ResponseMessage.Success,
    };

    private static ResponseMessage ApprovedMessageFor(InvoiceType type) => type switch
    {
        InvoiceType.PurchaseOrder => ResponseMessage.PurchaseOrderApproved,
        InvoiceType.SalesOrder => ResponseMessage.SalesOrderApproved,
        InvoiceType.PurchaseInvoice => ResponseMessage.PurchaseInvoiceApproved,
        InvoiceType.SalesInvoice => ResponseMessage.SalesInvoiceApproved,
        InvoiceType.PurchaseReturn => ResponseMessage.PurchaseReturnApproved,
        InvoiceType.SaleReturn => ResponseMessage.SaleReturnApproved,
        _ => ResponseMessage.Success,
    };

    private static ResponseMessage RejectedMessageFor(InvoiceType type) => type switch
    {
        InvoiceType.PurchaseOrder => ResponseMessage.PurchaseOrderRejected,
        InvoiceType.SalesOrder => ResponseMessage.SalesOrderRejected,
        InvoiceType.PurchaseInvoice => ResponseMessage.PurchaseInvoiceRejected,
        InvoiceType.SalesInvoice => ResponseMessage.SalesInvoiceRejected,
        InvoiceType.PurchaseReturn => ResponseMessage.PurchaseReturnRejected,
        InvoiceType.SaleReturn => ResponseMessage.SaleReturnRejected,
        _ => ResponseMessage.Success,
    };

    private static ResponseMessage CancelledMessageFor(InvoiceType type) => type switch
    {
        InvoiceType.PurchaseOrder => ResponseMessage.PurchaseOrderCancelled,
        InvoiceType.SalesOrder => ResponseMessage.SalesOrderCancelled,
        _ => ResponseMessage.Success,
    };
}
