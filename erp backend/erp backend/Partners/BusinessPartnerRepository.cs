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
    private readonly IPartnerLedgerAccountResolver _ledgerAccounts;

    public BusinessPartnerRepository(AppDbContext context, IPartnerLedgerAccountResolver ledgerAccounts)
    {
        _context = context;
        _ledgerAccounts = ledgerAccounts;
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
            IsDefault = request.IsDefault,
        };

        if (request.IsDefault)
        {
            await ClearOtherDefaultsAsync(exceptId: null, request.PartnerType);
        }

        _context.BusinessPartners.Add(partner);
        await _ledgerAccounts.EnsureLedgerAccountsAsync(partner);
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

        if (request.IsDefault && !partner.IsDefault)
        {
            await ClearOtherDefaultsAsync(exceptId: id, request.PartnerType);
        }

        partner.IsDefault = request.IsDefault;

        await _ledgerAccounts.EnsureLedgerAccountsAsync(partner);
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

    // "Default" is scoped per side (Customer vs Supplier), not globally, so marking a partner the
    // default customer doesn't clobber a separately-marked default supplier; "Both" partners count
    // on whichever side(s) they overlap with the partner being set.
    private async Task ClearOtherDefaultsAsync(int? exceptId, PartnerType partnerType)
    {
        var isCustomerSide = partnerType is PartnerType.Customer or PartnerType.Both;
        var isSupplierSide = partnerType is PartnerType.Supplier or PartnerType.Both;

        var others = await _context.BusinessPartners
            .Where(p => p.IsDefault && p.Id != (exceptId ?? -1))
            .Where(p =>
                (isCustomerSide && (p.PartnerType == PartnerType.Customer || p.PartnerType == PartnerType.Both)) ||
                (isSupplierSide && (p.PartnerType == PartnerType.Supplier || p.PartnerType == PartnerType.Both)))
            .ToListAsync();

        foreach (var other in others)
        {
            other.IsDefault = false;
        }
    }
}
