using erp_backend.Accounting;
using erp_backend.Auditing;
using erp_backend.Auth;
using erp_backend.Data;
using erp_backend.Exceptions;
using erp_backend.Inventory;
using erp_backend.Messages;
using erp_backend.Models;
using erp_backend.Transfers.Dtos;
using Microsoft.EntityFrameworkCore;

namespace erp_backend.Transfers;

public class StockTransferRepository : IStockTransferRepository
{
    private readonly AppDbContext _context;
    private readonly ICurrentTenantContext _tenantContext;
    private readonly IFiscalPeriodGuard _fiscalPeriodGuard;
    private readonly IStockMovementService _stockMovementService;
    private readonly IAuditLogger _auditLogger;

    public StockTransferRepository(
        AppDbContext context,
        ICurrentTenantContext tenantContext,
        IFiscalPeriodGuard fiscalPeriodGuard,
        IStockMovementService stockMovementService,
        IAuditLogger auditLogger)
    {
        _context = context;
        _tenantContext = tenantContext;
        _fiscalPeriodGuard = fiscalPeriodGuard;
        _stockMovementService = stockMovementService;
        _auditLogger = auditLogger;
    }

    public async Task<PagedResult<StockTransferListItemResponse>> GetAllAsync(
        StockTransferStatus? status, DateTime? from, DateTime? to, int pageNumber, int pageSize)
    {
        var query = _context.StockTransferHeaders
            .Include(t => t.SourceWarehouse)
            .Include(t => t.DestinationWarehouse)
            .AsQueryable();

        if (status is not null) query = query.Where(t => t.Status == status);
        if (from is not null) query = query.Where(t => t.Date >= from);
        if (to is not null) query = query.Where(t => t.Date <= to);

        query = query.OrderByDescending(t => t.Date).ThenByDescending(t => t.Id);

        var totalCount = await query.CountAsync();
        var items = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Select(t => new StockTransferListItemResponse
            {
                Id = t.Id,
                TransferNo = t.TransferNo,
                SourceWarehouseName = t.SourceWarehouse!.Name,
                DestinationWarehouseName = t.DestinationWarehouse!.Name,
                Date = t.Date,
                Status = t.Status,
            })
            .AsNoTracking()
            .ToListAsync();

        return new PagedResult<StockTransferListItemResponse> { Items = items, TotalCount = totalCount, PageNumber = pageNumber, PageSize = pageSize };
    }

    public async Task<StockTransferResponse> GetByIdAsync(int id)
    {
        var header = await LoadFullAsync(id) ?? throw new NotFoundException(ResponseMessage.StockTransferNotFound);
        return ToResponse(header);
    }

    public async Task<StockTransferResponse> CreateAsync(StockTransferRequest request, string username)
    {
        var destinationBranchId = await ValidateWarehousesAsync(request.SourceWarehouseId, request.DestinationWarehouseId);

        if (request.Lines.Count == 0)
        {
            throw new BadRequestException(ResponseMessage.StockTransferRequiresAtLeastOneLine);
        }

        var header = new StockTransferHeader
        {
            SourceWarehouseId = request.SourceWarehouseId,
            DestinationWarehouseId = request.DestinationWarehouseId,
            DestinationBranchId = destinationBranchId,
            Date = request.Date,
            Narration = request.Narration,
            Status = StockTransferStatus.Draft,
            CreatedBy = username,
        };

        foreach (var lineRequest in request.Lines)
        {
            header.Lines.Add(await BuildLineAsync(lineRequest));
        }

        header.TransferNo = await GenerateTransferNoAsync(request.Date);

        _context.StockTransferHeaders.Add(header);
        await _context.SaveChangesAsync();

        var response = await GetByIdAsync(header.Id);
        await _auditLogger.LogAsync("StockTransferHeader", header.Id, "Create", newValue: response);

        return response;
    }

    public async Task SubmitAsync(int id)
    {
        var header = await _context.StockTransferHeaders.FindAsync(id) ?? throw new NotFoundException(ResponseMessage.StockTransferNotFound);

        if (header.Status != StockTransferStatus.Draft)
        {
            throw new BadRequestException(ResponseMessage.StockTransferNotDraftForSubmit);
        }

        header.Status = StockTransferStatus.PendingApproval;
        await _context.SaveChangesAsync();
        await _auditLogger.LogAsync("StockTransferHeader", header.Id, "Submit");
    }

