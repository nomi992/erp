using System.ComponentModel.DataAnnotations;

namespace erp_backend.Warehouses.Dtos;

public class WarehouseRequest
{
    [Required]
    public string Code { get; set; } = string.Empty;

    [Required]
    public string Name { get; set; } = string.Empty;

    public int? CostCenterId { get; set; }

    public bool IsDefault { get; set; }
}
