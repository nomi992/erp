namespace erp_backend.Models;

public class UserBranch
{
    public int UserId { get; set; }
    public User? User { get; set; }
    public int BranchId { get; set; }
    public Branch? Branch { get; set; }
    public DateTime GrantedAtUtc { get; set; } = DateTime.UtcNow;
}
