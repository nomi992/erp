using erp_backend.Accounting;
using erp_backend.Auditing;
using erp_backend.Data;
using erp_backend.Exceptions;
using erp_backend.Inventory;
using erp_backend.Invoices.Dtos;
using erp_backend.Messages;
using erp_backend.Models;
using erp_backend.Vouchers;
using Microsoft.EntityFrameworkCore;

namespace erp_backend.Invoices;

public class InvoiceRepository : IInvoiceRepository
{
    private static readonly Dictionary<InvoiceType, string> Prefixes = new()
    {
        [InvoiceType.PurchaseOrder] = "PO",
        [InvoiceType.PurchaseInvoice] = "PINV",
        [InvoiceType.PurchaseReturn] = "PRET",
        [InvoiceType.SalesOrder] = "SO",
        [InvoiceType.SalesInvoice] = "SINV",
        [InvoiceType.SaleReturn] = "SRET",
    };

    private readonly AppDbContext _context;
    private readonly IFiscalPeriodGuard _fiscalPeriodGuard;
    private readonly IStockMovementService _stockMovementService;
    private readonly IStockAccountMappingRepository _stockAccountMappings;
    private readonly IAuditLogger _auditLogger;

    public InvoiceRepository(
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

    public async Task<PagedResult<InvoiceListItemResponse>> GetAllAsync(
        InvoiceType? invoiceType, int? partnerId, InvoiceStatus? status, DateTime? from, DateTime? to,
        string? search, string? sortBy, string? sortDirection,
        int pageNumber, int pageSize)
    {
        var query = _context.InvoiceHeaders.Include(h => h.Partner).AsQueryable();

        if (invoiceType is not null) query = query.Where(h => h.InvoiceType == invoiceType);
        if (partnerId is not null) query = query.Where(h => h.PartnerId == partnerId);
        if (status is not null) query = query.Where(h => h.Status == status);
        if (from is not null) query = query.Where(h => h.Date >= from);
        if (to is not null) query = query.Where(h => h.Date <= to);

        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(h => h.InvoiceNo.Contains(search) || (h.Partner != null && h.Partner.Name.Contains(search)));
        }

        var direction = string.Equals(sortDirection, "asc", StringComparison.OrdinalIgnoreCase) ? "asc" : "desc";
        query = (sortBy?.ToLowerInvariant(), direction) switch
        {
            ("invoiceno", "asc") => query.OrderBy(h => h.InvoiceNo),
            ("invoiceno", _) => query.OrderByDescending(h => h.InvoiceNo),
            ("partnername", "asc") => query.OrderBy(h => h.Partner!.Name),
            ("partnername", _) => query.OrderByDescending(h => h.Partner!.Name),
            ("date", "asc") => query.OrderBy(h => h.Date),
            ("date", _) => query.OrderByDescending(h => h.Date),
            ("duedate", "asc") => query.OrderBy(h => h.DueDate),
            ("duedate", _) => query.OrderByDescending(h => h.DueDate),
            ("outstandingamount", "asc") => query.OrderBy(h => h.OutstandingAmount),
            ("outstandingamount", _) => query.OrderByDescending(h => h.OutstandingAmount),
            ("status", "asc") => query.OrderBy(h => h.Status),
            ("status", _) => query.OrderByDescending(h => h.Status),
            _ => query.OrderByDescending(h => h.Date).ThenByDescending(h => h.Id),
        };

        var totalCount = await query.CountAsync();
        var items = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Select(h => new InvoiceListItemResponse
            {
                Id = h.Id,
                InvoiceType = h.InvoiceType,
                InvoiceNo = h.InvoiceNo,
                PartnerId = h.PartnerId,
                PartnerName = h.Partner!.Name,
                Date = h.Date,
                DueDate = h.DueDate,
                OutstandingAmount = h.OutstandingAmount,
                PaymentStatus = h.PaymentStatus,
                FulfillmentStatus = h.FulfillmentStatus,
                Status = h.Status,
                TotalAmount = h.Lines.Sum(l => l.LineTotal),
            })
            .AsNoTracking()
            .ToListAsync();

        return new PagedResult<InvoiceListItemResponse> { Items = items, TotalCount = totalCount, PageNumber = pageNumber, PageSize = pageSize };
    }

    public async Task<InvoiceResponse> GetByIdAsync(int id)
    {
        var header = await LoadFullAsync(id) ?? throw new NotFoundException(ResponseMessage.InvoiceNotFound);
        return ToResponse(header);
    }