    public async Task ApproveAsync(int id, string username)
    {
        var header = await _context.StockTransferHeaders.Include(t => t.Lines).FirstOrDefaultAsync(t => t.Id == id)
            ?? throw new NotFoundException(ResponseMessage.StockTransferNotFound);

        if (header.Status != StockTransferStatus.PendingApproval)
        {
            throw new BadRequestException(ResponseMessage.StockTransferNotPendingForApprove);
        }

        if (await _fiscalPeriodGuard.IsDateInClosedPeriodAsync(header.Date))
        {
            throw new BadRequestException(ResponseMessage.StockTransferDateInClosedPeriod);
        }

        await using var transaction = await _context.Database.BeginTransactionAsync();

        var isSameBranch = header.DestinationBranchId == header.BranchId;

        foreach (var line in header.Lines)
        {
            var balance = await _context.StockBalances
                .FirstOrDefaultAsync(b => b.ProductVariantId == line.ProductVariantId && b.WarehouseId == header.SourceWarehouseId);
            line.UnitCostAtTransfer = balance?.AverageCost ?? 0;

            await _stockMovementService.IssueAsync(
                line.ProductVariantId, header.SourceWarehouseId, line.BaseQty, header.Date,
                MovementType.TransferOut, SourceDocumentType.StockTransfer, header.Id, line.Id,
                $"Stock Transfer {header.TransferNo}", username);

            if (isSameBranch)
            {
                await _stockMovementService.ReceiveAsync(
                    line.ProductVariantId, header.DestinationWarehouseId, line.BaseQty, line.UnitCostAtTransfer, header.Date,
                    MovementType.TransferIn, SourceDocumentType.StockTransfer, header.Id, line.Id,
                    $"Stock Transfer {header.TransferNo}", username);

                line.ReceivedBaseQty = line.BaseQty;
            }
        }

        header.ApprovedBy = username;
        header.ApprovedAtUtc = DateTime.UtcNow;

        if (isSameBranch)
        {
            header.Status = StockTransferStatus.Completed;
            header.ReceivedBy = username;
            header.ReceivedAtUtc = DateTime.UtcNow;
        }
        else
        {
            header.Status = StockTransferStatus.PendingReceipt;
        }

        await _context.SaveChangesAsync();
        await transaction.CommitAsync();

        await _auditLogger.LogAsync("StockTransferHeader", header.Id, "Approve");
    }

    public async Task RejectAsync(int id)
    {
        var header = await _context.StockTransferHeaders.FindAsync(id) ?? throw new NotFoundException(ResponseMessage.StockTransferNotFound);

        if (header.Status != StockTransferStatus.PendingApproval)
        {
            throw new BadRequestException(ResponseMessage.StockTransferNotPendingForReject);
        }

        header.Status = StockTransferStatus.Rejected;
        await _context.SaveChangesAsync();
        await _auditLogger.LogAsync("StockTransferHeader", header.Id, "Reject");
    }

    public async Task<StockTransferResponse> ReceiveAsync(int id, StockTransferReceiveRequest request, string username)
    {
        // The header's BranchId is the SOURCE branch, but confirming receipt happens under the
        // DESTINATION branch's session context — the standard branch-scoped query filter would hide
        // this row entirely, so this lookup deliberately bypasses it and re-applies only the tenant
        // half of the scoping rule by hand.
        var header = await _context.StockTransferHeaders.IgnoreQueryFilters()
            .Include(t => t.Lines)
            .FirstOrDefaultAsync(t => t.Id == id && t.TenantId == _tenantContext.TenantId)
            ?? throw new NotFoundException(ResponseMessage.StockTransferNotFound);

        if (header.Status != StockTransferStatus.PendingReceipt)
        {
            throw new BadRequestException(ResponseMessage.StockTransferNotPendingReceipt);
        }

        await using var transaction = await _context.Database.BeginTransactionAsync();

        foreach (var receiveLine in request.Lines)
        {
            var line = header.Lines.FirstOrDefault(l => l.Id == receiveLine.LineId)
                ?? throw new BadRequestException(ResponseMessage.StockTransferNotFound);

            if (line.ReceivedBaseQty + receiveLine.ReceivedBaseQty > line.BaseQty)
            {
                throw new BadRequestException(ResponseMessage.StockTransferReceivedQtyExceedsTransferred);
            }

            if (receiveLine.ReceivedBaseQty <= 0)
            {
                continue;
            }

            await _stockMovementService.ReceiveAsync(
                line.ProductVariantId, header.DestinationWarehouseId, receiveLine.ReceivedBaseQty, line.UnitCostAtTransfer, header.Date,
                MovementType.TransferIn, SourceDocumentType.StockTransfer, header.Id, line.Id,
                $"Stock Transfer {header.TransferNo}", username);

            line.ReceivedBaseQty += receiveLine.ReceivedBaseQty;
        }

        if (header.Lines.All(l => l.ReceivedBaseQty >= l.BaseQty))
        {
            header.Status = StockTransferStatus.Completed;
            header.ReceivedBy = username;
            header.ReceivedAtUtc = DateTime.UtcNow;
        }

        await _context.SaveChangesAsync();
        await transaction.CommitAsync();

        await _auditLogger.LogAsync("StockTransferHeader", header.Id, "Receive");

        return await GetByIdAsync(header.Id);
    }

