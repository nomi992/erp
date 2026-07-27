using erp_backend.Models;
using erp_backend.StockAdjustments.Dtos;

namespace erp_backend.StockAdjustments;

public interface IStockAdjustmentRepository
{
    Task<PagedResult<StockAdjustmentListItemResponse>> GetAllAsync(
        AdjustmentReasonCode? reasonCode, StockAdjustmentStatus? status, DateTime? from, DateTime? to,
        string? search, string? sortBy, string? sortDirection,
        int pageNumber, int pageSize);

    Task<StockAdjustmentResponse> GetByIdAsync(int id);
    Task<StockAdjustmentResponse> CreateAsync(StockAdjustmentRequest request, string username);
    Task SubmitAsync(int id);
    Task ApproveAsync(int id, string username);
    Task RejectAsync(int id);
}
