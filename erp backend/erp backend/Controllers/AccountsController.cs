using erp_backend.Accounts;
using erp_backend.Accounts.Dtos;
using erp_backend.Auth;
using erp_backend.Exceptions;
using erp_backend.Messages;
using erp_backend.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace erp_backend.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class AccountsController : ControllerBase
{
    private readonly IAccountRepository _accounts;

    public AccountsController(IAccountRepository accounts)
    {
        _accounts = accounts;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<List<AccountResponse>>>> GetAll([FromQuery] bool? isActive)
    {
        try
        {
            var accounts = await _accounts.GetAllAsync(isActive);
            return Ok(ApiResponse<List<AccountResponse>>.Ok(accounts, ResponseMessage.Success.ToText()));
        }
        catch (AppException ex)
        {
            return StatusCode(ex.StatusCode, ApiResponse<List<AccountResponse>>.Fail(ex.Message, ex.StatusCode));
        }
        catch (Exception)
        {
            return StatusCode(500, ApiResponse<List<AccountResponse>>.Fail(ResponseMessage.UnknownError.ToText(), 500));
        }
    }

    [HttpGet("tree")]
    public async Task<ActionResult<ApiResponse<List<AccountTreeNodeResponse>>>> GetTree()
    {
        try
        {
            var tree = await _accounts.GetTreeAsync();
            return Ok(ApiResponse<List<AccountTreeNodeResponse>>.Ok(tree, ResponseMessage.Success.ToText()));
        }
        catch (AppException ex)
        {
            return StatusCode(ex.StatusCode, ApiResponse<List<AccountTreeNodeResponse>>.Fail(ex.Message, ex.StatusCode));
        }
        catch (Exception)
        {
            return StatusCode(500, ApiResponse<List<AccountTreeNodeResponse>>.Fail(ResponseMessage.UnknownError.ToText(), 500));
        }
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<ApiResponse<AccountResponse>>> GetById(int id)
    {
        try
        {
            var account = await _accounts.GetByIdAsync(id);
            return Ok(ApiResponse<AccountResponse>.Ok(account, ResponseMessage.Success.ToText()));
        }
        catch (AppException ex)
        {
            return StatusCode(ex.StatusCode, ApiResponse<AccountResponse>.Fail(ex.Message, ex.StatusCode));
        }
        catch (Exception)
        {
            return StatusCode(500, ApiResponse<AccountResponse>.Fail(ResponseMessage.UnknownError.ToText(), 500));
        }
    }

    [HttpPost]
    [Authorize(Policy = RightPolicies.Prefix + RightCodes.AccountsCreate)]
    public async Task<ActionResult<ApiResponse<AccountResponse>>> Create(CreateAccountRequest request)
    {
        try
        {
            var account = await _accounts.CreateAsync(request);
            return Ok(ApiResponse<AccountResponse>.Ok(account, ResponseMessage.AccountCreated.ToText()));
        }
        catch (AppException ex)
        {
            return StatusCode(ex.StatusCode, ApiResponse<AccountResponse>.Fail(ex.Message, ex.StatusCode));
        }
        catch (Exception)
        {
            return StatusCode(500, ApiResponse<AccountResponse>.Fail(ResponseMessage.UnknownError.ToText(), 500));
        }
    }

    [HttpPut("{id:int}")]
    [Authorize(Policy = RightPolicies.Prefix + RightCodes.AccountsEdit)]
    public async Task<ActionResult<ApiResponse<AccountResponse>>> Update(int id, UpdateAccountRequest request)
    {
        try
        {
            var account = await _accounts.UpdateAsync(id, request);
            return Ok(ApiResponse<AccountResponse>.Ok(account, ResponseMessage.AccountUpdated.ToText()));
        }
        catch (AppException ex)
        {
            return StatusCode(ex.StatusCode, ApiResponse<AccountResponse>.Fail(ex.Message, ex.StatusCode));
        }
        catch (Exception)
        {
            return StatusCode(500, ApiResponse<AccountResponse>.Fail(ResponseMessage.UnknownError.ToText(), 500));
        }
    }

    [HttpPatch("{id:int}/activate")]
    [Authorize(Policy = RightPolicies.Prefix + RightCodes.AccountsEdit)]
    public async Task<ActionResult<ApiResponse<object>>> Activate(int id)
    {
        try
        {
            await _accounts.ActivateAsync(id);
            return Ok(ApiResponse<object>.Ok(null, ResponseMessage.AccountActivated.ToText()));
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

    [HttpPatch("{id:int}/deactivate")]
    [Authorize(Policy = RightPolicies.Prefix + RightCodes.AccountsEdit)]
    public async Task<ActionResult<ApiResponse<object>>> Deactivate(int id)
    {
        try
        {
            await _accounts.DeactivateAsync(id);
            return Ok(ApiResponse<object>.Ok(null, ResponseMessage.AccountDeactivated.ToText()));
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

    [HttpDelete("{id:int}")]
    [Authorize(Policy = RightPolicies.Prefix + RightCodes.AccountsDelete)]
    public async Task<ActionResult<ApiResponse<object>>> Delete(int id)
    {
        try
        {
            await _accounts.DeleteAsync(id);
            return Ok(ApiResponse<object>.Ok(null, ResponseMessage.AccountDeleted.ToText()));
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
