using erp_backend.Data;
using Microsoft.EntityFrameworkCore;

namespace erp_backend.Accounting;

public class FiscalPeriodGuard : IFiscalPeriodGuard
{
    private readonly AppDbContext _context;

    public FiscalPeriodGuard(AppDbContext context)
    {
        _context = context;
    }

    public async Task<bool> IsDateInClosedPeriodAsync(DateTime date) =>
        await _context.FiscalPeriods.AnyAsync(p => p.IsClosed && date >= p.StartDate && date <= p.EndDate);
}
