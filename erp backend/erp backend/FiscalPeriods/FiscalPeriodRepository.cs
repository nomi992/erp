using erp_backend.Exceptions;
using erp_backend.FiscalPeriods.Dtos;
using erp_backend.Messages;
using erp_backend.Models;
using erp_backend.Repositories;

namespace erp_backend.FiscalPeriods;

public class FiscalPeriodRepository : IFiscalPeriodRepository
{
    private readonly IRepository<FiscalPeriod> _periods;

    public FiscalPeriodRepository(IRepository<FiscalPeriod> periods)
    {
        _periods = periods;
    }

    public async Task<List<FiscalPeriod>> GetAllAsync() =>
        (await _periods.GetAllAsync()).OrderBy(p => p.StartDate).ToList();

    public async Task<FiscalPeriod> GetByIdAsync(int id) =>
        await _periods.GetByIdAsync(id) ?? throw new NotFoundException(ResponseMessage.FiscalPeriodNotFound);

    public async Task<FiscalPeriod> CreateAsync(FiscalPeriodRequest request)
    {
        if (request.EndDate < request.StartDate)
        {
            throw new BadRequestException(ResponseMessage.FiscalPeriodEndBeforeStart);
        }

        var overlapping = (await _periods.FindAsync(p =>
            request.StartDate <= p.EndDate && request.EndDate >= p.StartDate)).Count > 0;
        if (overlapping)
        {
            throw new BadRequestException(ResponseMessage.FiscalPeriodOverlaps);
        }

        var period = new FiscalPeriod
        {
            Name = request.Name,
            StartDate = request.StartDate,
            EndDate = request.EndDate,
        };

        await _periods.AddAsync(period);
        await _periods.SaveChangesAsync();

        return period;
    }

    public Task CloseAsync(int id) => SetClosedAsync(id, true);

    public Task OpenAsync(int id) => SetClosedAsync(id, false);

    private async Task SetClosedAsync(int id, bool isClosed)
    {
        var period = await _periods.GetByIdAsync(id) ?? throw new NotFoundException(ResponseMessage.FiscalPeriodNotFound);

        period.IsClosed = isClosed;
        _periods.Update(period);
        await _periods.SaveChangesAsync();
    }
}
