namespace erp_backend.Models.Abstractions;

public interface ITenantScoped
{
    int TenantId { get; set; }
}
