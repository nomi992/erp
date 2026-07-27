using erp_backend.Models;
using erp_backend.PartnerPayments.Dtos;

namespace erp_backend.PartnerPayments;

/// <summary>
/// Single shared engine for both payment directions (CustomerReceipt/SupplierPayment). There is no
/// Update/Edit operation — a draft payment that needs changing is rejected/cancelled and re-created,
/// matching the rights list (no *.Edit right exists for either direction).
/// </summary>
public interface IPartnerPaymentRepository
{
    Task<PagedResult<PartnerPaymentListItemResponse>> GetAllAsync(
        PaymentDirection? direction, int? partnerId, PartnerPaymentStatus? status, DateTime? from, DateTime? to,
        string? search, string? sortBy, string? sortDirection,
        int pageNumber, int pageSize);

    Task<PartnerPaymentResponse> GetByIdAsync(int id);
    Task<PaymentDirection> GetDirectionAsync(int id);
    Task<PartnerPaymentResponse> CreateAsync(PartnerPaymentRequest request, string username);
    Task SubmitAsync(int id);
    Task ApproveAsync(int id, string username);
    Task RejectAsync(int id);
}
