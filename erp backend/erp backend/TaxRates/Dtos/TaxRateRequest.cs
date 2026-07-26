using System.ComponentModel.DataAnnotations;

namespace erp_backend.TaxRates.Dtos;

public class TaxRateRequest
{
    [Required]
    public string Name { get; set; } = string.Empty;

    [Range(0, 100)]
    public decimal Percentage { get; set; }
}
