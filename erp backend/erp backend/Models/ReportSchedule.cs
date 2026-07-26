using erp_backend.Models.Abstractions;

namespace erp_backend.Models;

public class ReportSchedule : IBranchScoped
{
    public int Id { get; set; }
    public int TenantId { get; set; }
    public int BranchId { get; set; }
    public string ReportType { get; set; } = string.Empty;
    public string Recipients { get; set; } = string.Empty;
    public RecurringFrequency Frequency { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime? LastRunAtUtc { get; set; }
}
