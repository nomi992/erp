using erp_backend.Exceptions;
using erp_backend.Ledgers;
using erp_backend.Ledgers.Dtos;
using erp_backend.Messages;
using erp_backend.Models;
using erp_backend.Reporting;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace erp_backend.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ReportsController : ControllerBase
{
    private readonly IReportRepository _reports;
    private readonly ISubLedgerRepository _subLedgers;

    public ReportsController(IReportRepository reports, ISubLedgerRepository subLedgers)
    {
        _reports = reports;
        _subLedgers = subLedgers;
    }

    [HttpGet("trial-balance")]
    public async Task<ActionResult<ApiResponse<object>>> TrialBalance([FromQuery] DateTime? asOf, [FromQuery] int? costCenterId)
    {
        try
        {
            var result = await _reports.GetTrialBalanceAsync(asOf ?? DateTime.UtcNow, costCenterId);
            return Ok(ApiResponse<object>.Ok(result, ResponseMessage.Success.ToText()));
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

    [HttpGet("profit-loss")]
    public async Task<ActionResult<ApiResponse<object>>> ProfitLoss(
        [FromQuery] DateTime from, [FromQuery] DateTime to, [FromQuery] bool compare = false, [FromQuery] int? costCenterId = null)
    {
        try
        {
            var result = await _reports.GetProfitLossAsync(from, to, compare, costCenterId);
            return Ok(ApiResponse<object>.Ok(result, ResponseMessage.Success.ToText()));
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

    [HttpGet("balance-sheet")]
    public async Task<ActionResult<ApiResponse<object>>> BalanceSheet([FromQuery] DateTime? asOf, [FromQuery] int? costCenterId)
    {
        try
        {
            var result = await _reports.GetBalanceSheetAsync(asOf ?? DateTime.UtcNow, costCenterId);
            return Ok(ApiResponse<object>.Ok(result, ResponseMessage.Success.ToText()));
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

    [HttpGet("cash-flow")]
    public async Task<ActionResult<ApiResponse<object>>> CashFlow([FromQuery] DateTime from, [FromQuery] DateTime to)
    {
        try
        {
            var result = await _reports.GetCashFlowAsync(from, to);
            return Ok(ApiResponse<object>.Ok(result, ResponseMessage.Success.ToText()));
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

    [HttpGet("aging")]
    public async Task<ActionResult<ApiResponse<List<SubLedgerEntryResponse>>>> Aging(
        [FromQuery] string type, [FromQuery] DateTime? asOf, [FromQuery] int? controlAccountId)
    {
        try
        {
            var results = await _subLedgers.GetAgingAsync(type, asOf ?? DateTime.UtcNow, controlAccountId);
            return Ok(ApiResponse<List<SubLedgerEntryResponse>>.Ok(results, ResponseMessage.Success.ToText()));
        }
        catch (AppException ex)
        {
            return StatusCode(ex.StatusCode, ApiResponse<List<SubLedgerEntryResponse>>.Fail(ex.Message, ex.StatusCode));
        }
        catch (Exception)
        {
            return StatusCode(500, ApiResponse<List<SubLedgerEntryResponse>>.Fail(ResponseMessage.UnknownError.ToText(), 500));
        }
    }

    [HttpGet("day-book")]
    public async Task<ActionResult<ApiResponse<object>>> DayBook([FromQuery] DateTime from, [FromQuery] DateTime to)
    {
        try
        {
            var result = await _reports.GetDayBookAsync(from, to);
            return Ok(ApiResponse<object>.Ok(result, ResponseMessage.Success.ToText()));
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

    [HttpGet("budget-vs-actual")]
    public async Task<ActionResult<ApiResponse<object>>> BudgetVsActual([FromQuery] int year, [FromQuery] int month)
    {
        try
        {
            var result = await _reports.GetBudgetVsActualAsync(year, month);
            return Ok(ApiResponse<object>.Ok(result, ResponseMessage.Success.ToText()));
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

    [HttpGet("{type}/export")]
    public async Task<IActionResult> Export(
        string type,
        [FromQuery] string format,
        [FromQuery] DateTime? asOf,
        [FromQuery] DateTime? from,
        [FromQuery] DateTime? to,
        [FromQuery] bool compare = false,
        [FromQuery] int? costCenterId = null,
        [FromQuery] string? subLedgerType = null,
        [FromQuery] int? controlAccountId = null,
        [FromQuery] int? year = null,
        [FromQuery] int? month = null)
    {
        try
        {
            var (bytes, contentType, fileName) = await _reports.ExportAsync(
                type, format, asOf, from, to, compare, costCenterId, subLedgerType, controlAccountId, year, month);
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
