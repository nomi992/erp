using erp_backend.Models;
using erp_backend.Transfers.Dtos;

namespace erp_backend.Transfers;

public interface IStockTransferRepository
{
    Task<PagedResult<StockTransferListItemResponse>> GetAllAsync(StockTransferStatus? status, DateTime? from, DateTime? to, string? search, string? sortBy, string? sortDirection, int pageNumber, int pageSize);
    Task<StockTransferResponse> GetByIdAsync(int id);
    Task<StockTransferResponse> CreateAsync(StockTransferRequest request, string username);
    Task SubmitAsync(int id);
    Task ApproveAsync(int id, string username);
    Task RejectAsync(int id);
    Task<StockTransferResponse> ReceiveAsync(int id, StockTransferReceiveRequest request, string username);
}
