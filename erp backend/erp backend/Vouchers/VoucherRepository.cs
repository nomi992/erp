using erp_backend.Accounting;
using erp_backend.Auditing;
using erp_backend.Data;
using erp_backend.Exceptions;
using erp_backend.Messages;
using erp_backend.Models;
using erp_backend.Vouchers.Dtos;
using Microsoft.EntityFrameworkCore;

namespace erp_backend.Vouchers;

public class VoucherRepository : IVoucherRepository
{
    private readonly AppDbContext _context;
    private readonly IFiscalPeriodGuard _fiscalPeriodGuard;
    private readonly IAuditLogger _auditLogger;
    private readonly IWebHostEnvironment _environment;

    public VoucherRepository(
        AppDbContext context,
        IFiscalPeriodGuard fiscalPeriodGuard,
        IAuditLogger auditLogger,
        IWebHostEnvironment environment)
    {
        _context = context;
        _fiscalPeriodGuard = fiscalPeriodGuard;
        _auditLogger = auditLogger;
        _environment = environment;
    }

    public async Task<List<VoucherListItemResponse>> GetAllAsync(
        VoucherType? type, VoucherStatus? status, DateTime? from, DateTime? to)
    {
        var query = _context.VoucherHeaders.Include(v => v.Details).AsQueryable();

        if (type is not null) query = query.Where(v => v.VoucherType == type);
        if (status is not null) query = query.Where(v => v.Status == status);
        if (from is not null) query = query.Where(v => v.Date >= from);
        if (to is not null) query = query.Where(v => v.Date <= to);

        var vouchers = await query.OrderByDescending(v => v.Date).ThenByDescending(v => v.Id).ToListAsync();

        return vouchers.Select(v => new VoucherListItemResponse
        {
            Id = v.Id,
            VoucherType = v.VoucherType,
            VoucherNo = v.VoucherNo,
            Date = v.Date,
            Narration = v.Narration,
            Status = v.Status,
            TotalDebit = v.Details.Sum(d => d.DebitAmount),
        }).ToList();
    }

    public async Task<VoucherResponse> GetByIdAsync(int id)
    {
        var voucher = await LoadVoucherAsync(id) ?? throw new NotFoundException(ResponseMessage.VoucherNotFound);
        return await ToResponseAsync(voucher);
    }

    public async Task<VoucherResponse> CreateAsync(CreateVoucherRequest request, string username)
    {
        await ValidateLinesAsync(request);

        if (await _fiscalPeriodGuard.IsDateInClosedPeriodAsync(request.Date))
        {
            throw new BadRequestException(ResponseMessage.VoucherDateInClosedPeriod);
        }

        await using var transaction = await _context.Database.BeginTransactionAsync();

        var voucherNo = await VoucherNumberGenerator.GenerateAsync(_context, request.VoucherType, request.Date);

        var voucher = new VoucherHeader
        {
            VoucherType = request.VoucherType,
            VoucherNo = voucherNo,
            Date = request.Date,
            Narration = request.Narration,
            CurrencyCode = request.CurrencyCode,
            ExchangeRate = request.ExchangeRate,
            Status = VoucherStatus.Draft,
            CreatedBy = username,
        };

        foreach (var line in request.Lines)
        {
            voucher.Details.Add(await BuildDetailAsync(request.VoucherType, line));
        }

        _context.VoucherHeaders.Add(voucher);
        await _context.SaveChangesAsync();
        await transaction.CommitAsync();

        var response = await ToResponseAsync(voucher);
        await _auditLogger.LogAsync("VoucherHeader", voucher.Id, "Create", newValue: response);

        return response;
    }

    public async Task<VoucherResponse> UpdateAsync(int id, CreateVoucherRequest request)
    {
        var voucher = await LoadVoucherAsync(id) ?? throw new NotFoundException(ResponseMessage.VoucherNotFound);

        if (voucher.Status != VoucherStatus.Draft)
        {
            throw new BadRequestException(ResponseMessage.VoucherNotDraftForEdit);
        }

        await ValidateLinesAsync(request);

        if (await _fiscalPeriodGuard.IsDateInClosedPeriodAsync(request.Date))
        {
            throw new BadRequestException(ResponseMessage.VoucherDateInClosedPeriod);
        }

        var oldValue = await ToResponseAsync(voucher);

        voucher.VoucherType = request.VoucherType;
        voucher.Date = request.Date;
        voucher.Narration = request.Narration;
        voucher.CurrencyCode = request.CurrencyCode;
        voucher.ExchangeRate = request.ExchangeRate;

        _context.VoucherDetails.RemoveRange(voucher.Details);
        voucher.Details.Clear();

        foreach (var line in request.Lines)
        {
            voucher.Details.Add(await BuildDetailAsync(request.VoucherType, line));
        }

        await _context.SaveChangesAsync();

        var response = await ToResponseAsync(voucher);
        await _auditLogger.LogAsync("VoucherHeader", voucher.Id, "Update", oldValue, response);

        return response;
    }

