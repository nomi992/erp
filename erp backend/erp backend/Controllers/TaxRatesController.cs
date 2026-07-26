using erp_backend.Auth;
using erp_backend.Exceptions;
using erp_backend.Messages;
using erp_backend.Models;
using erp_backend.TaxRates;
using erp_backend.TaxRates.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace erp_backend.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class TaxRatesController : ControllerBase
{
    private readonly ITaxRateRepository _taxRates;

    public TaxRatesController(ITaxRateRepository taxRates)
    {
        _taxRates = taxRates;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<List<TaxRate>>>> GetAll()
    {
        try
        {
            var rates = await _taxRates.GetAllAsync();
            return Ok(ApiResponse<List<TaxRate>>.Ok(rates, ResponseMessage.Success.ToText()));
        }
        catch (AppException ex)
        {
            return StatusCode(ex.StatusCode, ApiResponse<List<TaxRate>>.Fail(ex.Message, ex.StatusCode));
        }
        catch (Exception)
        {
            return StatusCode(500, ApiResponse<List<TaxRate>>.Fail(ResponseMessage.UnknownError.ToText(), 500));
        }
    }

    [HttpPost]
    [Authorize(Policy = RightPolicies.Prefix + RightCodes.TaxRatesCreate)]
    public async Task<ActionResult<ApiResponse<TaxRate>>> Create(TaxRateRequest request)
    {
        try
        {
            var taxRate = await _taxRates.CreateAsync(request);
            return Ok(ApiResponse<TaxRate>.Ok(taxRate, ResponseMessage.TaxRateCreated.ToText()));
        }
        catch (AppException ex)
        {
            return StatusCode(ex.StatusCode, ApiResponse<TaxRate>.Fail(ex.Message, ex.StatusCode));
        }
        catch (Exception)
        {
            return StatusCode(500, ApiResponse<TaxRate>.Fail(ResponseMessage.UnknownError.ToText(), 500));
        }
    }

    [HttpPut("{id:int}")]
    [Authorize(Policy = RightPolicies.Prefix + RightCodes.TaxRatesEdit)]
    public async Task<ActionResult<ApiResponse<TaxRate>>> Update(int id, TaxRateRequest request)
    {
        try
        {
            var taxRate = await _taxRates.UpdateAsync(id, request);
            return Ok(ApiResponse<TaxRate>.Ok(taxRate, ResponseMessage.TaxRateUpdated.ToText()));
        }
        catch (AppException ex)
        {
            return StatusCode(ex.StatusCode, ApiResponse<TaxRate>.Fail(ex.Message, ex.StatusCode));
        }
        catch (Exception)
        {
            return StatusCode(500, ApiResponse<TaxRate>.Fail(ResponseMessage.UnknownError.ToText(), 500));
        }
    }

    [HttpPatch("{id:int}/deactivate")]
    [Authorize(Policy = RightPolicies.Prefix + RightCodes.TaxRatesEdit)]
    public async Task<ActionResult<ApiResponse<object>>> Deactivate(int id)
    {
        try
        {
            await _taxRates.DeactivateAsync(id);
            return Ok(ApiResponse<object>.Ok(null, ResponseMessage.TaxRateDeactivated.ToText()));
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

    [HttpPatch("{id:int}/activate")]
    [Authorize(Policy = RightPolicies.Prefix + RightCodes.TaxRatesEdit)]
    public async Task<ActionResult<ApiResponse<object>>> Activate(int id)
    {
        try
        {
            await _taxRates.ActivateAsync(id);
            return Ok(ApiResponse<object>.Ok(null, ResponseMessage.TaxRateActivated.ToText()));
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
