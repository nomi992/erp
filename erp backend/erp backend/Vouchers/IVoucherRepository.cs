using erp_backend.Models;
using erp_backend.Vouchers.Dtos;

namespace erp_backend.Vouchers;

public interface IVoucherRepository
{
    Task<List<VoucherListItemResponse>> GetAllAsync(VoucherType? type, VoucherStatus? status, DateTime? from, DateTime? to);
    Task<VoucherResponse> GetByIdAsync(int id);
    Task<VoucherResponse> CreateAsync(CreateVoucherRequest request, string username);
    Task<VoucherResponse> UpdateAsync(int id, CreateVoucherRequest request);
    Task SubmitAsync(int id);
    Task ApproveAsync(int id, string username);
    Task RejectAsync(int id);
    Task<VoucherResponse> ReverseAsync(int id, string username);
    Task<VoucherAttachmentResponse> UploadAttachmentAsync(int id, IFormFile file);
    Task<(byte[] Bytes, string ContentType, string FileName)> DownloadAttachmentAsync(int id, int attachmentId);
    Task DeleteAttachmentAsync(int id, int attachmentId);
}
