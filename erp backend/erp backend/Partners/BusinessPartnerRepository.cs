using erp_backend.Data;
using erp_backend.Exceptions;
using erp_backend.Messages;
using erp_backend.Models;
using erp_backend.Partners.Dtos;
using Microsoft.EntityFrameworkCore;

namespace erp_backend.Partners;

public class BusinessPartnerRepository : IBusinessPartnerRepository
{
    private readonly AppDbContext _context;

    public BusinessPartnerRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<BusinessPartnerResponse>> GetAllAsync(PartnerType? partnerType)
    {
        var query = _context.BusinessPartners.AsQueryable();

        if (partnerType is PartnerType.Supplier or PartnerType.Customer)
        {
            query = query.Where(p => p.PartnerType == partnerType || p.PartnerType == PartnerType.Both);
        }
        else if (partnerType is PartnerType.Both)
        {
            query = query.Where(p => p.PartnerType == PartnerType.Both);
        }

        var partners = await query.OrderBy(p => p.Name).ToListAsync();
        return partners.Select(BusinessPartnerResponse.FromEntity).ToList();
    }

    public async Task<BusinessPartnerResponse> GetByIdAsync(int id)
    {
        var partner = await _context.BusinessPartners.FindAsync(id) ?? throw new NotFoundException(ResponseMessage.BusinessPartnerNotFound);
        return BusinessPartnerResponse.FromEntity(partner);
    }

    public async Task<PartnerType> GetPartnerTypeAsync(int id)
    {
        var partner = await _context.BusinessPartners.FindAsync(id) ?? throw new NotFoundException(ResponseMessage.BusinessPartnerNotFound);
        return partner.PartnerType;
    }

    public async Task<BusinessPartnerResponse> CreateAsync(BusinessPartnerRequest request)
    {
        if (await _context.BusinessPartners.AnyAsync(p => p.Code == request.Code))
        {
            throw new BadRequestException(ResponseMessage.BusinessPartnerCodeExists);
        }

        var partner = new BusinessPartner
        {
            PartnerType = request.PartnerType,
            Code = request.Code,
            Name = request.Name,
            ContactPerson = request.ContactPerson,
            Phone = request.Phone,
            Email = request.Email,
            Address = request.Address,
            DefaultPaymentTermDays = request.DefaultPaymentTermDays,
            CreditLimit = request.CreditLimit,
        };

        _context.BusinessPartners.Add(partner);
        await _context.SaveChangesAsync();

        return BusinessPartnerResponse.FromEntity(partner);
    }

    public async Task<BusinessPartnerResponse> UpdateAsync(int id, BusinessPartnerRequest request)
    {
        var partner = await _context.BusinessPartners.FindAsync(id) ?? throw new NotFoundException(ResponseMessage.BusinessPartnerNotFound);

        if (await _context.BusinessPartners.AnyAsync(p => p.Code == request.Code && p.Id != id))
        {
            throw new BadRequestException(ResponseMessage.BusinessPartnerCodeExists);
        }

        partner.PartnerType = request.PartnerType;
        partner.Code = request.Code;
        partner.Name = request.Name;
        partner.ContactPerson = request.ContactPerson;
        partner.Phone = request.Phone;
        partner.Email = request.Email;
        partner.Address = request.Address;
        partner.DefaultPaymentTermDays = request.DefaultPaymentTermDays;
        partner.CreditLimit = request.CreditLimit;

        await _context.SaveChangesAsync();

        return BusinessPartnerResponse.FromEntity(partner);
    }

    public Task ActivateAsync(int id) => SetActiveAsync(id, true);

    public Task DeactivateAsync(int id) => SetActiveAsync(id, false);

    private async Task SetActiveAsync(int id, bool isActive)
    {
        var partner = await _context.BusinessPartners.FindAsync(id) ?? throw new NotFoundException(ResponseMessage.BusinessPartnerNotFound);
        partner.IsActive = isActive;
        await _context.SaveChangesAsync();
    }
}