    public async Task<InvoiceType> GetInvoiceTypeAsync(int id)
    {
        var header = await _context.InvoiceHeaders.FindAsync(id) ?? throw new NotFoundException(ResponseMessage.InvoiceNotFound);
        return header.InvoiceType;
    }

    public async Task<InvoiceResponse> CreateAsync(InvoiceRequest request, string username)
    {
        var partner = await _context.BusinessPartners.FindAsync(request.PartnerId) ?? throw new BadRequestException(ResponseMessage.InvoicePartnerNotFound);
        ValidatePartnerCompatibility(request.InvoiceType, partner.PartnerType);

        if (await _context.Warehouses.FindAsync(request.WarehouseId) is null)
        {
            throw new BadRequestException(ResponseMessage.InvoiceWarehouseNotFound);
        }

        if (request.Lines.Count == 0)
        {
            throw new BadRequestException(ResponseMessage.InvoiceRequiresAtLeastOneLine);
        }

        var reference = await ValidateAndLoadReferenceAsync(request);

        var header = new InvoiceHeader
        {
            InvoiceType = request.InvoiceType,
            ExternalReferenceNo = request.ExternalReferenceNo,
            PartnerId = request.PartnerId,
            ReferenceInvoiceId = IsOrderType(request.InvoiceType) ? null : request.ReferenceInvoiceId,
            WarehouseId = request.WarehouseId,
            Date = request.Date,
            PaymentMode = request.PaymentMode,
            PaymentTermDays = request.PaymentTermDays,
            Narration = request.Narration,
            Status = InvoiceStatus.Draft,
            CreatedBy = username,
        };

        foreach (var lineRequest in request.Lines)
        {
            header.Lines.Add(await BuildLineAsync(lineRequest, reference));
        }

        ApplyComputedHeaderFields(header, request);
        header.InvoiceNo = await GenerateInvoiceNoAsync(request.InvoiceType, request.Date);

        _context.InvoiceHeaders.Add(header);
        await _context.SaveChangesAsync();

        var response = await GetByIdAsync(header.Id);
        await _auditLogger.LogAsync("InvoiceHeader", header.Id, "Create", newValue: response);

        return response;
    }

    public async Task<InvoiceResponse> UpdateAsync(int id, InvoiceRequest request)
    {
        var header = await _context.InvoiceHeaders.Include(h => h.Lines).FirstOrDefaultAsync(h => h.Id == id)
            ?? throw new NotFoundException(ResponseMessage.InvoiceNotFound);

        if (header.Status != InvoiceStatus.Draft)
        {
            throw new BadRequestException(ResponseMessage.InvoiceNotDraftForEdit);
        }

        var partner = await _context.BusinessPartners.FindAsync(request.PartnerId) ?? throw new BadRequestException(ResponseMessage.InvoicePartnerNotFound);
        ValidatePartnerCompatibility(header.InvoiceType, partner.PartnerType);

        if (await _context.Warehouses.FindAsync(request.WarehouseId) is null)
        {
            throw new BadRequestException(ResponseMessage.InvoiceWarehouseNotFound);
        }

        if (request.Lines.Count == 0)
        {
            throw new BadRequestException(ResponseMessage.InvoiceRequiresAtLeastOneLine);
        }

        // InvoiceType is immutable after creation — always resolve/validate references against the
        // persisted type, never whatever the client happened to send.
        request.InvoiceType = header.InvoiceType;
        var reference = await ValidateAndLoadReferenceAsync(request);

        header.ExternalReferenceNo = request.ExternalReferenceNo;
        header.PartnerId = request.PartnerId;
        header.ReferenceInvoiceId = IsOrderType(header.InvoiceType) ? null : request.ReferenceInvoiceId;
        header.WarehouseId = request.WarehouseId;
        header.Date = request.Date;
        header.PaymentMode = request.PaymentMode;
        header.PaymentTermDays = request.PaymentTermDays;
        header.Narration = request.Narration;

        _context.InvoiceLines.RemoveRange(header.Lines);
        header.Lines.Clear();

        foreach (var lineRequest in request.Lines)
        {
            header.Lines.Add(await BuildLineAsync(lineRequest, reference));
        }

        ApplyComputedHeaderFields(header, request);

        await _context.SaveChangesAsync();

        return await GetByIdAsync(id);
    }

