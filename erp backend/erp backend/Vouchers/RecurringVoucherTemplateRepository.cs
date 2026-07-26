using erp_backend.Data;
using erp_backend.Exceptions;
using erp_backend.Messages;
using erp_backend.Models;
using erp_backend.Vouchers.Dtos;
using Microsoft.EntityFrameworkCore;

namespace erp_backend.Vouchers;

public class RecurringVoucherTemplateRepository : IRecurringVoucherTemplateRepository
{
    private readonly AppDbContext _context;

    public RecurringVoucherTemplateRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<RecurringVoucherTemplateResponse>> GetAllAsync()
    {
        var templates = await _context.RecurringVoucherTemplates
            .Include(t => t.Lines).ThenInclude(l => l.Account)
            .Include(t => t.Lines).ThenInclude(l => l.CostCenter)
            .OrderBy(t => t.Name)
            .ToListAsync();

        return templates.Select(RecurringVoucherTemplateResponse.FromEntity).ToList();
    }

    public async Task<RecurringVoucherTemplateResponse> GetByIdAsync(int id)
    {
        var template = await LoadAsync(id) ?? throw new NotFoundException(ResponseMessage.TemplateNotFound);
        return RecurringVoucherTemplateResponse.FromEntity(template);
    }

    public async Task<RecurringVoucherTemplateResponse> CreateAsync(RecurringVoucherTemplateRequest request)
    {
        await ValidateAsync(request);

        var template = new RecurringVoucherTemplate
        {
            Name = request.Name,
            VoucherType = request.VoucherType,
            NarrationTemplate = request.NarrationTemplate,
            Frequency = request.Frequency,
            NextRunDate = request.NextRunDate,
        };

        foreach (var line in request.Lines)
        {
            template.Lines.Add(new RecurringVoucherTemplateLine
            {
                AccountId = line.AccountId,
                DebitAmount = line.DebitAmount,
                CreditAmount = line.CreditAmount,
                CostCenterId = line.CostCenterId,
            });
        }

        _context.RecurringVoucherTemplates.Add(template);
        await _context.SaveChangesAsync();

        var reloaded = await LoadAsync(template.Id);
        return RecurringVoucherTemplateResponse.FromEntity(reloaded!);
    }

    public async Task<RecurringVoucherTemplateResponse> UpdateAsync(int id, RecurringVoucherTemplateRequest request)
    {
        var template = await LoadAsync(id) ?? throw new NotFoundException(ResponseMessage.TemplateNotFound);

        await ValidateAsync(request);

        template.Name = request.Name;
        template.VoucherType = request.VoucherType;
        template.NarrationTemplate = request.NarrationTemplate;
        template.Frequency = request.Frequency;
        template.NextRunDate = request.NextRunDate;

        _context.RecurringVoucherTemplateLines.RemoveRange(template.Lines);
        template.Lines.Clear();

        foreach (var line in request.Lines)
        {
            template.Lines.Add(new RecurringVoucherTemplateLine
            {
                AccountId = line.AccountId,
                DebitAmount = line.DebitAmount,
                CreditAmount = line.CreditAmount,
                CostCenterId = line.CostCenterId,
            });
        }

        await _context.SaveChangesAsync();

        var reloaded = await LoadAsync(id);
        return RecurringVoucherTemplateResponse.FromEntity(reloaded!);
    }

    public Task ActivateAsync(int id) => SetActiveAsync(id, true);

    public Task DeactivateAsync(int id) => SetActiveAsync(id, false);

    public async Task<(int VoucherId, string VoucherNo, DateTime NextRunDate)> GenerateNowAsync(int id, string username)
    {
        var template = await LoadAsync(id) ?? throw new NotFoundException(ResponseMessage.TemplateNotFound);

        if (!template.IsActive)
        {
            throw new BadRequestException(ResponseMessage.TemplateInactive);
        }

        var voucher = new VoucherHeader
        {
            VoucherType = template.VoucherType,
            VoucherNo = await VoucherNumberGenerator.GenerateAsync(_context, template.VoucherType, template.NextRunDate),
            Date = template.NextRunDate,
            Narration = template.NarrationTemplate,
            Status = VoucherStatus.Draft,
            CreatedBy = username,
        };

        foreach (var line in template.Lines)
        {
            voucher.Details.Add(new VoucherDetail
            {
                AccountId = line.AccountId,
                DebitAmount = line.DebitAmount,
                CreditAmount = line.CreditAmount,
                CostCenterId = line.CostCenterId,
            });
        }

        template.NextRunDate = template.Frequency switch
        {
            RecurringFrequency.Daily => template.NextRunDate.AddDays(1),
            RecurringFrequency.Weekly => template.NextRunDate.AddDays(7),
            RecurringFrequency.Monthly => template.NextRunDate.AddMonths(1),
            RecurringFrequency.Yearly => template.NextRunDate.AddYears(1),
            _ => template.NextRunDate,
        };

        _context.VoucherHeaders.Add(voucher);
        await _context.SaveChangesAsync();

        return (voucher.Id, voucher.VoucherNo, template.NextRunDate);
    }

    private async Task SetActiveAsync(int id, bool isActive)
    {
        var template = await _context.RecurringVoucherTemplates.FindAsync(id) ?? throw new NotFoundException(ResponseMessage.TemplateNotFound);

        template.IsActive = isActive;
        await _context.SaveChangesAsync();
    }

    private Task<RecurringVoucherTemplate?> LoadAsync(int id) =>
        _context.RecurringVoucherTemplates
            .Include(t => t.Lines).ThenInclude(l => l.Account)
            .Include(t => t.Lines).ThenInclude(l => l.CostCenter)
            .FirstOrDefaultAsync(t => t.Id == id);

    private async Task ValidateAsync(RecurringVoucherTemplateRequest request)
    {
        if (request.Lines.Count < 2)
        {
            throw new BadRequestException(ResponseMessage.TemplateRequiresTwoLines);
        }

        foreach (var line in request.Lines)
        {
            if (await _context.Accounts.FindAsync(line.AccountId) is null)
            {
                throw new BadRequestException(ResponseMessage.VoucherLineAccountNotFound, line.AccountId);
            }
        }
    }
}
