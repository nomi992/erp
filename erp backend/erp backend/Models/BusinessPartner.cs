using erp_backend.Models.Abstractions;

namespace erp_backend.Models;

public enum PartnerType
{
    Supplier,
    Customer,
    Both,
}

public class BusinessPartner : ITenantScoped
{
    public int Id { get; set; }
    public int TenantId { get; set; }
    public PartnerType PartnerType { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? ContactPerson { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public string? Address { get; set; }
    public int DefaultPaymentTermDays { get; set; }
    public decimal? CreditLimit { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
}