    public async Task SubmitAsync(int id)
    {
        var header = await _context.InvoiceHeaders.FindAsync(id) ?? throw new NotFoundException(ResponseMessage.InvoiceNotFound);

        if (header.Status != InvoiceStatus.Draft)
        {
            throw new BadRequestException(ResponseMessage.InvoiceNotDraftForSubmit);
        }

        header.Status = InvoiceStatus.PendingApproval;
        await _context.SaveChangesAsync();
        await _auditLogger.LogAsync("InvoiceHeader", header.Id, "Submit");
    }

    public async Task ApproveAsync(int id, string username)
    {
        var header = await _context.InvoiceHeaders
            .Include(h => h.Lines).ThenInclude(l => l.ProductVariant).ThenInclude(v => v!.Product)
            .Include(h => h.ReferenceInvoice).ThenInclude(r => r!.Lines)
            .FirstOrDefaultAsync(h => h.Id == id)
            ?? throw new NotFoundException(ResponseMessage.InvoiceNotFound);

        if (header.Status != InvoiceStatus.PendingApproval)
        {
            throw new BadRequestException(ResponseMessage.InvoiceNotPendingForApprove);
        }

        if (await _fiscalPeriodGuard.IsDateInClosedPeriodAsync(header.Date))
        {
            throw new BadRequestException(ResponseMessage.InvoiceDateInClosedPeriodForApproval);
        }

        if (IsOrderType(header.InvoiceType))
        {
            header.Status = InvoiceStatus.Posted;
            header.FulfillmentStatus = FulfillmentStatus.Open;
            header.ApprovedBy = username;
            header.ApprovedAtUtc = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            await _auditLogger.LogAsync("InvoiceHeader", header.Id, "Approve");
            return;
        }

        await using var transaction = await _context.Database.BeginTransactionAsync();

        var controlMapping = await _stockAccountMappings.ResolveForCategoryAsync(null);

        var voucher = new VoucherHeader
        {
            VoucherType = header.InvoiceType switch
            {
                InvoiceType.PurchaseInvoice => VoucherType.Purchase,
                InvoiceType.PurchaseReturn => VoucherType.DebitNote,
                InvoiceType.SalesInvoice => VoucherType.Sales,
                InvoiceType.SaleReturn => VoucherType.CreditNote,
                _ => VoucherType.Journal,
            },
            Date = header.Date,
            CreatedBy = username,
            Status = VoucherStatus.Posted,
            ApprovedBy = username,
            ApprovedAtUtc = DateTime.UtcNow,
        };

        switch (header.InvoiceType)
        {
            case InvoiceType.PurchaseInvoice:
                await PostPurchaseInvoiceAsync(header, voucher, controlMapping, username);
                break;
            case InvoiceType.PurchaseReturn:
                await PostPurchaseReturnAsync(header, voucher, controlMapping, username);
                break;
            case InvoiceType.SalesInvoice:
                await PostSalesInvoiceAsync(header, voucher, controlMapping, username);
                break;
            case InvoiceType.SaleReturn:
                await PostSaleReturnAsync(header, voucher, controlMapping, username);
                break;
        }

        var total = header.Lines.Sum(l => l.LineTotal);
        header.DueDate = header.Date.AddDays(header.PaymentTermDays);

        if (header.PaymentMode == PaymentMode.Cash)
        {
            header.AmountPaid = total;
            header.OutstandingAmount = 0;
            header.PaymentStatus = InvoicePaymentStatus.Paid;
        }
        else
        {
            header.AmountPaid = 0;
            header.OutstandingAmount = total;
            header.PaymentStatus = InvoicePaymentStatus.Unpaid;
        }

        header.Status = InvoiceStatus.Posted;
        header.ApprovedBy = username;
        header.ApprovedAtUtc = DateTime.UtcNow;

        voucher.VoucherNo = await VoucherNumberGenerator.GenerateAsync(_context, voucher.VoucherType, header.Date);
        voucher.Narration = $"Auto-posted from {header.InvoiceType} {header.InvoiceNo}";

        _context.VoucherHeaders.Add(voucher);
        await _context.SaveChangesAsync();

        header.LinkedVoucherId = voucher.Id;

        if (header.ReferenceInvoiceId is not null && header.ReferenceInvoice is not null && IsOrderType(header.ReferenceInvoice.InvoiceType))
        {
            UpdateOrderFulfillment(header.ReferenceInvoice, header);
        }

        await _context.SaveChangesAsync();
        await transaction.CommitAsync();

        await _auditLogger.LogAsync("InvoiceHeader", header.Id, "Approve", newValue: new { header.LinkedVoucherId });
    }

