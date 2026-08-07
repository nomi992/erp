using erp_backend.Accounting;
using erp_backend.Auditing;
using erp_backend.Data;
using erp_backend.Exceptions;
using erp_backend.Inventory;
using erp_backend.Messages;
using erp_backend.Models;
using erp_backend.StockAdjustments.Dtos;
using erp_backend.Vouchers;
using Microsoft.EntityFrameworkCore;

namespace erp_backend.StockAdjustments;

public class StockAdjustmentRepository : IStockAdjustmentRepository
{
    private readonly AppDbContext _context;
    private readonly IFiscalPeriodGuard _fiscalPeriodGuard;
    private readonly IStockMovementService _stockMovementService;
    private readonly IStockAccountMappingRepository _stockAccountMappings;
    private readonly IAuditLogger _auditLogger;

    public StockAdjustmentRepository(
        AppDbContext context,
        IFiscalPeriodGuard fiscalPeriodGuard,
        IStockMovementService stockMovementService,
        IStockAccountMappingRepository stockAccountMappings,
        IAuditLogger auditLogger)
    {
        _context = context;
        _fiscalPeriodGuard = fiscalPeriodGuard;
        _stockMovementService = stockMovementService;
        _stockAccountMappings = stockAccountMappings;
        _auditLogger = auditLogger;
    }

    public async Task<PagedResult<StockAdjustmentListItemResponse>> GetAllAsync(
        AdjustmentReasonCode? reasonCode, StockAdjustmentStatus? status, DateTime? from, DateTime? to,
        string? search, string? sortBy, string? sortDirection,
        int pageNumber, int pageSize)
    {
        var query = _context.StockAdjustmentHeaders.Include(a => a.Warehouse).AsQueryable();

        if (reasonCode is not null) query = query.Where(a => a.ReasonCode == reasonCode);
        if (status is not null) query = query.Where(a => a.Status == status);
        if (from is not null) query = query.Where(a => a.Date >= from);
        if (to is not null) query = query.Where(a => a.Date <= to);

        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(a => a.AdjustmentNo.Contains(search) || (a.Warehouse != null && a.Warehouse.Name.Contains(search)));
        }

        var direction = string.Equals(sortDirection, "asc", StringComparison.OrdinalIgnoreCase) ? "asc" : "desc";
        query = (sortBy?.ToLowerInvariant(), direction) switch
        {
            ("adjustmentno", "asc") => query.OrderBy(a => a.AdjustmentNo),
            ("adjustmentno", _) => query.OrderByDescending(a => a.AdjustmentNo),
            ("warehousename", "asc") => query.OrderBy(a => a.Warehouse!.Name),
            ("warehousename", _) => query.OrderByDescending(a => a.Warehouse!.Name),
            ("reasoncode", "asc") => query.OrderBy(a => a.ReasonCode),
            ("reasoncode", _) => query.OrderByDescending(a => a.ReasonCode),
            ("date", "asc") => query.OrderBy(a => a.Date),
            ("date", _) => query.OrderByDescending(a => a.Date),
            ("status", "asc") => query.OrderBy(a => a.Status),
            ("status", _) => query.OrderByDescending(a => a.Status),
            _ => query.OrderByDescending(a => a.Date).ThenByDescending(a => a.Id),
        };

        var totalCount = await query.CountAsync();
        var items = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Select(a => new StockAdjustmentListItemResponse
            {
                Id = a.Id,
                AdjustmentNo = a.AdjustmentNo,
                WarehouseName = a.Warehouse!.Name,
                Date = a.Date,
                ReasonCode = a.ReasonCode,
                Status = a.Status,
            })
            .AsNoTracking()
            .ToListAsync();

