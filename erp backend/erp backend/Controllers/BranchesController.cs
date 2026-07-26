using erp_backend.Auth;
using erp_backend.Branches;
using erp_backend.Branches.Dtos;
using erp_backend.Exceptions;
using erp_backend.Messages;
using erp_backend.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace erp_backend.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class BranchesController : ControllerBase
{
    private readonly IBranchRepository _branches;

    public BranchesController(IBranchRepository branches)
    {
        _branches = branches;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<List<BranchResponse>>>> GetAll([FromQuery] int? tenantId)
    {
        try
        {
            var branches = await _branches.GetAllAsync(tenantId);
            return Ok(ApiResponse<List<BranchResponse>>.Ok(branches, ResponseMessage.Success.ToText()));
        }
        catch (AppException ex)
        {
            return StatusCode(ex.StatusCode, ApiResponse<List<BranchResponse>>.Fail(ex.Message, ex.StatusCode));
        }
        catch (Exception)
        {
            return StatusCode(500, ApiResponse<List<BranchResponse>>.Fail(ResponseMessage.UnknownError.ToText(), 500));
        }
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<ApiResponse<BranchResponse>>> GetById(int id)
    {
        try
        {
            var branch = await _branches.GetByIdAsync(id);
            return Ok(ApiResponse<BranchResponse>.Ok(branch, ResponseMessage.Success.ToText()));
        }
        catch (AppException ex)
        {
            return StatusCode(ex.StatusCode, ApiResponse<BranchResponse>.Fail(ex.Message, ex.StatusCode));
        }
        catch (Exception)
        {
            return StatusCode(500, ApiResponse<BranchResponse>.Fail(ResponseMessage.UnknownError.ToText(), 500));
        }
    }

    [HttpPost]
    [Authorize(Policy = RightPolicies.Prefix + RightCodes.BranchesCreate)]
    public async Task<ActionResult<ApiResponse<BranchResponse>>> Create(CreateBranchRequest request)
    {
        try
        {
            var branch = await _branches.CreateAsync(request);
            return Ok(ApiResponse<BranchResponse>.Ok(branch, ResponseMessage.BranchCreated.ToText()));
        }
        catch (AppException ex)
        {
            return StatusCode(ex.StatusCode, ApiResponse<BranchResponse>.Fail(ex.Message, ex.StatusCode));
        }
        catch (Exception)
        {
            return StatusCode(500, ApiResponse<BranchResponse>.Fail(ResponseMessage.UnknownError.ToText(), 500));
        }
    }

    [HttpPut("{id:int}")]
    [Authorize(Policy = RightPolicies.Prefix + RightCodes.BranchesEdit)]
    public async Task<ActionResult<ApiResponse<BranchResponse>>> Update(int id, UpdateBranchRequest request)
    {
        try
        {
            var branch = await _branches.UpdateAsync(id, request);
            return Ok(ApiResponse<BranchResponse>.Ok(branch, ResponseMessage.BranchUpdated.ToText()));
        }
        catch (AppException ex)
        {
            return StatusCode(ex.StatusCode, ApiResponse<BranchResponse>.Fail(ex.Message, ex.StatusCode));
        }
        catch (Exception)
        {
            return StatusCode(500, ApiResponse<BranchResponse>.Fail(ResponseMessage.UnknownError.ToText(), 500));
        }
    }

    [HttpPatch("{id:int}/activate")]
    [Authorize(Policy = RightPolicies.Prefix + RightCodes.BranchesEdit)]
    public async Task<ActionResult<ApiResponse<object>>> Activate(int id)
    {
        try
        {
            await _branches.ActivateAsync(id);
            return Ok(ApiResponse<object>.Ok(null, ResponseMessage.BranchActivated.ToText()));
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
    [Authorize(Policy = RightPolicies.Prefix + RightCodes.BranchesEdit)]
    public async Task<ActionResult<ApiResponse<object>>> Deactivate(int id)
    {
        try
        {
            await _branches.DeactivateAsync(id);
            return Ok(ApiResponse<object>.Ok(null, ResponseMessage.BranchDeactivated.ToText()));
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