    public async Task RejectAsync(int id)
    {
        var header = await _context.InvoiceHeaders.FindAsync(id) ?? throw new NotFoundException(ResponseMessage.InvoiceNotFound);

        if (header.Status != InvoiceStatus.PendingApproval)
        {
            throw new BadRequestException(ResponseMessage.InvoiceNotPendingForReject);
        }

        header.Status = InvoiceStatus.Rejected;
        await _context.SaveChangesAsync();
        await _auditLogger.LogAsync("InvoiceHeader", header.Id, "Reject");
    }

    public async Task CancelAsync(int id)
    {
        var header = await _context.InvoiceHeaders.FindAsync(id) ?? throw new NotFoundException(ResponseMessage.InvoiceNotFound);

        if (header.Status == InvoiceStatus.Posted)
        {
            if (!IsOrderType(header.InvoiceType))
            {
                throw new BadRequestException(ResponseMessage.InvoiceNotCancellable);
            }

            if (header.FulfillmentStatus == FulfillmentStatus.Fulfilled)
            {
                throw new BadRequestException(ResponseMessage.InvoiceOrderFullyFulfilledCannotCancel);
            }

            header.FulfillmentStatus = FulfillmentStatus.Cancelled;
        }
        else if (header.Status is not (InvoiceStatus.Draft or InvoiceStatus.PendingApproval))
        {
            throw new BadRequestException(ResponseMessage.InvoiceNotCancellable);
        }

        header.Status = InvoiceStatus.Cancelled;
        await _context.SaveChangesAsync();
        await _auditLogger.LogAsync("InvoiceHeader", header.Id, "Cancel");
    }

    // --- GL posting per InvoiceType (only called for the 4 stock/GL-affecting types) ---

    private async Task PostPurchaseInvoiceAsync(InvoiceHeader header, VoucherHeader voucher, StockAccountMapping controlMapping, string username)
    {
        var inventoryDebits = new Dictionary<int, decimal>();
        decimal totalTax = 0;

        foreach (var line in header.Lines)
        {
            var lineMapping = await _stockAccountMappings.ResolveForCategoryAsync(line.ProductVariant!.Product!.ProductCategoryId);
            var netAmount = Math.Round(line.Qty * line.UnitAmount, 2);
            var unitCostPerBase = line.BaseQty > 0 ? netAmount / line.BaseQty : 0;

            await _stockMovementService.ReceiveAsync(
                line.ProductVariantId, header.WarehouseId, line.BaseQty, unitCostPerBase, header.Date,
                MovementType.PurchaseReceipt, SourceDocumentType.Invoice, header.Id, line.Id,
                $"Purchase Invoice {header.InvoiceNo}", username);

            inventoryDebits[lineMapping.InventoryAssetAccountId] = inventoryDebits.GetValueOrDefault(lineMapping.InventoryAssetAccountId) + netAmount;
            totalTax += line.TaxAmount;
        }

        foreach (var (accountId, amount) in inventoryDebits)
        {
            voucher.Details.Add(new VoucherDetail { AccountId = accountId, DebitAmount = amount });
        }

        if (totalTax > 0)
        {
            var inputTaxAccountId = controlMapping.InputTaxAccountId ?? throw new BadRequestException(ResponseMessage.InvoiceNoStockAccountMapping);
            voucher.Details.Add(new VoucherDetail { AccountId = inputTaxAccountId, DebitAmount = totalTax });
        }

        var total = header.Lines.Sum(l => l.LineTotal);
        var creditAccountId = header.PaymentMode == PaymentMode.Cash
            ? controlMapping.CashOrBankAccountId ?? throw new BadRequestException(ResponseMessage.InvoiceNoStockAccountMapping)
            : controlMapping.AccountsPayableAccountId;

        voucher.Details.Add(new VoucherDetail { AccountId = creditAccountId, CreditAmount = total });
    }