        return new PagedResult<StockAdjustmentListItemResponse> { Items = items, TotalCount = totalCount, PageNumber = pageNumber, PageSize = pageSize };
    }

    public async Task<StockAdjustmentResponse> GetByIdAsync(int id)
    {
        var header = await LoadFullAsync(id) ?? throw new NotFoundException(ResponseMessage.StockAdjustmentNotFound);
        return ToResponse(header);
    }

    public async Task<StockAdjustmentResponse> CreateAsync(StockAdjustmentRequest request, string username)
    {
        if (await _context.Warehouses.FindAsync(request.WarehouseId) is null)
        {
            throw new BadRequestException(ResponseMessage.StockMovementWarehouseNotFound);
        }

        if (request.Lines.Count == 0)
        {
            throw new BadRequestException(ResponseMessage.StockAdjustmentRequiresAtLeastOneLine);
        }

        var header = new StockAdjustmentHeader
        {
            WarehouseId = request.WarehouseId,
            Date = request.Date,
            ReasonCode = request.ReasonCode,
            Narration = request.Narration,
            Status = StockAdjustmentStatus.Draft,
            CreatedBy = username,
        };

        foreach (var lineRequest in request.Lines)
        {
            header.Lines.Add(await BuildLineAsync(lineRequest));
        }

        header.AdjustmentNo = await GenerateAdjustmentNoAsync(request.Date);

        _context.StockAdjustmentHeaders.Add(header);
        await _context.SaveChangesAsync();

        var response = await GetByIdAsync(header.Id);
        await _auditLogger.LogAsync("StockAdjustmentHeader", header.Id, "Create", newValue: response);

        return response;
    }

    public async Task SubmitAsync(int id)
    {
        var header = await _context.StockAdjustmentHeaders.FindAsync(id) ?? throw new NotFoundException(ResponseMessage.StockAdjustmentNotFound);

        if (header.Status != StockAdjustmentStatus.Draft)
        {
            throw new BadRequestException(ResponseMessage.StockAdjustmentNotDraftForSubmit);
        }

        header.Status = StockAdjustmentStatus.PendingApproval;
        await _context.SaveChangesAsync();
        await _auditLogger.LogAsync("StockAdjustmentHeader", header.Id, "Submit");
    }

    public async Task ApproveAsync(int id, string username)
    {
        var header = await _context.StockAdjustmentHeaders
            .Include(a => a.Lines).ThenInclude(l => l.ProductVariant).ThenInclude(v => v!.Product)
            .FirstOrDefaultAsync(a => a.Id == id)
            ?? throw new NotFoundException(ResponseMessage.StockAdjustmentNotFound);

        if (header.Status != StockAdjustmentStatus.PendingApproval)
        {
            throw new BadRequestException(ResponseMessage.StockAdjustmentNotPendingForApprove);
        }

        if (await _fiscalPeriodGuard.IsDateInClosedPeriodAsync(header.Date))
        {
            throw new BadRequestException(ResponseMessage.StockAdjustmentDateInClosedPeriod);
        }

        await using var transaction = await _context.Database.BeginTransactionAsync();

        var inventoryDebits = new Dictionary<int, decimal>();
        var inventoryCredits = new Dictionary<int, decimal>();
        var varianceCredits = new Dictionary<int, decimal>();
        var varianceDebits = new Dictionary<int, decimal>();
        var openingBalanceCredits = new Dictionary<int, decimal>();

        foreach (var line in header.Lines)
        {
            var lineMapping = await _stockAccountMappings.ResolveForCategoryAsync(line.ProductVariant!.Product!.ProductCategoryId);

            if (line.Direction == AdjustmentDirection.Increase)
            {
                if (header.ReasonCode == AdjustmentReasonCode.OpeningBalance &&
                    await _context.StockLedgerEntries.AnyAsync(e => e.ProductVariantId == line.ProductVariantId && e.WarehouseId == header.WarehouseId))
                {
                    throw new BadRequestException(ResponseMessage.StockAdjustmentOpeningBalanceAlreadyExists);
                }

                var unitCost = line.UnitCost ?? throw new BadRequestException(ResponseMessage.StockAdjustmentUnitCostRequiredForIncrease);

                await _stockMovementService.ReceiveAsync(
                    line.ProductVariantId, header.WarehouseId, line.BaseQty, unitCost, header.Date,
                    header.ReasonCode == AdjustmentReasonCode.OpeningBalance ? MovementType.OpeningBalance : MovementType.AdjustmentIncrease,
                    SourceDocumentType.StockAdjustment, header.Id, line.Id,
                    $"Stock Adjustment {header.AdjustmentNo}", username);

                line.LineValue = Math.Round(line.BaseQty * unitCost, 2);
                inventoryDebits[lineMapping.InventoryAssetAccountId] = inventoryDebits.GetValueOrDefault(lineMapping.InventoryAssetAccountId) + line.LineValue;

                if (header.ReasonCode == AdjustmentReasonCode.OpeningBalance)
                {
                    openingBalanceCredits[lineMapping.OpeningBalanceEquityAccountId] =
                        openingBalanceCredits.GetValueOrDefault(lineMapping.OpeningBalanceEquityAccountId) + line.LineValue;
                }
                else
                {
                    varianceCredits[lineMapping.StockAdjustmentVarianceAccountId] =
                        varianceCredits.GetValueOrDefault(lineMapping.StockAdjustmentVarianceAccountId) + line.LineValue;
                }
            }
            else
            {
                var result = await _stockMovementService.IssueAsync(
                    line.ProductVariantId, header.WarehouseId, line.BaseQty, header.Date,
                    MovementType.AdjustmentDecrease, SourceDocumentType.StockAdjustment, header.Id, line.Id,
                    $"Stock Adjustment {header.AdjustmentNo}", username);

                line.UnitCost = result.NewAverageCost;
                line.LineValue = Math.Round(line.BaseQty * result.NewAverageCost, 2);

                inventoryCredits[lineMapping.InventoryAssetAccountId] =
                    inventoryCredits.GetValueOrDefault(lineMapping.InventoryAssetAccountId) + line.LineValue;
                varianceDebits[lineMapping.StockAdjustmentVarianceAccountId] =
                    varianceDebits.GetValueOrDefault(lineMapping.StockAdjustmentVarianceAccountId) + line.LineValue;
            }
        }

        var voucher = new VoucherHeader
        {
            VoucherType = VoucherType.Journal,
            Date = header.Date,
            CreatedBy = username,
            Status = VoucherStatus.Posted,
            ApprovedBy = username,
            ApprovedAtUtc = DateTime.UtcNow,
            Narration = $"Auto-posted from Stock Adjustment {header.AdjustmentNo}",
        };

        foreach (var (accountId, amount) in inventoryDebits)
        {
            voucher.Details.Add(new VoucherDetail { AccountId = accountId, DebitAmount = amount });
        }

        foreach (var (accountId, amount) in varianceDebits)
        {
            voucher.Details.Add(new VoucherDetail { AccountId = accountId, DebitAmount = amount });
        }

        foreach (var (accountId, amount) in inventoryCredits)
        {
            voucher.Details.Add(new VoucherDetail { AccountId = accountId, CreditAmount = amount });
        }

        foreach (var (accountId, amount) in varianceCredits)
        {
            voucher.Details.Add(new VoucherDetail { AccountId = accountId, CreditAmount = amount });
        }

        foreach (var (accountId, amount) in openingBalanceCredits)
        {
            voucher.Details.Add(new VoucherDetail { AccountId = accountId, CreditAmount = amount });
        }

        voucher.VoucherNo = await VoucherNumberGenerator.GenerateAsync(_context, voucher.VoucherType, header.Date);

        _context.VoucherHeaders.Add(voucher);
        await _context.SaveChangesAsync();

        header.LinkedVoucherId = voucher.Id;
        header.Status = StockAdjustmentStatus.Posted;
        header.ApprovedBy = username;
        header.ApprovedAtUtc = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        await transaction.CommitAsync();

        await _auditLogger.LogAsync("StockAdjustmentHeader", header.Id, "Approve", newValue: new { header.LinkedVoucherId });
    }

    public async Task RejectAsync(int id)
    {
        var header = await _context.StockAdjustmentHeaders.FindAsync(id) ?? throw new NotFoundException(ResponseMessage.StockAdjustmentNotFound);

        if (header.Status != StockAdjustmentStatus.PendingApproval)
        {
            throw new BadRequestException(ResponseMessage.StockAdjustmentNotPendingForReject);
        }

        header.Status = StockAdjustmentStatus.Rejected;
        await _context.SaveChangesAsync();
        await _auditLogger.LogAsync("StockAdjustmentHeader", header.Id, "Reject");
    }

    private async Task<StockAdjustmentLine> BuildLineAsync(StockAdjustmentLineRequest lineRequest)
    {
        var variant = await _context.ProductVariants.Include(v => v.Product)
            .FirstOrDefaultAsync(v => v.Id == lineRequest.ProductVariantId)
            ?? throw new BadRequestException(ResponseMessage.StockMovementVariantNotFound);

        // A stock adjustment only makes sense for a physically stocked item — non-stock-tracked
        // products (services) never carry a StockBalance to adjust.
        if (!variant.Product!.IsStockTracked)
        {
            throw new BadRequestException(ResponseMessage.StockAdjustmentProductNotStockTracked);
        }

        if (lineRequest.BaseQty <= 0)
        {
            throw new BadRequestException(ResponseMessage.InvoiceLineQuantityInvalid);
        }

        if (lineRequest.Direction == AdjustmentDirection.Increase && lineRequest.UnitCost is null)
        {
            throw new BadRequestException(ResponseMessage.StockAdjustmentUnitCostRequiredForIncrease);
        }

        var lineValue = lineRequest.Direction == AdjustmentDirection.Increase
            ? Math.Round(lineRequest.BaseQty * lineRequest.UnitCost!.Value, 2)
            : 0;

        return new StockAdjustmentLine
        {
            ProductVariantId = lineRequest.ProductVariantId,
            Direction = lineRequest.Direction,
            BaseQty = lineRequest.BaseQty,
            UnitCost = lineRequest.UnitCost,
            LineValue = lineValue,
        };
    }

    private async Task<string> GenerateAdjustmentNoAsync(DateTime date)
    {
        var year = date.Year;
        var count = await _context.StockAdjustmentHeaders.CountAsync(a => a.Date.Year == year);
        return StockDocumentNumberGenerator.Generate("ADJ", year, count);
    }

    private async Task<StockAdjustmentHeader?> LoadFullAsync(int id) =>
        await _context.StockAdjustmentHeaders
            .Include(a => a.Warehouse)
            .Include(a => a.LinkedVoucher)
            .Include(a => a.Lines).ThenInclude(l => l.ProductVariant).ThenInclude(v => v!.Product)
            .FirstOrDefaultAsync(a => a.Id == id);

    private static StockAdjustmentResponse ToResponse(StockAdjustmentHeader header) => new()
    {
        Id = header.Id,
        AdjustmentNo = header.AdjustmentNo,
        WarehouseId = header.WarehouseId,
        WarehouseName = header.Warehouse?.Name ?? string.Empty,
        Date = header.Date,
        ReasonCode = header.ReasonCode,
        Status = header.Status,
        Narration = header.Narration,
        LinkedVoucherId = header.LinkedVoucherId,
        LinkedVoucherNo = header.LinkedVoucher?.VoucherNo,
        CreatedBy = header.CreatedBy,
        CreatedAtUtc = header.CreatedAtUtc,
        ApprovedBy = header.ApprovedBy,
        ApprovedAtUtc = header.ApprovedAtUtc,
        Lines = header.Lines.Select(l => new StockAdjustmentLineResponse
        {
            Id = l.Id,
            ProductVariantId = l.ProductVariantId,
            ProductVariantName = l.ProductVariant?.Name ?? string.Empty,
            ProductName = l.ProductVariant?.Product?.Name ?? string.Empty,
            Direction = l.Direction,
            BaseQty = l.BaseQty,
            UnitCost = l.UnitCost,
            LineValue = l.LineValue,
        }).ToList(),
    };
}