    private async Task<int> ValidateWarehousesAsync(int sourceWarehouseId, int destinationWarehouseId)
    {
        if (sourceWarehouseId == destinationWarehouseId)
        {
            throw new BadRequestException(ResponseMessage.StockTransferSameWarehouse);
        }

        var source = await _context.Warehouses.FindAsync(sourceWarehouseId) ?? throw new BadRequestException(ResponseMessage.StockMovementWarehouseNotFound);
        var destination = await _context.Warehouses.FindAsync(destinationWarehouseId) ?? throw new BadRequestException(ResponseMessage.StockMovementWarehouseNotFound);

        return destination.BranchId;
    }

    private async Task<StockTransferLine> BuildLineAsync(StockTransferLineRequest lineRequest)
    {
        var variant = await _context.ProductVariants.Include(v => v.Product)
            .FirstOrDefaultAsync(v => v.Id == lineRequest.ProductVariantId)
            ?? throw new BadRequestException(ResponseMessage.StockMovementVariantNotFound);

        if (await _context.UnitsOfMeasure.FindAsync(lineRequest.UnitOfMeasureId) is null)
        {
            throw new BadRequestException(ResponseMessage.InvoiceLineUnitOfMeasureNotFound, lineRequest.UnitOfMeasureId);
        }

        if (lineRequest.Qty <= 0)
        {
            throw new BadRequestException(ResponseMessage.InvoiceLineQuantityInvalid);
        }

        decimal baseQty;
        if (lineRequest.UnitOfMeasureId == variant.Product!.BaseUnitOfMeasureId)
        {
            baseQty = lineRequest.Qty;
        }
        else
        {
            var conversion = await _context.UOMConversions
                .FirstOrDefaultAsync(c => c.ProductId == variant.ProductId && c.UnitOfMeasureId == lineRequest.UnitOfMeasureId)
                ?? throw new BadRequestException(ResponseMessage.UOMConversionNotFound);

            baseQty = lineRequest.Qty * conversion.ConversionFactor;
        }

        return new StockTransferLine
        {
            ProductVariantId = lineRequest.ProductVariantId,
            UnitOfMeasureId = lineRequest.UnitOfMeasureId,
            Qty = lineRequest.Qty,
            BaseQty = baseQty,
        };
    }

    private async Task<string> GenerateTransferNoAsync(DateTime date)
    {
        var year = date.Year;
        var count = await _context.StockTransferHeaders.CountAsync(t => t.Date.Year == year);
        return StockDocumentNumberGenerator.Generate("TRF", year, count);
    }

    private async Task<StockTransferHeader?> LoadFullAsync(int id) =>
        await _context.StockTransferHeaders
            .Include(t => t.SourceWarehouse)
            .Include(t => t.DestinationWarehouse)
            .Include(t => t.Lines).ThenInclude(l => l.ProductVariant).ThenInclude(v => v!.Product)
            .Include(t => t.Lines).ThenInclude(l => l.UnitOfMeasure)
            .FirstOrDefaultAsync(t => t.Id == id);

    private static StockTransferResponse ToResponse(StockTransferHeader header) => new()
    {
        Id = header.Id,
        TransferNo = header.TransferNo,
        SourceWarehouseId = header.SourceWarehouseId,
        SourceWarehouseName = header.SourceWarehouse?.Name ?? string.Empty,
        DestinationWarehouseId = header.DestinationWarehouseId,
        DestinationWarehouseName = header.DestinationWarehouse?.Name ?? string.Empty,
        DestinationBranchId = header.DestinationBranchId,
        Date = header.Date,
        Status = header.Status,
        Narration = header.Narration,
        CreatedBy = header.CreatedBy,
        CreatedAtUtc = header.CreatedAtUtc,
        ApprovedBy = header.ApprovedBy,
        ApprovedAtUtc = header.ApprovedAtUtc,
        ReceivedBy = header.ReceivedBy,
        ReceivedAtUtc = header.ReceivedAtUtc,
        Lines = header.Lines.Select(l => new StockTransferLineResponse
        {
            Id = l.Id,
            ProductVariantId = l.ProductVariantId,
            ProductVariantName = l.ProductVariant?.Name ?? string.Empty,
            ProductName = l.ProductVariant?.Product?.Name ?? string.Empty,
            UnitOfMeasureId = l.UnitOfMeasureId,
            UnitOfMeasureCode = l.UnitOfMeasure?.Code ?? string.Empty,
            Qty = l.Qty,
            BaseQty = l.BaseQty,
            UnitCostAtTransfer = l.UnitCostAtTransfer,
            ReceivedBaseQty = l.ReceivedBaseQty,
        }).ToList(),
    };
}