    private async Task PostPurchaseReturnAsync(InvoiceHeader header, VoucherHeader voucher, StockAccountMapping controlMapping, string username)
    {
        var inventoryCredits = new Dictionary<int, decimal>();
        decimal totalTax = 0;

        foreach (var line in header.Lines)
        {
            var lineMapping = await _stockAccountMappings.ResolveForCategoryAsync(line.ProductVariant!.Product!.ProductCategoryId);
            var netAmount = Math.Round(line.Qty * line.UnitAmount, 2);

            await _stockMovementService.IssueAsync(
                line.ProductVariantId, header.WarehouseId, line.BaseQty, header.Date,
                MovementType.PurchaseReturnIssue, SourceDocumentType.Invoice, header.Id, line.Id,
                $"Purchase Return {header.InvoiceNo}", username);

            inventoryCredits[lineMapping.InventoryAssetAccountId] = inventoryCredits.GetValueOrDefault(lineMapping.InventoryAssetAccountId) + netAmount;
            totalTax += line.TaxAmount;
        }

        var total = header.Lines.Sum(l => l.LineTotal);
        var debitAccountId = header.PaymentMode == PaymentMode.Cash
            ? controlMapping.CashOrBankAccountId ?? throw new BadRequestException(ResponseMessage.InvoiceNoStockAccountMapping)
            : controlMapping.AccountsPayableAccountId;

        voucher.Details.Add(new VoucherDetail { AccountId = debitAccountId, DebitAmount = total });

        foreach (var (accountId, amount) in inventoryCredits)
        {
            voucher.Details.Add(new VoucherDetail { AccountId = accountId, CreditAmount = amount });
        }

        if (totalTax > 0)
        {
            var inputTaxAccountId = controlMapping.InputTaxAccountId ?? throw new BadRequestException(ResponseMessage.InvoiceNoStockAccountMapping);
            voucher.Details.Add(new VoucherDetail { AccountId = inputTaxAccountId, CreditAmount = totalTax });
        }
    }

    private async Task PostSalesInvoiceAsync(InvoiceHeader header, VoucherHeader voucher, StockAccountMapping controlMapping, string username)
    {
        var cogsDebits = new Dictionary<int, decimal>();
        var inventoryCredits = new Dictionary<int, decimal>();
        decimal totalTax = 0;
        decimal totalRevenue = 0;

        foreach (var line in header.Lines)
        {
            var lineMapping = await _stockAccountMappings.ResolveForCategoryAsync(line.ProductVariant!.Product!.ProductCategoryId);
            var netAmount = Math.Round(line.Qty * line.UnitAmount, 2);

            var result = await _stockMovementService.IssueAsync(
                line.ProductVariantId, header.WarehouseId, line.BaseQty, header.Date,
                MovementType.SaleIssue, SourceDocumentType.Invoice, header.Id, line.Id,
                $"Sales Invoice {header.InvoiceNo}", username);

            line.UnitCostAtSale = result.NewAverageCost;
            var cogsAmount = Math.Round(line.BaseQty * result.NewAverageCost, 2);

            cogsDebits[lineMapping.COGSAccountId] = cogsDebits.GetValueOrDefault(lineMapping.COGSAccountId) + cogsAmount;
            inventoryCredits[lineMapping.InventoryAssetAccountId] = inventoryCredits.GetValueOrDefault(lineMapping.InventoryAssetAccountId) + cogsAmount;

            totalRevenue += netAmount;
            totalTax += line.TaxAmount;
        }

        var debitAccountId = header.PaymentMode == PaymentMode.Cash
            ? controlMapping.CashOrBankAccountId ?? throw new BadRequestException(ResponseMessage.InvoiceNoStockAccountMapping)
            : controlMapping.AccountsReceivableAccountId;

        voucher.Details.Add(new VoucherDetail { AccountId = debitAccountId, DebitAmount = totalRevenue + totalTax });
        voucher.Details.Add(new VoucherDetail { AccountId = controlMapping.SalesRevenueAccountId, CreditAmount = totalRevenue });

        if (totalTax > 0)
        {
            var outputTaxAccountId = controlMapping.OutputTaxAccountId ?? throw new BadRequestException(ResponseMessage.InvoiceNoStockAccountMapping);
            voucher.Details.Add(new VoucherDetail { AccountId = outputTaxAccountId, CreditAmount = totalTax });
        }

        foreach (var (accountId, amount) in cogsDebits)
        {
            voucher.Details.Add(new VoucherDetail { AccountId = accountId, DebitAmount = amount });
        }

        foreach (var (accountId, amount) in inventoryCredits)
        {
            voucher.Details.Add(new VoucherDetail { AccountId = accountId, CreditAmount = amount });
        }
    }

