using System.ComponentModel.DataAnnotations;

namespace erp_backend.Tenants.Dtos;

public class CreateTenantRequest
{
    [Required]
    public string Name { get; set; } = string.Empty;

    [Required]
    public string Code { get; set; } = string.Empty;
}

public class UpdateTenantRequest
{
    [Required]
    public string Name { get; set; } = string.Empty;
}
