using erp_backend.Auth;
using erp_backend.Exceptions;
using erp_backend.Messages;
using erp_backend.Models;
using erp_backend.Tenants;
using erp_backend.Tenants.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace erp_backend.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = AppRoles.SystemAdmin)]
public class TenantsController : ControllerBase
{
    private readonly ITenantRepository _tenants;

    public TenantsController(ITenantRepository tenants)
    {
        _tenants = tenants;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<List<TenantResponse>>>> GetAll()
    {
        try
        {
            var tenants = await _tenants.GetAllAsync();
            return Ok(ApiResponse<List<TenantResponse>>.Ok(tenants, ResponseMessage.Success.ToText()));
        }
        catch (AppException ex)
        {
            return StatusCode(ex.StatusCode, ApiResponse<List<TenantResponse>>.Fail(ex.Message, ex.StatusCode));
        }
        catch (Exception)
        {
            return StatusCode(500, ApiResponse<List<TenantResponse>>.Fail(ResponseMessage.UnknownError.ToText(), 500));
        }
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<ApiResponse<TenantResponse>>> GetById(int id)
    {
        try
        {
            var tenant = await _tenants.GetByIdAsync(id);
            return Ok(ApiResponse<TenantResponse>.Ok(tenant, ResponseMessage.Success.ToText()));
        }
        catch (AppException ex)
        {
            return StatusCode(ex.StatusCode, ApiResponse<TenantResponse>.Fail(ex.Message, ex.StatusCode));
        }
        catch (Exception)
        {
            return StatusCode(500, ApiResponse<TenantResponse>.Fail(ResponseMessage.UnknownError.ToText(), 500));
        }
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<TenantResponse>>> Create(CreateTenantRequest request)
    {
        try
        {
            var tenant = await _tenants.CreateAsync(request);
            return Ok(ApiResponse<TenantResponse>.Ok(tenant, ResponseMessage.TenantCreated.ToText()));
        }
        catch (AppException ex)
        {
            return StatusCode(ex.StatusCode, ApiResponse<TenantResponse>.Fail(ex.Message, ex.StatusCode));
        }
        catch (Exception)
        {
            return StatusCode(500, ApiResponse<TenantResponse>.Fail(ResponseMessage.UnknownError.ToText(), 500));
        }
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<ApiResponse<TenantResponse>>> Update(int id, UpdateTenantRequest request)
    {
        try
        {
            var tenant = await _tenants.UpdateAsync(id, request);
            return Ok(ApiResponse<TenantResponse>.Ok(tenant, ResponseMessage.TenantUpdated.ToText()));
        }
        catch (AppException ex)
        {
            return StatusCode(ex.StatusCode, ApiResponse<TenantResponse>.Fail(ex.Message, ex.StatusCode));
        }
        catch (Exception)
        {
            return StatusCode(500, ApiResponse<TenantResponse>.Fail(ResponseMessage.UnknownError.ToText(), 500));
        }
    }

    [HttpPatch("{id:int}/activate")]
    public async Task<ActionResult<ApiResponse<object>>> Activate(int id)
    {
        try
        {
            await _tenants.ActivateAsync(id);
            return Ok(ApiResponse<object>.Ok(null, ResponseMessage.TenantActivated.ToText()));
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
    public async Task<ActionResult<ApiResponse<object>>> Deactivate(int id)
    {
        try
        {
            await _tenants.DeactivateAsync(id);
            return Ok(ApiResponse<object>.Ok(null, ResponseMessage.TenantDeactivated.ToText()));
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
