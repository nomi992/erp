namespace erp_backend.Auth;

public interface ICurrentTenantContext
{
    int TenantId { get; }
    int? BranchId { get; }
    bool IsTenantResolved { get; }
    bool IsBranchResolved { get; }

    void SetTenant(int tenantId);
    void SetBranch(int branchId);
}
