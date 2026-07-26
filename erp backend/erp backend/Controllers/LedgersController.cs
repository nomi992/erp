using erp_backend.Exceptions;
using erp_backend.Ledgers;
using erp_backend.Ledgers.Dtos;
using erp_backend.Messages;
using erp_backend.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace erp_backend.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class LedgersController : ControllerBase
{
    private readonly ILedgerRepository _ledgers;
    private readonly ISubLedgerRepository _subLedgers;
    private readonly IBankReconciliationRepository _bankReconciliation;

    public LedgersController(
        ILedgerRepository ledgers, ISubLedgerRepository subLedgers, IBankReconciliationRepository bankReconciliation)
    {
        _ledgers = ledgers;
        _subLedgers = subLedgers;
        _bankReconciliation = bankReconciliation;
    }

    [HttpGet("general")]
    public async Task<ActionResult<ApiResponse<List<GeneralLedgerEntryResponse>>>> GetGeneralLedger(
        [FromQuery] DateTime? from, [FromQuery] DateTime? to, [FromQuery] VoucherType? voucherType)
    {
        try
        {
            var entries = await _ledgers.GetGeneralLedgerAsync(from, to, voucherType);
            return Ok(ApiResponse<List<GeneralLedgerEntryResponse>>.Ok(entries, ResponseMessage.Success.ToText()));
        }
        catch (AppException ex)
        {
            return StatusCode(ex.StatusCode, ApiResponse<List<GeneralLedgerEntryResponse>>.Fail(ex.Message, ex.StatusCode));
        }
        catch (Exception)
        {
            return StatusCode(500, ApiResponse<List<GeneralLedgerEntryResponse>>.Fail(ResponseMessage.UnknownError.ToText(), 500));
        }
    }

    [HttpGet("account/{accountId:int}")]
    public async Task<ActionResult<ApiResponse<AccountLedgerResponse>>> GetAccountLedger(
        int accountId, [FromQuery] DateTime? from, [FromQuery] DateTime? to)
    {
        try
        {
            var ledger = await _ledgers.GetAccountLedgerAsync(accountId, from, to);
            return Ok(ApiResponse<AccountLedgerResponse>.Ok(ledger, ResponseMessage.Success.ToText()));
        }
        catch (AppException ex)
        {
            return StatusCode(ex.StatusCode, ApiResponse<AccountLedgerResponse>.Fail(ex.Message, ex.StatusCode));
        }
        catch (Exception)
        {
            return StatusCode(500, ApiResponse<AccountLedgerResponse>.Fail(ResponseMessage.UnknownError.ToText(), 500));
        }
    }

    [HttpGet("subledger")]
    public async Task<ActionResult<ApiResponse<List<SubLedgerEntryResponse>>>> GetSubLedger(
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

    // --- Bank reconciliation (FR-LED-06 / FR-LED-07) ---

    [HttpGet("bank/{accountId:int}/lines")]
    public async Task<ActionResult<ApiResponse<List<BankStatementLineResponse>>>> GetBankLines(int accountId, [FromQuery] bool? matched)
    {
        try
        {
            var lines = await _bankReconciliation.GetLinesAsync(accountId, matched);
            return Ok(ApiResponse<List<BankStatementLineResponse>>.Ok(lines, ResponseMessage.Success.ToText()));
        }
        catch (AppException ex)
        {
            return StatusCode(ex.StatusCode, ApiResponse<List<BankStatementLineResponse>>.Fail(ex.Message, ex.StatusCode));
        }
        catch (Exception)
        {
            return StatusCode(500, ApiResponse<List<BankStatementLineResponse>>.Fail(ResponseMessage.UnknownError.ToText(), 500));
        }
    }

    [HttpPost("bank/{accountId:int}/import")]
    public async Task<ActionResult<ApiResponse<object>>> ImportStatement(int accountId, IFormFile file)
    {
        try
        {
            var imported = await _bankReconciliation.ImportStatementAsync(accountId, file);
            return Ok(ApiResponse<object>.Ok(new { imported }, ResponseMessage.BankStatementImported.ToText(imported)));
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

    [HttpPost("bank/{accountId:int}/auto-match")]
    public async Task<ActionResult<ApiResponse<object>>> AutoMatch(int accountId)
    {
        try
        {
            var (matchedCount, remaining) = await _bankReconciliation.AutoMatchAsync(accountId);
            var message = ResponseMessage.BankAutoMatchResult.ToText(matchedCount, matchedCount + remaining);
            return Ok(ApiResponse<object>.Ok(new { matchedCount, remaining }, message));
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

    [HttpPost("bank/match")]
    public async Task<ActionResult<ApiResponse<object>>> ManualMatch(ManualMatchRequest request)
    {
        try
        {
            await _bankReconciliation.ManualMatchAsync(request);
            return Ok(ApiResponse<object>.Ok(null, ResponseMessage.BankLineMatched.ToText()));
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

    [HttpDelete("bank/match/{bankStatementLineId:int}")]
    public async Task<ActionResult<ApiResponse<object>>> Unmatch(int bankStatementLineId)
    {
        try
        {
            await _bankReconciliation.UnmatchAsync(bankStatementLineId);
            return Ok(ApiResponse<object>.Ok(null, ResponseMessage.BankLineUnmatched.ToText()));
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
