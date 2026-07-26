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
public class BudgetsController : ControllerBase
{
    private readonly IBudgetRepository _budgets;

    public BudgetsController(IBudgetRepository budgets)
    {
        _budgets = budgets;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<List<BudgetResponse>>>> GetAll([FromQuery] int? year, [FromQuery] int? month)
    {
        try
        {
            var budgets = await _budgets.GetAllAsync(year, month);
            return Ok(ApiResponse<List<BudgetResponse>>.Ok(budgets, ResponseMessage.Success.ToText()));
        }
        catch (AppException ex)
        {
            return StatusCode(ex.StatusCode, ApiResponse<List<BudgetResponse>>.Fail(ex.Message, ex.StatusCode));
        }
        catch (Exception)
        {
            return StatusCode(500, ApiResponse<List<BudgetResponse>>.Fail(ResponseMessage.UnknownError.ToText(), 500));
        }
    }

    [HttpPost]
    [Authorize(Policy = RightPolicies.Prefix + RightCodes.BudgetsCreate)]
    public async Task<ActionResult<ApiResponse<BudgetResponse>>> Create(BudgetRequest request)
    {
        try
        {
            var budget = await _budgets.CreateAsync(request);
            return Ok(ApiResponse<BudgetResponse>.Ok(budget, ResponseMessage.BudgetCreated.ToText()));
        }
        catch (AppException ex)
        {
            return StatusCode(ex.StatusCode, ApiResponse<BudgetResponse>.Fail(ex.Message, ex.StatusCode));
        }
        catch (Exception)
        {
            return StatusCode(500, ApiResponse<BudgetResponse>.Fail(ResponseMessage.UnknownError.ToText(), 500));
        }
    }

    [HttpPut("{id:int}")]
    [Authorize(Policy = RightPolicies.Prefix + RightCodes.BudgetsEdit)]
    public async Task<ActionResult<ApiResponse<BudgetResponse>>> Update(int id, BudgetRequest request)
    {
        try
        {
            var budget = await _budgets.UpdateAsync(id, request);
            return Ok(ApiResponse<BudgetResponse>.Ok(budget, ResponseMessage.BudgetUpdated.ToText()));
        }
        catch (AppException ex)
        {
            return StatusCode(ex.StatusCode, ApiResponse<BudgetResponse>.Fail(ex.Message, ex.StatusCode));
        }
        catch (Exception)
        {
            return StatusCode(500, ApiResponse<BudgetResponse>.Fail(ResponseMessage.UnknownError.ToText(), 500));
        }
    }

    [HttpDelete("{id:int}")]
    [Authorize(Policy = RightPolicies.Prefix + RightCodes.BudgetsDelete)]
    public async Task<ActionResult<ApiResponse<object>>> Delete(int id)
    {
        try
        {
            await _budgets.DeleteAsync(id);
            return Ok(ApiResponse<object>.Ok(null, ResponseMessage.BudgetDeleted.ToText()));
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