    public async Task SubmitAsync(int id)
    {
        var voucher = await _context.VoucherHeaders.FindAsync(id) ?? throw new NotFoundException(ResponseMessage.VoucherNotFound);

        if (voucher.Status != VoucherStatus.Draft)
        {
            throw new BadRequestException(ResponseMessage.VoucherNotDraftForSubmit);
        }

        voucher.Status = VoucherStatus.PendingApproval;
        await _context.SaveChangesAsync();
        await _auditLogger.LogAsync("VoucherHeader", voucher.Id, "Submit");
    }

    public async Task ApproveAsync(int id, string username)
    {
        var voucher = await _context.VoucherHeaders.FindAsync(id) ?? throw new NotFoundException(ResponseMessage.VoucherNotFound);

        if (voucher.Status != VoucherStatus.PendingApproval)
        {
            throw new BadRequestException(ResponseMessage.VoucherNotPendingForApprove);
        }

        if (await _fiscalPeriodGuard.IsDateInClosedPeriodAsync(voucher.Date))
        {
            throw new BadRequestException(ResponseMessage.VoucherDateInClosedPeriodForApproval);
        }

        voucher.Status = VoucherStatus.Posted;
        voucher.ApprovedBy = username;
        voucher.ApprovedAtUtc = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        await _auditLogger.LogAsync("VoucherHeader", voucher.Id, "Approve");
    }

    public async Task RejectAsync(int id)
    {
        var voucher = await _context.VoucherHeaders.FindAsync(id) ?? throw new NotFoundException(ResponseMessage.VoucherNotFound);

        if (voucher.Status != VoucherStatus.PendingApproval)
        {
            throw new BadRequestException(ResponseMessage.VoucherNotPendingForReject);
        }

        voucher.Status = VoucherStatus.Rejected;
        await _context.SaveChangesAsync();
        await _auditLogger.LogAsync("VoucherHeader", voucher.Id, "Reject");
    }

    public async Task<VoucherResponse> ReverseAsync(int id, string username)
    {
        var original = await LoadVoucherAsync(id) ?? throw new NotFoundException(ResponseMessage.VoucherNotFound);

        if (original.Status != VoucherStatus.Posted)
        {
            throw new BadRequestException(ResponseMessage.VoucherNotPostedForReverse);
        }

        // FiscalPeriod.StartDate/EndDate are mapped as Postgres "timestamp without time zone",
        // which Npgsql refuses to compare against a Kind=Utc DateTime. DateTime.UtcNow.Date keeps
        // Kind=Utc, so it must be re-specified as Unspecified (matching how request.Date arrives
        // from JSON model binding elsewhere in this file) before it can be used in that query.
        var reversalDate = DateTime.SpecifyKind(DateTime.UtcNow.Date, DateTimeKind.Unspecified);
        if (await _fiscalPeriodGuard.IsDateInClosedPeriodAsync(reversalDate))
        {
            throw new BadRequestException(ResponseMessage.TodayInClosedPeriod);
        }

        var voucherNo = await VoucherNumberGenerator.GenerateAsync(_context, original.VoucherType, reversalDate);

        var reversal = new VoucherHeader
        {
            VoucherType = original.VoucherType,
            VoucherNo = voucherNo,
            Date = reversalDate,
            Narration = $"Reversal of {original.VoucherNo}",
            CurrencyCode = original.CurrencyCode,
            ExchangeRate = original.ExchangeRate,
            Status = VoucherStatus.Posted,
            ReversalOfVoucherId = original.Id,
            CreatedBy = username,
            ApprovedBy = username,
            ApprovedAtUtc = DateTime.UtcNow,
        };

        foreach (var line in original.Details)
        {
            reversal.Details.Add(new VoucherDetail
            {
                AccountId = line.AccountId,
                DebitAmount = line.CreditAmount,
                CreditAmount = line.DebitAmount,
                CostCenterId = line.CostCenterId,
            });
        }

        _context.VoucherHeaders.Add(reversal);
        await _context.SaveChangesAsync();

        var response = await ToResponseAsync(reversal);
        await _auditLogger.LogAsync("VoucherHeader", original.Id, "ReversedBy", newValue: new { reversal.Id, reversal.VoucherNo });
        await _auditLogger.LogAsync("VoucherHeader", reversal.Id, "ReversalOf", newValue: new { original.Id, original.VoucherNo });

        return response;
    }

