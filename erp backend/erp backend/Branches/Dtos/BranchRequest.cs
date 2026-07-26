using System.ComponentModel.DataAnnotations;

namespace erp_backend.Branches.Dtos;

public class CreateBranchRequest
{
    /// <summary>Only honored for a SystemAdmin caller; a tenant Admin always creates in their own tenant.</summary>
    public int? TenantId { get; set; }

    [Required]
    public string Name { get; set; } = string.Empty;

    [Required]
    public string Code { get; set; } = string.Empty;
}

public class UpdateBranchRequest
{
    [Required]
    public string Name { get; set; } = string.Empty;

    [Required]
    public string Code { get; set; } = string.Empty;
}
