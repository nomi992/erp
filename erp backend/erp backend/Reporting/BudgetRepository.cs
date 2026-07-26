using erp_backend.Data;
using erp_backend.Exceptions;
using erp_backend.Messages;
using erp_backend.Models;
using erp_backend.Reporting.Dtos;
using Microsoft.EntityFrameworkCore;

namespace erp_backend.Reporting;

public class BudgetRepository : IBudgetRepository
{
    private readonly AppDbContext _context;

    public BudgetRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<BudgetResponse>> GetAllAsync(int? year, int? month)
    {
        var query = _context.Budgets.Include(b => b.Account).AsQueryable();

        if (year is not null) query = query.Where(b => b.Year == year);
        if (month is not null) query = query.Where(b => b.Month == month);

        var budgets = await query.OrderBy(b => b.Year).ThenBy(b => b.Month).ThenBy(b => b.Account!.Code).ToListAsync();
        return budgets.Select(BudgetResponse.FromEntity).ToList();
    }

    public async Task<BudgetResponse> CreateAsync(BudgetRequest request)
    {
        if (await _context.Accounts.FindAsync(request.AccountId) is null)
        {
            throw new BadRequestException(ResponseMessage.AccountNotFound);
        }

        var exists = await _context.Budgets.AnyAsync(b =>
            b.AccountId == request.AccountId && b.Year == request.Year && b.Month == request.Month);
        if (exists)
        {
            throw new BadRequestException(ResponseMessage.BudgetAlreadyExists);
        }

        var budget = new Budget
        {
            AccountId = request.AccountId,
            Year = request.Year,
            Month = request.Month,
            BudgetedAmount = request.BudgetedAmount,
        };

        _context.Budgets.Add(budget);
        await _context.SaveChangesAsync();

        await _context.Entry(budget).Reference(b => b.Account).LoadAsync();
        return BudgetResponse.FromEntity(budget);
    }

    public async Task<BudgetResponse> UpdateAsync(int id, BudgetRequest request)
    {
        var budget = await _context.Budgets.Include(b => b.Account).FirstOrDefaultAsync(b => b.Id == id)
            ?? throw new NotFoundException(ResponseMessage.BudgetNotFound);

        budget.AccountId = request.AccountId;
        budget.Year = request.Year;
        budget.Month = request.Month;
        budget.BudgetedAmount = request.BudgetedAmount;

        await _context.SaveChangesAsync();
        await _context.Entry(budget).Reference(b => b.Account).LoadAsync();

        return BudgetResponse.FromEntity(budget);
    }

    public async Task DeleteAsync(int id)
    {
        var budget = await _context.Budgets.FindAsync(id) ?? throw new NotFoundException(ResponseMessage.BudgetNotFound);

        _context.Budgets.Remove(budget);
        await _context.SaveChangesAsync();
    }
}
