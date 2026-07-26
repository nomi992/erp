using System.ComponentModel.DataAnnotations;

namespace erp_backend.FiscalPeriods.Dtos;

public class FiscalPeriodRequest
{
    [Required]
    public string Name { get; set; } = string.Empty;

    [Required]
    public DateTime StartDate { get; set; }

    [Required]
    public DateTime EndDate { get; set; }
}
