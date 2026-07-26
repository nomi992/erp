using erp_backend.Auth;
using erp_backend.Exceptions;
using erp_backend.Messages;
using erp_backend.Models;
using erp_backend.Partners;
using erp_backend.Partners.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace erp_backend.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class BusinessPartnersController : ControllerBase
{
    private readonly IBusinessPartnerRepository _partners;
    private readonly IAuthorizationService _authorizationService;

    public BusinessPartnersController(IBusinessPartnerRepository partners, IAuthorizationService authorizationService)
    {
        _partners = partners;
        _authorizationService = authorizationService;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<List<BusinessPartnerResponse>>>> GetAll([FromQuery] PartnerType? partnerType)
    {
        try
        {
            var partners = await _partners.GetAllAsync(partnerType);
            return Ok(ApiResponse<List<BusinessPartnerResponse>>.Ok(partners, ResponseMessage.Success.ToText()));
        }
        catch (AppException ex)
        {
            return StatusCode(ex.StatusCode, ApiResponse<List<BusinessPartnerResponse>>.Fail(ex.Message, ex.StatusCode));
        }
        catch (Exception)
        {
            return StatusCode(500, ApiResponse<List<BusinessPartnerResponse>>.Fail(ResponseMessage.UnknownError.ToText(), 500));
        }
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<ApiResponse<BusinessPartnerResponse>>> GetById(int id)
    {
        try
        {
            var partner = await _partners.GetByIdAsync(id);
            return Ok(ApiResponse<BusinessPartnerResponse>.Ok(partner, ResponseMessage.Success.ToText()));
        }
        catch (AppException ex)
        {
            return StatusCode(ex.StatusCode, ApiResponse<BusinessPartnerResponse>.Fail(ex.Message, ex.StatusCode));
        }
        catch (Exception)
        {
            return StatusCode(500, ApiResponse<BusinessPartnerResponse>.Fail(ResponseMessage.UnknownError.ToText(), 500));
        }
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<BusinessPartnerResponse>>> Create(BusinessPartnerRequest request)
    {
        try
        {
            if (!await HasAllRightsAsync(BusinessPartnerRightResolver.Resolve(request.PartnerType, PartnerAction.Create)))
            {
                return Forbid();
            }

            var partner = await _partners.CreateAsync(request);
            return Ok(ApiResponse<BusinessPartnerResponse>.Ok(partner, ResponseMessage.BusinessPartnerCreated.ToText()));
        }
        catch (AppException ex)
        {
            return StatusCode(ex.StatusCode, ApiResponse<BusinessPartnerResponse>.Fail(ex.Message, ex.StatusCode));
        }
        catch (Exception)
        {
            return StatusCode(500, ApiResponse<BusinessPartnerResponse>.Fail(ResponseMessage.UnknownError.ToText(), 500));
        }
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<ApiResponse<BusinessPartnerResponse>>> Update(int id, BusinessPartnerRequest request)
    {
        try
        {
            // Both the persisted type and the (immutable-in-practice) requested type must be authorized,
            // so a request can't smuggle a partner from Supplier-only into Both without the Customer right.
            var persistedType = await _partners.GetPartnerTypeAsync(id);
            var requiredCodes = BusinessPartnerRightResolver.Resolve(persistedType, PartnerAction.Edit)
                .Concat(BusinessPartnerRightResolver.Resolve(request.PartnerType, PartnerAction.Edit))
                .Distinct();

            if (!await HasAllRightsAsync(requiredCodes))
            {
                return Forbid();
            }

            var partner = await _partners.UpdateAsync(id, request);
            return Ok(ApiResponse<BusinessPartnerResponse>.Ok(partner, ResponseMessage.BusinessPartnerUpdated.ToText()));
        }
        catch (AppException ex)
        {
            return StatusCode(ex.StatusCode, ApiResponse<BusinessPartnerResponse>.Fail(ex.Message, ex.StatusCode));
        }
        catch (Exception)
        {
            return StatusCode(500, ApiResponse<BusinessPartnerResponse>.Fail(ResponseMessage.UnknownError.ToText(), 500));
        }
    }

    [HttpPatch("{id:int}/activate")]
    public async Task<ActionResult<ApiResponse<object>>> Activate(int id)
    {
        try
        {
            var persistedType = await _partners.GetPartnerTypeAsync(id);
            if (!await HasAllRightsAsync(BusinessPartnerRightResolver.Resolve(persistedType, PartnerAction.Edit)))
            {
                return Forbid();
            }

            await _partners.ActivateAsync(id);
            return Ok(ApiResponse<object>.Ok(null, ResponseMessage.BusinessPartnerActivated.ToText()));
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
            var persistedType = await _partners.GetPartnerTypeAsync(id);
            if (!await HasAllRightsAsync(BusinessPartnerRightResolver.Resolve(persistedType, PartnerAction.Edit)))
            {
                return Forbid();
            }

            await _partners.DeactivateAsync(id);
            return Ok(ApiResponse<object>.Ok(null, ResponseMessage.BusinessPartnerDeactivated.ToText()));
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

    private async Task<bool> HasAllRightsAsync(IEnumerable<string> rightCodes)
    {
        foreach (var code in rightCodes)
        {
            var result = await _authorizationService.AuthorizeAsync(User, RightPolicies.Prefix + code);
            if (!result.Succeeded)
            {
                return false;
            }
        }

        return true;
    }
}
