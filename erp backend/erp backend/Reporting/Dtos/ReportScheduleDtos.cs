using System.ComponentModel.DataAnnotations;
using erp_backend.Models;

namespace erp_backend.Reporting.Dtos;

public class ReportScheduleRequest
{
    [Required]
    public string ReportType { get; set; } = string.Empty;

    [Required]
    public string Recipients { get; set; } = string.Empty;

    [Required]
    public RecurringFrequency Frequency { get; set; }
}