    private async Task PostSaleReturnAsync(InvoiceHeader header, VoucherHeader voucher, StockAccountMapping controlMapping, string username)
    {
        var cogsCredits = new Dictionary<int, decimal>();
        var inventoryDebits = new Dictionary<int, decimal>();
        decimal totalTax = 0;
        decimal totalRevenue = 0;

        foreach (var line in header.Lines)
        {
            var lineMapping = await _stockAccountMappings.ResolveForCategoryAsync(line.ProductVariant!.Product!.ProductCategoryId);
            var netAmount = Math.Round(line.Qty * line.UnitAmount, 2);

            var originalLine = line.ReferenceInvoiceLineId is int refLineId
                ? await _context.InvoiceLines.FindAsync(refLineId)
                : null;
            var unitCostAtSale = originalLine?.UnitCostAtSale ?? 0;
            line.UnitCostAtSale = unitCostAtSale;

            await _stockMovementService.ReceiveAsync(
                line.ProductVariantId, header.WarehouseId, line.BaseQty, unitCostAtSale, header.Date,
                MovementType.SaleReturnReceipt, SourceDocumentType.Invoice, header.Id, line.Id,
                $"Sale Return {header.InvoiceNo}", username);

            var cogsAmount = Math.Round(line.BaseQty * unitCostAtSale, 2);
            cogsCredits[lineMapping.COGSAccountId] = cogsCredits.GetValueOrDefault(lineMapping.COGSAccountId) + cogsAmount;
            inventoryDebits[lineMapping.InventoryAssetAccountId] = inventoryDebits.GetValueOrDefault(lineMapping.InventoryAssetAccountId) + cogsAmount;

            totalRevenue += netAmount;
            totalTax += line.TaxAmount;
        }

        voucher.Details.Add(new VoucherDetail { AccountId = controlMapping.SalesRevenueAccountId, DebitAmount = totalRevenue });

        if (totalTax > 0)
        {
            var outputTaxAccountId = controlMapping.OutputTaxAccountId ?? throw new BadRequestException(ResponseMessage.InvoiceNoStockAccountMapping);
            voucher.Details.Add(new VoucherDetail { AccountId = outputTaxAccountId, DebitAmount = totalTax });
        }

        var creditAccountId = header.PaymentMode == PaymentMode.Cash
            ? controlMapping.CashOrBankAccountId ?? throw new BadRequestException(ResponseMessage.InvoiceNoStockAccountMapping)
            : controlMapping.AccountsReceivableAccountId;

        voucher.Details.Add(new VoucherDetail { AccountId = creditAccountId, CreditAmount = totalRevenue + totalTax });

        foreach (var (accountId, amount) in inventoryDebits)
        {
            voucher.Details.Add(new VoucherDetail { AccountId = accountId, DebitAmount = amount });
        }

        foreach (var (accountId, amount) in cogsCredits)
        {
            voucher.Details.Add(new VoucherDetail { AccountId = accountId, CreditAmount = amount });
        }
    }

    private static void UpdateOrderFulfillment(InvoiceHeader order, InvoiceHeader postedDocument)
    {
        foreach (var line in postedDocument.Lines)
        {
            if (line.ReferenceInvoiceLineId is not int refLineId)
            {
                continue;
            }

            var orderLine = order.Lines.FirstOrDefault(l => l.Id == refLineId);
            if (orderLine is not null)
            {
                orderLine.FulfilledBaseQty += line.BaseQty;
            }
        }

        if (order.Lines.Count > 0 && order.Lines.All(l => l.FulfilledBaseQty >= l.BaseQty))
        {
            order.FulfillmentStatus = FulfillmentStatus.Fulfilled;
        }
        else if (order.Lines.Any(l => l.FulfilledBaseQty > 0))
        {
            order.FulfillmentStatus = FulfillmentStatus.PartiallyFulfilled;
        }
    }

    // --- Shared validation / mapping helpers ---

    private async Task<InvoiceHeader?> ValidateAndLoadReferenceAsync(InvoiceRequest request)
    {
        if (IsOrderType(request.InvoiceType))
        {
            return null;
        }

        if (IsReturnType(request.InvoiceType) && request.ReferenceInvoiceId is null)
        {
            throw new BadRequestException(ResponseMessage.InvoiceReturnRequiresReference);
        }

        if (request.ReferenceInvoiceId is null)
        {
            return null;
        }

        var reference = await _context.InvoiceHeaders.Include(h => h.Lines)
            .FirstOrDefaultAsync(h => h.Id == request.ReferenceInvoiceId)
            ?? throw new BadRequestException(ResponseMessage.InvoiceReferenceNotFound);

        if (reference.Status != InvoiceStatus.Posted)
        {
            throw new BadRequestException(ResponseMessage.InvoiceReferenceNotPosted);
        }

        var expectedType = ExpectedReferenceType(request.InvoiceType);
        if (expectedType is null || reference.InvoiceType != expectedType)
        {
            throw new BadRequestException(ResponseMessage.InvoiceReferenceTypeMismatch);
        }

        return reference;
    }