    public async Task<VoucherAttachmentResponse> UploadAttachmentAsync(int id, IFormFile file)
    {
        var voucher = await _context.VoucherHeaders.FindAsync(id) ?? throw new NotFoundException(ResponseMessage.VoucherNotFound);

        if (file.Length == 0)
        {
            throw new BadRequestException(ResponseMessage.AttachmentFileEmpty);
        }

        var uploadsDir = Path.Combine(_environment.ContentRootPath, "wwwroot", "uploads", "vouchers", id.ToString());
        Directory.CreateDirectory(uploadsDir);

        var storedFileName = $"{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";
        var filePath = Path.Combine(uploadsDir, storedFileName);

        await using (var stream = File.Create(filePath))
        {
            await file.CopyToAsync(stream);
        }

        var attachment = new VoucherAttachment
        {
            VoucherId = id,
            FileName = file.FileName,
            StoredFileName = storedFileName,
            ContentType = file.ContentType,
            SizeBytes = file.Length,
        };

        _context.VoucherAttachments.Add(attachment);
        await _context.SaveChangesAsync();
        await _auditLogger.LogAsync("VoucherHeader", id, "AttachmentUploaded", newValue: new { attachment.FileName });

        return new VoucherAttachmentResponse
        {
            Id = attachment.Id,
            FileName = attachment.FileName,
            ContentType = attachment.ContentType,
            SizeBytes = attachment.SizeBytes,
            UploadedAtUtc = attachment.UploadedAtUtc,
        };
    }

    public async Task<(byte[] Bytes, string ContentType, string FileName)> DownloadAttachmentAsync(int id, int attachmentId)
    {
        var attachment = await _context.VoucherAttachments
            .FirstOrDefaultAsync(a => a.Id == attachmentId && a.VoucherId == id)
            ?? throw new NotFoundException(ResponseMessage.AttachmentNotFound);

        var filePath = Path.Combine(_environment.ContentRootPath, "wwwroot", "uploads", "vouchers", id.ToString(), attachment.StoredFileName);
        if (!File.Exists(filePath))
        {
            throw new NotFoundException(ResponseMessage.AttachmentFileMissing);
        }

        var bytes = await File.ReadAllBytesAsync(filePath);
        return (bytes, attachment.ContentType, attachment.FileName);
    }

    public async Task DeleteAttachmentAsync(int id, int attachmentId)
    {
        var attachment = await _context.VoucherAttachments
            .FirstOrDefaultAsync(a => a.Id == attachmentId && a.VoucherId == id)
            ?? throw new NotFoundException(ResponseMessage.AttachmentNotFound);

        var filePath = Path.Combine(_environment.ContentRootPath, "wwwroot", "uploads", "vouchers", id.ToString(), attachment.StoredFileName);
        if (File.Exists(filePath))
        {
            File.Delete(filePath);
        }

        _context.VoucherAttachments.Remove(attachment);
        await _context.SaveChangesAsync();
    }

    private async Task<VoucherHeader?> LoadVoucherAsync(int id) =>
        await _context.VoucherHeaders
            .Include(v => v.Details).ThenInclude(d => d.Account)
            .Include(v => v.Details).ThenInclude(d => d.CostCenter)
            .Include(v => v.Details).ThenInclude(d => d.TaxRate)
            .Include(v => v.Attachments)
            .Include(v => v.ReversalOfVoucher)
            .FirstOrDefaultAsync(v => v.Id == id);

