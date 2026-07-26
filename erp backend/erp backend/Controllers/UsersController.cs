using erp_backend.Auth;
using erp_backend.Exceptions;
using erp_backend.Messages;
using erp_backend.Models;
using erp_backend.Users;
using erp_backend.Users.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace erp_backend.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class UsersController : ControllerBase
{
    private readonly IUserRepository _users;

    public UsersController(IUserRepository users)
    {
        _users = users;
    }

    [HttpGet]
    [Authorize(Policy = RightPolicies.Prefix + RightCodes.UsersView)]
    public async Task<ActionResult<ApiResponse<List<UserResponse>>>> GetAll([FromQuery] int? tenantId)
    {
        try
        {
            var users = await _users.GetAllAsync(tenantId);
            return Ok(ApiResponse<List<UserResponse>>.Ok(users, ResponseMessage.Success.ToText()));
        }
        catch (AppException ex)
        {
            return StatusCode(ex.StatusCode, ApiResponse<List<UserResponse>>.Fail(ex.Message, ex.StatusCode));
        }
        catch (Exception)
        {
            return StatusCode(500, ApiResponse<List<UserResponse>>.Fail(ResponseMessage.UnknownError.ToText(), 500));
        }
    }

    [HttpGet("{id:int}")]
    [Authorize(Policy = RightPolicies.Prefix + RightCodes.UsersView)]
    public async Task<ActionResult<ApiResponse<UserResponse>>> GetById(int id)
    {
        try
        {
            var user = await _users.GetByIdAsync(id);
            return Ok(ApiResponse<UserResponse>.Ok(user, ResponseMessage.Success.ToText()));
        }
        catch (AppException ex)
        {
            return StatusCode(ex.StatusCode, ApiResponse<UserResponse>.Fail(ex.Message, ex.StatusCode));
        }
        catch (Exception)
        {
            return StatusCode(500, ApiResponse<UserResponse>.Fail(ResponseMessage.UnknownError.ToText(), 500));
        }
    }

    [HttpPost]
    [Authorize(Policy = RightPolicies.Prefix + RightCodes.UsersCreate)]
    public async Task<ActionResult<ApiResponse<UserResponse>>> Create(CreateUserRequest request)
    {
        try
        {
            var user = await _users.CreateAsync(request);
            return Ok(ApiResponse<UserResponse>.Ok(user, ResponseMessage.UserCreated.ToText()));
        }
        catch (AppException ex)
        {
            return StatusCode(ex.StatusCode, ApiResponse<UserResponse>.Fail(ex.Message, ex.StatusCode));
        }
        catch (Exception)
        {
            return StatusCode(500, ApiResponse<UserResponse>.Fail(ResponseMessage.UnknownError.ToText(), 500));
        }
    }

    [HttpPut("{id:int}")]
    [Authorize(Policy = RightPolicies.Prefix + RightCodes.UsersEdit)]
    public async Task<ActionResult<ApiResponse<UserResponse>>> Update(int id, UpdateUserRequest request)
    {
        try
        {
            var user = await _users.UpdateAsync(id, request);
            return Ok(ApiResponse<UserResponse>.Ok(user, ResponseMessage.UserUpdated.ToText()));
        }
        catch (AppException ex)
        {
            return StatusCode(ex.StatusCode, ApiResponse<UserResponse>.Fail(ex.Message, ex.StatusCode));
        }
        catch (Exception)
        {
            return StatusCode(500, ApiResponse<UserResponse>.Fail(ResponseMessage.UnknownError.ToText(), 500));
        }
    }

    [HttpPatch("{id:int}/activate")]
    [Authorize(Policy = RightPolicies.Prefix + RightCodes.UsersEdit)]
    public async Task<ActionResult<ApiResponse<object>>> Activate(int id)
    {
        try
        {
            await _users.ActivateAsync(id);
            return Ok(ApiResponse<object>.Ok(null, ResponseMessage.UserActivated.ToText()));
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
    [Authorize(Policy = RightPolicies.Prefix + RightCodes.UsersEdit)]
    public async Task<ActionResult<ApiResponse<object>>> Deactivate(int id)
    {
        try
        {
            await _users.DeactivateAsync(id);
            return Ok(ApiResponse<object>.Ok(null, ResponseMessage.UserDeactivated.ToText()));
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

    [HttpPost("{id:int}/branches")]
    [Authorize(Policy = RightPolicies.Prefix + RightCodes.UsersManageBranchAccess)]
    public async Task<ActionResult<ApiResponse<object>>> GrantBranch(int id, GrantBranchRequest request)
    {
        try
        {
            await _users.GrantBranchAsync(id, request.BranchId);
            return Ok(ApiResponse<object>.Ok(null, ResponseMessage.BranchAccessGranted.ToText()));
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

    [HttpDelete("{id:int}/branches/{branchId:int}")]
    [Authorize(Policy = RightPolicies.Prefix + RightCodes.UsersManageBranchAccess)]
    public async Task<ActionResult<ApiResponse<object>>> RevokeBranch(int id, int branchId)
    {
        try
        {
            await _users.RevokeBranchAsync(id, branchId);
            return Ok(ApiResponse<object>.Ok(null, ResponseMessage.BranchAccessRevoked.ToText()));
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
