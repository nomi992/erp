using erp_backend.Models;

namespace erp_backend.Partners.Dtos;

public class BusinessPartnerResponse
{
    public int Id { get; set; }
    public PartnerType PartnerType { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? ContactPerson { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public string? Address { get; set; }
    public int DefaultPaymentTermDays { get; set; }
    public decimal? CreditLimit { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAtUtc { get; set; }

    public static BusinessPartnerResponse FromEntity(BusinessPartner entity) =>
        new()
        {
            Id = entity.Id,
            PartnerType = entity.PartnerType,
            Code = entity.Code,
            Name = entity.Name,
            ContactPerson = entity.ContactPerson,
            Phone = entity.Phone,
            Email = entity.Email,
            Address = entity.Address,
            DefaultPaymentTermDays = entity.DefaultPaymentTermDays,
            CreditLimit = entity.CreditLimit,
            IsActive = entity.IsActive,
            CreatedAtUtc = entity.CreatedAtUtc,
        };
}
