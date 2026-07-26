using System.ComponentModel.DataAnnotations;
using erp_backend.Models;

namespace erp_backend.Partners.Dtos;

public class BusinessPartnerRequest
{
    public PartnerType PartnerType { get; set; }

    [Required]
    public string Code { get; set; } = string.Empty;

    [Required]
    public string Name { get; set; } = string.Empty;

    public string? ContactPerson { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public string? Address { get; set; }
    public int DefaultPaymentTermDays { get; set; }
    public decimal? CreditLimit { get; set; }
}
