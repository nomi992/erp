using System.ComponentModel.DataAnnotations;

namespace erp_backend.CostCenters.Dtos;

public class CostCenterRequest
{
    [Required]
    public string Name { get; set; } = string.Empty;

    public int? ParentCostCenterId { get; set; }
}
