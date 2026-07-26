namespace erp_backend.Auth.Dtos;

public class BranchSummary
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
}

public class LoginResponse
{
    public string Token { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public DateTime ExpiresAtUtc { get; set; }
    public int TenantId { get; set; }
    public string TenantName { get; set; } = string.Empty;
    public List<BranchSummary> Branches { get; set; } = [];
    public List<string> Rights { get; set; } = [];
}
