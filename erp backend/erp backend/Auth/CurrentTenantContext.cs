namespace erp_backend.Auth;

public class CurrentTenantContext : ICurrentTenantContext
{
    public int TenantId { get; private set; }
    public int? BranchId { get; private set; }
    public bool IsTenantResolved => TenantId != 0;
    public bool IsBranchResolved => BranchId.HasValue;

    public void SetTenant(int tenantId) => TenantId = tenantId;

    public void SetBranch(int branchId) => BranchId = branchId;
}
