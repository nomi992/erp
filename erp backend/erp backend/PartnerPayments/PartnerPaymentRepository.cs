using erp_backend.Auditing;
using erp_backend.Data;
using erp_backend.Exceptions;
using erp_backend.Inventory;
using erp_backend.Messages;
using erp_backend.Models;
using erp_backend.Partners;
using erp_backend.PartnerPayments.Dtos;
using erp_backend.Vouchers;
using Microsoft.EntityFrameworkCore;

namespace erp_backend.PartnerPayments;

public class PartnerPaymentRepository : IPartnerPaymentRepository
{
    private static readonly Dictionary<PaymentDirection, string> Prefixes = new()
    {
        [PaymentDirection.CustomerReceipt] = "RCPT",
        [PaymentDirection.SupplierPayment] = "SPMT",
    };

    private readonly AppDbContext _context;
    private readonly IPartnerLedgerAccountResolver _partnerLedgerAccounts;
    private readonly IAuditLogger _auditLogger;

    public PartnerPaymentRepository(
        AppDbContext context,
        IPartnerLedgerAccountResolver partnerLedgerAccounts,
        IAuditLogger auditLogger)
    {
        _context = context;
        _partnerLedgerAccounts = partnerLedgerAccounts;
        _auditLogger = auditLogger;
    }

    public async Task<PagedResult<PartnerPaymentListItemResponse>> GetAllAsync(
        PaymentDirection? direction, int? partnerId, PartnerPaymentStatus? status, DateTime? from, DateTime? to,
        string? search, string? sortBy, string? sortDirection,
        int pageNumber, int pageSize)
    {
        var query = _context.PartnerPaymentHeaders.Include(p => p.Partner).AsQueryable();

        if (direction is not null) query = query.Where(p => p.Direction == direction);
        if (partnerId is not null) query = query.Where(p => p.PartnerId == partnerId);
        if (status is not null) query = query.Where(p => p.Status == status);
        if (from is not null) query = query.Where(p => p.Date >= from);
        if (to is not null) query = query.Where(p => p.Date <= to);
        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(p => p.PaymentNo.Contains(search) || (p.Partner != null && p.Partner.Name.Contains(search)));
        }

        var direction2 = string.Equals(sortDirection, "asc", StringComparison.OrdinalIgnoreCase) ? "asc" : "desc";
        query = (sortBy?.ToLowerInvariant(), direction2) switch
        {
            ("paymentno", "asc") => query.OrderBy(p => p.PaymentNo),
            ("paymentno", _) => query.OrderByDescending(p => p.PaymentNo),
            ("partnername", "asc") => query.OrderBy(p => p.Partner!.Name),
            ("partnername", _) => query.OrderByDescending(p => p.Partner!.Name),
            ("date", "asc") => query.OrderBy(p => p.Date),
            ("date", _) => query.OrderByDescending(p => p.Date),
            ("totalamount", "asc") => query.OrderBy(p => p.TotalAmount),
            ("totalamount", _) => query.OrderByDescending(p => p.TotalAmount),
            ("status", "asc") => query.OrderBy(p => p.Status),
            ("status", _) => query.OrderByDescending(p => p.Status),
            _ => query.OrderByDescending(p => p.Date).ThenByDescending(p => p.Id),
        };

        var totalCount = await query.CountAsync();
        var items = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Select(p => new PartnerPaymentListItemResponse
            {
                Id = p.Id,
                Direction = p.Direction,
                PaymentNo = p.PaymentNo,
                PartnerId = p.PartnerId,
                PartnerName = p.Partner!.Name,
                Date = p.Date,
                TotalAmount = p.TotalAmount,
                Status = p.Status,
            })
            .AsNoTracking()
            .ToListAsync();