    private Task<VoucherResponse> ToResponseAsync(VoucherHeader voucher)
    {
        var response = new VoucherResponse
        {
            Id = voucher.Id,
            VoucherType = voucher.VoucherType,
            VoucherNo = voucher.VoucherNo,
            Date = voucher.Date,
            Narration = voucher.Narration,
            Status = voucher.Status,
            CurrencyCode = voucher.CurrencyCode,
            ExchangeRate = voucher.ExchangeRate,
            ReversalOfVoucherId = voucher.ReversalOfVoucherId,
            ReversalOfVoucherNo = voucher.ReversalOfVoucher?.VoucherNo,
            CreatedBy = voucher.CreatedBy,
            CreatedAtUtc = voucher.CreatedAtUtc,
            ApprovedBy = voucher.ApprovedBy,
            ApprovedAtUtc = voucher.ApprovedAtUtc,
            TotalDebit = voucher.Details.Sum(d => d.DebitAmount),
            TotalCredit = voucher.Details.Sum(d => d.CreditAmount),
            Lines = voucher.Details.Select(d => new VoucherLineResponse
            {
                Id = d.Id,
                AccountId = d.AccountId,
                AccountCode = d.Account?.Code ?? string.Empty,
                AccountName = d.Account?.Name ?? string.Empty,
                DebitAmount = d.DebitAmount,
                CreditAmount = d.CreditAmount,
                CostCenterId = d.CostCenterId,
                CostCenterName = d.CostCenter?.Name,
                TaxRateId = d.TaxRateId,
                TaxRateName = d.TaxRate?.Name,
                TaxAmount = d.TaxAmount,
            }).ToList(),
            Attachments = voucher.Attachments.Select(a => new VoucherAttachmentResponse
            {
                Id = a.Id,
                FileName = a.FileName,
                ContentType = a.ContentType,
                SizeBytes = a.SizeBytes,
                UploadedAtUtc = a.UploadedAtUtc,
            }).ToList(),
        };

        return Task.FromResult(response);
    }

    private async Task ValidateLinesAsync(CreateVoucherRequest request)
    {
        if (request.Lines.Count < 2)
        {
            throw new BadRequestException(ResponseMessage.VoucherRequiresTwoLines);
        }

        foreach (var line in request.Lines)
        {
            if (line.DebitAmount < 0 || line.CreditAmount < 0)
            {
                throw new BadRequestException(ResponseMessage.VoucherAmountsNegative);
            }

            if (line.DebitAmount == 0 && line.CreditAmount == 0)
            {
                throw new BadRequestException(ResponseMessage.VoucherLineRequiresAmount);
            }

            if (line.DebitAmount > 0 && line.CreditAmount > 0)
            {
                throw new BadRequestException(ResponseMessage.VoucherLineBothAmounts);
            }

            if (await _context.Accounts.FindAsync(line.AccountId) is null)
            {
                throw new BadRequestException(ResponseMessage.VoucherLineAccountNotFound, line.AccountId);
            }

            if (line.CostCenterId is int costCenterId && await _context.CostCenters.FindAsync(costCenterId) is null)
            {
                throw new BadRequestException(ResponseMessage.VoucherLineCostCenterNotFound, costCenterId);
            }

            if (line.TaxRateId is int taxRateId)
            {
                if (request.VoucherType is not (VoucherType.Sales or VoucherType.Purchase))
                {
                    throw new BadRequestException(ResponseMessage.VoucherTaxRateWrongType);
                }

                if (await _context.TaxRates.FindAsync(taxRateId) is null)
                {
                    throw new BadRequestException(ResponseMessage.VoucherLineTaxRateNotFound, taxRateId);
                }
            }
        }

        var totalDebit = request.Lines.Sum(l => l.DebitAmount);
        var totalCredit = request.Lines.Sum(l => l.CreditAmount);

        if (totalDebit != totalCredit)
        {
            throw new BadRequestException(ResponseMessage.VoucherUnbalanced, totalDebit, totalCredit);
        }
    }

    private async Task<VoucherDetail> BuildDetailAsync(VoucherType voucherType, VoucherLineRequest line)
    {
        var detail = new VoucherDetail
        {
            AccountId = line.AccountId,
            DebitAmount = line.DebitAmount,
            CreditAmount = line.CreditAmount,
            CostCenterId = line.CostCenterId,
            TaxRateId = line.TaxRateId,
        };

        if (line.TaxRateId is int taxRateId && voucherType is VoucherType.Sales or VoucherType.Purchase)
        {
            var taxRate = await _context.TaxRates.FindAsync(taxRateId);
            if (taxRate is not null)
            {
                var baseAmount = line.DebitAmount > 0 ? line.DebitAmount : line.CreditAmount;
                detail.TaxAmount = Math.Round(baseAmount * taxRate.Percentage / 100m, 2);
            }
        }

        return detail;
    }
}
