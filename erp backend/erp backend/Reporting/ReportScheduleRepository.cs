using erp_backend.Data;
using erp_backend.Exceptions;
using erp_backend.Messages;
using erp_backend.Models;
using erp_backend.Reporting.Dtos;
using Microsoft.EntityFrameworkCore;

namespace erp_backend.Reporting;

public class ReportScheduleRepository : IReportScheduleRepository
{
    private readonly AppDbContext _context;
    private readonly IReportRepository _reports;

    public ReportScheduleRepository(AppDbContext context, IReportRepository reports)
    {
        _context = context;
        _reports = reports;
    }

    public async Task<List<ReportSchedule>> GetAllAsync() =>
        await _context.ReportSchedules.OrderBy(s => s.ReportType).ToListAsync();

    public async Task<ReportSchedule> CreateAsync(ReportScheduleRequest request)
    {
        var schedule = new ReportSchedule
        {
            ReportType = request.ReportType,
            Recipients = request.Recipients,
            Frequency = request.Frequency,
        };

        _context.ReportSchedules.Add(schedule);
        await _context.SaveChangesAsync();

        return schedule;
    }

    public async Task<ReportSchedule> UpdateAsync(int id, ReportScheduleRequest request)
    {
        var schedule = await _context.ReportSchedules.FindAsync(id) ?? throw new NotFoundException(ResponseMessage.ReportScheduleNotFound);

        schedule.ReportType = request.ReportType;
        schedule.Recipients = request.Recipients;
        schedule.Frequency = request.Frequency;
        await _context.SaveChangesAsync();

        return schedule;
    }

    public Task ActivateAsync(int id) => SetActiveAsync(id, true);

    public Task DeactivateAsync(int id) => SetActiveAsync(id, false);

    public async Task DeleteAsync(int id)
    {
        var schedule = await _context.ReportSchedules.FindAsync(id) ?? throw new NotFoundException(ResponseMessage.ReportScheduleNotFound);

        _context.ReportSchedules.Remove(schedule);
        await _context.SaveChangesAsync();
    }

    /// <summary>
    /// Generates the scheduled report and returns it directly. There is no SMTP integration configured in this
    /// environment, so "emailing" is stubbed as an on-demand download here — swap this method's body for a real
    /// mailer call (attaching the same bytes) once SMTP credentials are available.
    /// </summary>
    public async Task<(byte[] Bytes, string ContentType, string FileName)> RunNowAsync(int id)
    {
        var schedule = await _context.ReportSchedules.FindAsync(id) ?? throw new NotFoundException(ResponseMessage.ReportScheduleNotFound);

        var now = DateTime.UtcNow;
        var result = await _reports.ExportAsync(
            schedule.ReportType,
            format: "pdf",
            asOf: now,
            from: now.AddMonths(-1),
            to: now,
            compare: false,
            costCenterId: null,
            subLedgerType: "Receivable",
            controlAccountId: null,
            year: now.Year,
            month: now.Month);

        schedule.LastRunAtUtc = now;
        await _context.SaveChangesAsync();

        return result;
    }

    private async Task SetActiveAsync(int id, bool isActive)
    {
        var schedule = await _context.ReportSchedules.FindAsync(id) ?? throw new NotFoundException(ResponseMessage.ReportScheduleNotFound);

        schedule.IsActive = isActive;
        await _context.SaveChangesAsync();
    }
}
