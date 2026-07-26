using erp_backend.Invoices.Dtos;
using erp_backend.Models;

namespace erp_backend.Invoices;

/// <summary>
/// The single shared engine handling all 6 InvoiceType values (PurchaseOrder/PurchaseInvoice/
/// PurchaseReturn/SalesOrder/SalesInvoice/SaleReturn) — state machine, costing, GL posting, and
/// order-fulfillment tracking all branch internally on InvoiceType.
/// </summary>
public interface IInvoiceRepository
{
    Task<PagedResult<InvoiceListItemResponse>> GetAllAsync(
        InvoiceType? invoiceType, int? partnerId, InvoiceStatus? status, DateTime? from, DateTime? to,
        int pageNumber, int pageSize);

    Task<InvoiceResponse> GetByIdAsync(int id);
    Task<InvoiceType> GetInvoiceTypeAsync(int id);
    Task<InvoiceResponse> CreateAsync(InvoiceRequest request, string username);
    Task<InvoiceResponse> UpdateAsync(int id, InvoiceRequest request);
    Task SubmitAsync(int id);
    Task ApproveAsync(int id, string username);
    Task RejectAsync(int id);
    Task CancelAsync(int id);
}
