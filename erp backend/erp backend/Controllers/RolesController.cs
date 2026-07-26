using erp_backend.Auth;
using erp_backend.Exceptions;
using erp_backend.Messages;
using erp_backend.Models;
using erp_backend.Roles;
using erp_backend.Roles.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace erp_backend.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Policy = RightPolicies.RolesManage)]
public class RolesController : ControllerBase
{
    private readonly IRoleRepository _roles;

    public RolesController(IRoleRepository roles)
    {
        _roles = roles;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<List<RoleResponse>>>> GetAll([FromQuery] int? tenantId)
    {
        try
        {
            var roles = await _roles.GetAllAsync(tenantId);
            return Ok(ApiResponse<List<RoleResponse>>.Ok(roles, ResponseMessage.Success.ToText()));
        }
        catch (AppException ex)
        {
            return StatusCode(ex.StatusCode, ApiResponse<List<RoleResponse>>.Fail(ex.Message, ex.StatusCode));
        }
        catch (Exception)
        {
            return StatusCode(500, ApiResponse<List<RoleResponse>>.Fail(ResponseMessage.UnknownError.ToText(), 500));
        }
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<ApiResponse<RoleResponse>>> GetById(int id)
    {
        try
        {
            var role = await _roles.GetByIdAsync(id);
            return Ok(ApiResponse<RoleResponse>.Ok(role, ResponseMessage.Success.ToText()));
        }
        catch (AppException ex)
        {
            return StatusCode(ex.StatusCode, ApiResponse<RoleResponse>.Fail(ex.Message, ex.StatusCode));
        }
        catch (Exception)
        {
            return StatusCode(500, ApiResponse<RoleResponse>.Fail(ResponseMessage.UnknownError.ToText(), 500));
        }
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<RoleResponse>>> Create(RoleRequest request)
    {
        try
        {
            var role = await _roles.CreateAsync(request);
            return Ok(ApiResponse<RoleResponse>.Ok(role, ResponseMessage.RoleCreated.ToText()));
        }
        catch (AppException ex)
        {
            return StatusCode(ex.StatusCode, ApiResponse<RoleResponse>.Fail(ex.Message, ex.StatusCode));
        }
        catch (Exception)
        {
            return StatusCode(500, ApiResponse<RoleResponse>.Fail(ResponseMessage.UnknownError.ToText(), 500));
        }
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<ApiResponse<RoleResponse>>> Update(int id, RoleRequest request)
    {
        try
        {
            var role = await _roles.UpdateAsync(id, request);
            return Ok(ApiResponse<RoleResponse>.Ok(role, ResponseMessage.RoleUpdated.ToText()));
        }
        catch (AppException ex)
        {
            return StatusCode(ex.StatusCode, ApiResponse<RoleResponse>.Fail(ex.Message, ex.StatusCode));
        }
        catch (Exception)
        {
            return StatusCode(500, ApiResponse<RoleResponse>.Fail(ResponseMessage.UnknownError.ToText(), 500));
        }
    }

    [HttpDelete("{id:int}")]
    public async Task<ActionResult<ApiResponse<object>>> Delete(int id)
    {
        try
        {
            await _roles.DeleteAsync(id);
            return Ok(ApiResponse<object>.Ok(null, ResponseMessage.RoleDeleted.ToText()));
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
