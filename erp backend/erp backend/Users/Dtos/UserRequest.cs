using System.ComponentModel.DataAnnotations;

namespace erp_backend.Users.Dtos;

public class CreateUserRequest
{
    /// <summary>Only honored for a SystemAdmin caller; a tenant Admin always creates in their own tenant.</summary>
    public int? TenantId { get; set; }

    [Required]
    public string Username { get; set; } = string.Empty;

    [Required]
    public string Password { get; set; } = string.Empty;

    [Required]
    public int RoleId { get; set; }

    public List<int> BranchIds { get; set; } = [];
}

public class UpdateUserRequest
{
    [Required]
    public int RoleId { get; set; }

    public bool IsActive { get; set; } = true;
}

public class GrantBranchRequest
{
    [Required]
    public int BranchId { get; set; }
}
