using System.ComponentModel.DataAnnotations;

namespace erp_backend.UnitsOfMeasure.Dtos;

public class UnitOfMeasureRequest
{
    [Required]
    public string Code { get; set; } = string.Empty;

    [Required]
    public string Name { get; set; } = string.Empty;
}
