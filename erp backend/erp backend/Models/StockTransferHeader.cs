using erp_backend.Models.Abstractions;

namespace erp_backend.Models;

public enum StockTransferStatus
{
    Draft,
    PendingApproval,
    PendingReceipt,
    Completed,
    Rejected,
    Cancelled,
}

/// <summary>
/// TenantId/BranchId on the header = the source branch. A same-branch transfer completes fully at
/// Approve; a cross-branch transfer stops at PendingReceipt until the destination branch confirms
/// receipt in a separate request made under its own branch context.
/// </summary>
public class StockTransferHeader : IBranchScoped
{
    public int Id { get; set; }
    public int TenantId { get; set; }
    public int BranchId { get; set; }
    public string TransferNo { get; set; } = string.Empty;
    public int SourceWarehouseId { get; set; }
    public Warehouse? SourceWarehouse { get; set; }
    public int DestinationWarehouseId { get; set; }
    public Warehouse? DestinationWarehouse { get; set; }
    public int DestinationBranchId { get; set; }
    public Branch? DestinationBranch { get; set; }
    public DateTime Date { get; set; }
    public StockTransferStatus Status { get; set; } = StockTransferStatus.Draft;
    public string? Narration { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    public string? ApprovedBy { get; set; }
    public DateTime? ApprovedAtUtc { get; set; }
    public string? ReceivedBy { get; set; }
    public DateTime? ReceivedAtUtc { get; set; }

    public List<StockTransferLine> Lines { get; set; } = [];

    [System.ComponentModel.DataAnnotations.Timestamp]
    public byte[] RowVersion { get; set; } = [];
}