        return new PagedResult<PartnerPaymentListItemResponse> { Items = items, TotalCount = totalCount, PageNumber = pageNumber, PageSize = pageSize };
    }

    public async Task<PartnerPaymentResponse> GetByIdAsync(int id)
    {
        var header = await LoadFullAsync(id) ?? throw new NotFoundException(ResponseMessage.PartnerPaymentNotFound);
        return ToResponse(header);
    }

    public async Task<PaymentDirection> GetDirectionAsync(int id)
    {
        var header = await _context.PartnerPaymentHeaders.FindAsync(id) ?? throw new NotFoundException(ResponseMessage.PartnerPaymentNotFound);
        return header.Direction;
    }

    public async Task<PartnerPaymentResponse> CreateAsync(PartnerPaymentRequest request, string username)
    {
        var partner = await _context.BusinessPartners.FindAsync(request.PartnerId) ?? throw new BadRequestException(ResponseMessage.InvoicePartnerNotFound);

        if (await _context.Accounts.FindAsync(request.BankOrCashAccountId) is null)
        {
            throw new BadRequestException(ResponseMessage.PartnerPaymentBankAccountNotFound);
        }

        if (request.Allocations.Count == 0)
        {
            throw new BadRequestException(ResponseMessage.PartnerPaymentRequiresAtLeastOneAllocation);
        }

        var allocationTotal = request.Allocations.Sum(a => a.AllocatedAmount);
        if (allocationTotal != request.TotalAmount)
        {
            throw new BadRequestException(ResponseMessage.PartnerPaymentAllocationTotalMismatch, allocationTotal, request.TotalAmount);
        }

        var header = new PartnerPaymentHeader
        {
            Direction = request.Direction,
            PartnerId = request.PartnerId,
            Date = request.Date,
            BankOrCashAccountId = request.BankOrCashAccountId,
            TotalAmount = request.TotalAmount,
            Narration = request.Narration,
            Status = PartnerPaymentStatus.Draft,
            CreatedBy = username,
        };

        foreach (var allocationRequest in request.Allocations)
        {
            header.Allocations.Add(await BuildAllocationAsync(request.Direction, request.PartnerId, allocationRequest));
        }

        header.PaymentNo = await GeneratePaymentNoAsync(request.Direction, request.Date);

        _context.PartnerPaymentHeaders.Add(header);
        await _context.SaveChangesAsync();

        var response = await GetByIdAsync(header.Id);
        await _auditLogger.LogAsync("PartnerPaymentHeader", header.Id, "Create", newValue: response);

        return response;
    }

    public async Task SubmitAsync(int id)
    {
        var header = await _context.PartnerPaymentHeaders.FindAsync(id) ?? throw new NotFoundException(ResponseMessage.PartnerPaymentNotFound);

        if (header.Status != PartnerPaymentStatus.Draft)
        {
            throw new BadRequestException(ResponseMessage.PartnerPaymentNotDraftForSubmit);
        }

        header.Status = PartnerPaymentStatus.PendingApproval;
        await _context.SaveChangesAsync();
        await _auditLogger.LogAsync("PartnerPaymentHeader", header.Id, "Submit");
    }

    public async Task ApproveAsync(int id, string username)
    {
        var header = await _context.PartnerPaymentHeaders
            .Include(p => p.Allocations).ThenInclude(a => a.InvoiceHeader)
            .Include(p => p.Partner)
            .FirstOrDefaultAsync(p => p.Id == id)
            ?? throw new NotFoundException(ResponseMessage.PartnerPaymentNotFound);

        if (header.Status != PartnerPaymentStatus.PendingApproval)
        {
            throw new BadRequestException(ResponseMessage.PartnerPaymentNotPendingForApprove);
        }

        await using var transaction = await _context.Database.BeginTransactionAsync();

        foreach (var allocation in header.Allocations)
        {
            var invoice = allocation.InvoiceHeader!;

            if (allocation.AllocatedAmount > invoice.OutstandingAmount)
            {
                throw new BadRequestException(ResponseMessage.PartnerPaymentAllocationExceedsOutstanding, allocation.AllocatedAmount, invoice.OutstandingAmount);
            }

            invoice.AmountPaid += allocation.AllocatedAmount;
            invoice.OutstandingAmount -= allocation.AllocatedAmount;
            invoice.PaymentStatus = invoice.OutstandingAmount <= 0
                ? InvoicePaymentStatus.Paid
                : invoice.AmountPaid > 0 ? InvoicePaymentStatus.PartiallyPaid : InvoicePaymentStatus.Unpaid;
        }

        var voucher = new VoucherHeader
        {
            VoucherType = header.Direction == PaymentDirection.CustomerReceipt ? VoucherType.Receipt : VoucherType.Payment,
            Date = header.Date,
            CreatedBy = username,
            Status = VoucherStatus.Posted,
            ApprovedBy = username,
            ApprovedAtUtc = DateTime.UtcNow,
            Narration = $"Auto-posted from {header.Direction} {header.PaymentNo}",
        };

        if (header.Direction == PaymentDirection.CustomerReceipt)
        {
            var receivableAccountId = await _partnerLedgerAccounts.GetOrCreateReceivableAccountIdAsync(header.Partner!);
            voucher.Details.Add(new VoucherDetail { AccountId = header.BankOrCashAccountId, DebitAmount = header.TotalAmount });
            voucher.Details.Add(new VoucherDetail { AccountId = receivableAccountId, CreditAmount = header.TotalAmount });
        }
        else
        {
            var payableAccountId = await _partnerLedgerAccounts.GetOrCreatePayableAccountIdAsync(header.Partner!);
            voucher.Details.Add(new VoucherDetail { AccountId = payableAccountId, DebitAmount = header.TotalAmount });
            voucher.Details.Add(new VoucherDetail { AccountId = header.BankOrCashAccountId, CreditAmount = header.TotalAmount });
        }

        voucher.VoucherNo = await VoucherNumberGenerator.GenerateAsync(_context, voucher.VoucherType, header.Date);

        _context.VoucherHeaders.Add(voucher);
        await _context.SaveChangesAsync();

        header.LinkedVoucherId = voucher.Id;
        header.Status = PartnerPaymentStatus.Posted;
        header.ApprovedBy = username;
        header.ApprovedAtUtc = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        await transaction.CommitAsync();

        await _auditLogger.LogAsync("PartnerPaymentHeader", header.Id, "Approve", newValue: new { header.LinkedVoucherId });
    }

    public async Task RejectAsync(int id)
    {
        var header = await _context.PartnerPaymentHeaders.FindAsync(id) ?? throw new NotFoundException(ResponseMessage.PartnerPaymentNotFound);

        if (header.Status != PartnerPaymentStatus.PendingApproval)
        {
            throw new BadRequestException(ResponseMessage.PartnerPaymentNotPendingForReject);
        }

        header.Status = PartnerPaymentStatus.Rejected;
        await _context.SaveChangesAsync();
        await _auditLogger.LogAsync("PartnerPaymentHeader", header.Id, "Reject");
    }

    private async Task<PartnerPaymentAllocation> BuildAllocationAsync(PaymentDirection direction, int partnerId, PartnerPaymentAllocationRequest request)
    {
        if (request.AllocatedAmount <= 0)
        {
            throw new BadRequestException(ResponseMessage.PartnerPaymentAllocationAmountInvalid);
        }

        var invoice = await _context.InvoiceHeaders.FindAsync(request.InvoiceHeaderId) ?? throw new BadRequestException(ResponseMessage.InvoiceNotFound);

        if (invoice.PartnerId != partnerId)
        {
            throw new BadRequestException(ResponseMessage.PartnerPaymentAllocationPartnerMismatch);
        }

        var expectedTypes = direction == PaymentDirection.CustomerReceipt
            ? (InvoiceType.SalesInvoice, InvoiceType.SaleReturn)
            : (InvoiceType.PurchaseInvoice, InvoiceType.PurchaseReturn);

        if (invoice.InvoiceType != expectedTypes.Item1 && invoice.InvoiceType != expectedTypes.Item2)
        {
            throw new BadRequestException(ResponseMessage.PartnerPaymentAllocationInvoiceTypeMismatch);
        }

        if (invoice.OutstandingAmount <= 0)
        {
            throw new BadRequestException(ResponseMessage.PartnerPaymentInvoiceAlreadyFullyPaid);
        }

        if (request.AllocatedAmount > invoice.OutstandingAmount)
        {
            throw new BadRequestException(ResponseMessage.PartnerPaymentAllocationExceedsOutstanding, request.AllocatedAmount, invoice.OutstandingAmount);
        }

        return new PartnerPaymentAllocation { InvoiceHeaderId = request.InvoiceHeaderId, AllocatedAmount = request.AllocatedAmount };
    }

    private async Task<string> GeneratePaymentNoAsync(PaymentDirection direction, DateTime date)
    {
        var year = date.Year;
        var count = await _context.PartnerPaymentHeaders.CountAsync(p => p.Direction == direction && p.Date.Year == year);
        return StockDocumentNumberGenerator.Generate(Prefixes[direction], year, count);
    }

    private async Task<PartnerPaymentHeader?> LoadFullAsync(int id) =>
        await _context.PartnerPaymentHeaders
            .Include(p => p.Partner)
            .Include(p => p.BankOrCashAccount)
            .Include(p => p.LinkedVoucher)
            .Include(p => p.Allocations).ThenInclude(a => a.InvoiceHeader)
            .FirstOrDefaultAsync(p => p.Id == id);

    private static PartnerPaymentResponse ToResponse(PartnerPaymentHeader header) => new()
    {
        Id = header.Id,
        Direction = header.Direction,
        PaymentNo = header.PaymentNo,
        PartnerId = header.PartnerId,
        PartnerName = header.Partner?.Name ?? string.Empty,
        Date = header.Date,
        BankOrCashAccountId = header.BankOrCashAccountId,
        BankOrCashAccountName = header.BankOrCashAccount?.Name ?? string.Empty,
        TotalAmount = header.TotalAmount,
        Narration = header.Narration,
        Status = header.Status,
        LinkedVoucherId = header.LinkedVoucherId,
        LinkedVoucherNo = header.LinkedVoucher?.VoucherNo,
        CreatedBy = header.CreatedBy,
        CreatedAtUtc = header.CreatedAtUtc,
        ApprovedBy = header.ApprovedBy,
        ApprovedAtUtc = header.ApprovedAtUtc,
        Allocations = header.Allocations.Select(a => new PartnerPaymentAllocationResponse
        {
            Id = a.Id,
            InvoiceHeaderId = a.InvoiceHeaderId,
            InvoiceNo = a.InvoiceHeader?.InvoiceNo ?? string.Empty,
            AllocatedAmount = a.AllocatedAmount,
            InvoiceOutstandingAmount = a.InvoiceHeader?.OutstandingAmount ?? 0,
        }).ToList(),
    };
}
