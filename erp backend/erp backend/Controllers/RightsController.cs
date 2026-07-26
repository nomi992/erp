using erp_backend.Auth;
using erp_backend.Exceptions;
using erp_backend.Messages;
using erp_backend.Models;
using erp_backend.Rights;
using erp_backend.Rights.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace erp_backend.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Policy = RightPolicies.RolesManage)]
public class RightsController : ControllerBase
{
    private readonly IRightRepository _rights;

    public RightsController(IRightRepository rights)
    {
        _rights = rights;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<List<RightModuleGroupResponse>>>> GetAll()
    {
        try
        {
            var rights = await _rights.GetAllGroupedAsync();
            return Ok(ApiResponse<List<RightModuleGroupResponse>>.Ok(rights, ResponseMessage.Success.ToText()));
        }
        catch (AppException ex)
        {
            return StatusCode(ex.StatusCode, ApiResponse<List<RightModuleGroupResponse>>.Fail(ex.Message, ex.StatusCode));
        }
        catch (Exception)
        {
            return StatusCode(500, ApiResponse<List<RightModuleGroupResponse>>.Fail(ResponseMessage.UnknownError.ToText(), 500));
        }
    }
}
