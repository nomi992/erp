using erp_backend.Models;
using erp_backend.Partners.Dtos;

namespace erp_backend.Partners;

public interface IBusinessPartnerRepository
{
    Task<List<BusinessPartnerResponse>> GetAllAsync(PartnerType? partnerType);
    Task<BusinessPartnerResponse> GetByIdAsync(int id);
    Task<PartnerType> GetPartnerTypeAsync(int id);
    Task<BusinessPartnerResponse> CreateAsync(BusinessPartnerRequest request);
    Task<BusinessPartnerResponse> UpdateAsync(int id, BusinessPartnerRequest request);
    Task ActivateAsync(int id);
    Task DeactivateAsync(int id);
}