    private async Task<InvoiceLine> BuildLineAsync(InvoiceLineRequest lineRequest, InvoiceHeader? reference)
    {
        var variant = await _context.ProductVariants.Include(v => v.Product)
            .FirstOrDefaultAsync(v => v.Id == lineRequest.ProductVariantId)
            ?? throw new BadRequestException(ResponseMessage.InvoiceLineVariantNotFound, lineRequest.ProductVariantId);

        if (await _context.UnitsOfMeasure.FindAsync(lineRequest.UnitOfMeasureId) is null)
        {
            throw new BadRequestException(ResponseMessage.InvoiceLineUnitOfMeasureNotFound, lineRequest.UnitOfMeasureId);
        }

        if (lineRequest.Qty <= 0)
        {
            throw new BadRequestException(ResponseMessage.InvoiceLineQuantityInvalid);
        }

        var baseQty = await GetBaseQtyAsync(variant.ProductId, variant.Product!.BaseUnitOfMeasureId, lineRequest.UnitOfMeasureId, lineRequest.Qty);

        if (lineRequest.ReferenceInvoiceLineId is int refLineId)
        {
            if (reference is null)
            {
                throw new BadRequestException(ResponseMessage.InvoiceReferenceNotFound);
            }

            var refLine = reference.Lines.FirstOrDefault(l => l.Id == refLineId)
                ?? throw new BadRequestException(ResponseMessage.InvoiceReferenceNotFound);

            if (IsOrderType(reference.InvoiceType) && refLine.FulfilledBaseQty + baseQty > refLine.BaseQty)
            {
                throw new BadRequestException(ResponseMessage.InvoiceReferenceLineOverFulfilled);
            }
        }

        decimal taxAmount = 0;
        if (lineRequest.TaxRateId is int taxRateId)
        {
            var taxRate = await _context.TaxRates.FindAsync(taxRateId) ?? throw new BadRequestException(ResponseMessage.InvoiceLineTaxRateNotFound, taxRateId);
            taxAmount = Math.Round(lineRequest.Qty * lineRequest.UnitAmount * taxRate.Percentage / 100m, 2);
        }

        var lineTotal = Math.Round(lineRequest.Qty * lineRequest.UnitAmount, 2) + taxAmount;

        return new InvoiceLine
        {
            ProductVariantId = lineRequest.ProductVariantId,
            UnitOfMeasureId = lineRequest.UnitOfMeasureId,
            Qty = lineRequest.Qty,
            BaseQty = baseQty,
            UnitAmount = lineRequest.UnitAmount,
            TaxRateId = lineRequest.TaxRateId,
            TaxAmount = taxAmount,
            LineTotal = lineTotal,
            ReferenceInvoiceLineId = lineRequest.ReferenceInvoiceLineId,
        };
    }

    private async Task<decimal> GetBaseQtyAsync(int productId, int baseUnitOfMeasureId, int lineUnitOfMeasureId, decimal qty)
    {
        if (lineUnitOfMeasureId == baseUnitOfMeasureId)
        {
            return qty;
        }

        var conversion = await _context.UOMConversions
            .FirstOrDefaultAsync(c => c.ProductId == productId && c.UnitOfMeasureId == lineUnitOfMeasureId)
            ?? throw new BadRequestException(ResponseMessage.UOMConversionNotFound);

        return qty * conversion.ConversionFactor;
    }

    private static void ApplyComputedHeaderFields(InvoiceHeader header, InvoiceRequest request)
    {
        header.DueDate = IsOrderType(header.InvoiceType)
            ? request.RequestedDeliveryDate ?? header.Date
            : header.Date.AddDays(header.PaymentTermDays);

        header.AmountPaid = 0;
        header.OutstandingAmount = header.Lines.Sum(l => l.LineTotal);
        header.PaymentStatus = InvoicePaymentStatus.Unpaid;
        header.FulfillmentStatus = null;
    }

    private static void ValidatePartnerCompatibility(InvoiceType invoiceType, PartnerType partnerType)
    {
        if (IsPurchaseSide(invoiceType))
        {
            if (partnerType is not (PartnerType.Supplier or PartnerType.Both))
            {
                throw new BadRequestException(ResponseMessage.BusinessPartnerNotASupplier);
            }
        }
        else if (partnerType is not (PartnerType.Customer or PartnerType.Both))
        {
            throw new BadRequestException(ResponseMessage.BusinessPartnerNotACustomer);
        }
    }

