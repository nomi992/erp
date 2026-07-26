using erp_backend.Auth;
using erp_backend.Auth.Dtos;
using erp_backend.Exceptions;
using erp_backend.Messages;
using erp_backend.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace erp_backend.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthRepository _auth;

    public AuthController(IAuthRepository auth)
    {
        _auth = auth;
    }

    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<ActionResult<ApiResponse<LoginResponse>>> Login(LoginRequest request)
    {
        try
        {
            var response = await _auth.LoginAsync(request);
            return Ok(ApiResponse<LoginResponse>.Ok(response, ResponseMessage.LoginSuccessful.ToText()));
        }
        catch (AppException ex)
        {
            return StatusCode(ex.StatusCode, ApiResponse<LoginResponse>.Fail(ex.Message, ex.StatusCode));
        }
        catch (Exception)
        {
            return StatusCode(500, ApiResponse<LoginResponse>.Fail(ResponseMessage.UnknownError.ToText(), 500));
        }
    }
}
