using erp_backend.Exceptions;
using erp_backend.Inventory;
using erp_backend.Inventory.Dtos;
using erp_backend.Messages;
using erp_backend.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace erp_backend.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class StockLedgerController : ControllerBase
{
    private readonly IStockLedgerRepository _stockLedger;

    public StockLedgerController(IStockLedgerRepository stockLedger)
    {
        _stockLedger = stockLedger;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<PagedResult<StockLedgerEntryResponse>>>> GetLedger(
        [FromQuery] int? productVariantId, [FromQuery] int? warehouseId, [FromQuery] DateTime? from, [FromQuery] DateTime? to,
        [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 25)
    {
        try
        {
            var ledger = await _stockLedger.GetLedgerAsync(productVariantId, warehouseId, from, to, pageNumber, pageSize);
            return Ok(ApiResponse<PagedResult<StockLedgerEntryResponse>>.Ok(ledger, ResponseMessage.Success.ToText()));
        }
        catch (AppException ex)
        {
            return StatusCode(ex.StatusCode, ApiResponse<PagedResult<StockLedgerEntryResponse>>.Fail(ex.Message, ex.StatusCode));
        }
        catch (Exception)
        {
            return StatusCode(500, ApiResponse<PagedResult<StockLedgerEntryResponse>>.Fail(ResponseMessage.UnknownError.ToText(), 500));
        }
    }

    [HttpGet("on-hand")]
    public async Task<ActionResult<ApiResponse<List<StockOnHandResponse>>>> GetOnHand(
        [FromQuery] int? warehouseId, [FromQuery] int? productVariantId, [FromQuery] bool lowStockOnly = false)
    {
        try
        {
            var onHand = await _stockLedger.GetOnHandAsync(warehouseId, productVariantId, lowStockOnly);
            return Ok(ApiResponse<List<StockOnHandResponse>>.Ok(onHand, ResponseMessage.Success.ToText()));
        }
        catch (AppException ex)
        {
            return StatusCode(ex.StatusCode, ApiResponse<List<StockOnHandResponse>>.Fail(ex.Message, ex.StatusCode));
        }
        catch (Exception)
        {
            return StatusCode(500, ApiResponse<List<StockOnHandResponse>>.Fail(ResponseMessage.UnknownError.ToText(), 500));
        }
    }

    [HttpGet("ar-aging")]
    public async Task<ActionResult<ApiResponse<List<PartnerAgingResponse>>>> GetAccountsReceivableAging()
    {
        try
        {
            var aging = await _stockLedger.GetAccountsReceivableAgingAsync();
            return Ok(ApiResponse<List<PartnerAgingResponse>>.Ok(aging, ResponseMessage.Success.ToText()));
        }
        catch (AppException ex)
        {
            return StatusCode(ex.StatusCode, ApiResponse<List<PartnerAgingResponse>>.Fail(ex.Message, ex.StatusCode));
        }
        catch (Exception)
        {
            return StatusCode(500, ApiResponse<List<PartnerAgingResponse>>.Fail(ResponseMessage.UnknownError.ToText(), 500));
        }
    }

    [HttpGet("ap-aging")]
    public async Task<ActionResult<ApiResponse<List<PartnerAgingResponse>>>> GetAccountsPayableAging()
    {
        try
        {
            var aging = await _stockLedger.GetAccountsPayableAgingAsync();
            return Ok(ApiResponse<List<PartnerAgingResponse>>.Ok(aging, ResponseMessage.Success.ToText()));
        }
        catch (AppException ex)
        {
            return StatusCode(ex.StatusCode, ApiResponse<List<PartnerAgingResponse>>.Fail(ex.Message, ex.StatusCode));
        }
        catch (Exception)
        {
            return StatusCode(500, ApiResponse<List<PartnerAgingResponse>>.Fail(ResponseMessage.UnknownError.ToText(), 500));
        }
    }
}
