using erp_backend.Models;
using erp_backend.Reporting.Dtos;

namespace erp_backend.Reporting;

public interface IReportScheduleRepository
{
    Task<List<ReportSchedule>> GetAllAsync();
    Task<ReportSchedule> CreateAsync(ReportScheduleRequest request);
    Task<ReportSchedule> UpdateAsync(int id, ReportScheduleRequest request);
    Task ActivateAsync(int id);
    Task DeactivateAsync(int id);
    Task DeleteAsync(int id);
    Task<(byte[] Bytes, string ContentType, string FileName)> RunNowAsync(int id);
}
