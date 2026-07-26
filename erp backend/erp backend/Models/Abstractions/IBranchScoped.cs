namespace erp_backend.Models.Abstractions;

public interface IBranchScoped : ITenantScoped
{
    int BranchId { get; set; }
}