    private async Task<string> GenerateInvoiceNoAsync(InvoiceType type, DateTime date)
    {
        var year = date.Year;
        var count = await _context.InvoiceHeaders.CountAsync(h => h.InvoiceType == type && h.Date.Year == year);
        return StockDocumentNumberGenerator.Generate(Prefixes[type], year, count);
    }

    private async Task<InvoiceHeader?> LoadFullAsync(int id) =>
        await _context.InvoiceHeaders
            .Include(h => h.Partner)
            .Include(h => h.Warehouse)
            .Include(h => h.ReferenceInvoice)
            .Include(h => h.LinkedVoucher)
            .Include(h => h.Lines).ThenInclude(l => l.ProductVariant).ThenInclude(v => v!.Product)
            .Include(h => h.Lines).ThenInclude(l => l.UnitOfMeasure)
            .Include(h => h.Lines).ThenInclude(l => l.TaxRate)
            .FirstOrDefaultAsync(h => h.Id == id);

    private static InvoiceResponse ToResponse(InvoiceHeader header)
    {
        var totalNet = header.Lines.Sum(l => Math.Round(l.Qty * l.UnitAmount, 2));
        var totalTax = header.Lines.Sum(l => l.TaxAmount);

        return new InvoiceResponse
        {
            Id = header.Id,
            InvoiceType = header.InvoiceType,
            InvoiceNo = header.InvoiceNo,
            ExternalReferenceNo = header.ExternalReferenceNo,
            PartnerId = header.PartnerId,
            PartnerName = header.Partner?.Name ?? string.Empty,
            ReferenceInvoiceId = header.ReferenceInvoiceId,
            ReferenceInvoiceNo = header.ReferenceInvoice?.InvoiceNo,
            WarehouseId = header.WarehouseId,
            WarehouseName = header.Warehouse?.Name ?? string.Empty,
            Date = header.Date,
            PaymentMode = header.PaymentMode,
            PaymentTermDays = header.PaymentTermDays,
            DueDate = header.DueDate,
            AmountPaid = header.AmountPaid,
            OutstandingAmount = header.OutstandingAmount,
            PaymentStatus = header.PaymentStatus,
            FulfillmentStatus = header.FulfillmentStatus,
            Status = header.Status,
            Narration = header.Narration,
            LinkedVoucherId = header.LinkedVoucherId,
            LinkedVoucherNo = header.LinkedVoucher?.VoucherNo,
            CreatedBy = header.CreatedBy,
            CreatedAtUtc = header.CreatedAtUtc,
            ApprovedBy = header.ApprovedBy,
            ApprovedAtUtc = header.ApprovedAtUtc,
            TotalNet = totalNet,
            TotalTax = totalTax,
            TotalAmount = totalNet + totalTax,
            Lines = header.Lines.Select(l => new InvoiceLineResponse
            {
                Id = l.Id,
                ProductVariantId = l.ProductVariantId,
                ProductVariantName = l.ProductVariant?.Name ?? string.Empty,
                ProductName = l.ProductVariant?.Product?.Name ?? string.Empty,
                UnitOfMeasureId = l.UnitOfMeasureId,
                UnitOfMeasureCode = l.UnitOfMeasure?.Code ?? string.Empty,
                Qty = l.Qty,
                BaseQty = l.BaseQty,
                UnitAmount = l.UnitAmount,
                UnitCostAtSale = l.UnitCostAtSale,
                TaxRateId = l.TaxRateId,
                TaxRateName = l.TaxRate?.Name,
                TaxAmount = l.TaxAmount,
                LineTotal = l.LineTotal,
                FulfilledBaseQty = l.FulfilledBaseQty,
                ReferenceInvoiceLineId = l.ReferenceInvoiceLineId,
            }).ToList(),
        };
    }

    private static bool IsOrderType(InvoiceType type) => type is InvoiceType.PurchaseOrder or InvoiceType.SalesOrder;

    private static bool IsReturnType(InvoiceType type) => type is InvoiceType.PurchaseReturn or InvoiceType.SaleReturn;

    private static bool IsPurchaseSide(InvoiceType type) =>
        type is InvoiceType.PurchaseOrder or InvoiceType.PurchaseInvoice or InvoiceType.PurchaseReturn;

    private static InvoiceType? ExpectedReferenceType(InvoiceType type) => type switch
    {
        InvoiceType.PurchaseInvoice => InvoiceType.PurchaseOrder,
        InvoiceType.SalesInvoice => InvoiceType.SalesOrder,
        InvoiceType.PurchaseReturn => InvoiceType.PurchaseInvoice,
        InvoiceType.SaleReturn => InvoiceType.SalesInvoice,
        _ => null,
    };
}
