using erp_backend.Models.Abstractions;

namespace erp_backend.Models;

public enum AdjustmentReasonCode
{
    Damage,
    Expiry,
    Loss,
    CountIncrease,
    CountDecrease,
    OpeningBalance,
    Other,
}

public enum StockAdjustmentStatus
{
    Draft,
    PendingApproval,
    Posted,
    Rejected,
}

/// <summary>
/// Also how a new item's opening stock balance gets entered — a StockAdjustment with
/// ReasonCode == OpeningBalance and every line's Direction == Increase, one line per
/// (ProductVariant, Warehouse) being seeded. No separate "opening balance" document/table.
/// </summary>
public class StockAdjustmentHeader : IBranchScoped
{
    public int Id { get; set; }
    public int TenantId { get; set; }
    public int BranchId { get; set; }
    public string AdjustmentNo { get; set; } = string.Empty;
    public int WarehouseId { get; set; }
    public Warehouse? Warehouse { get; set; }
    public DateTime Date { get; set; }
    public AdjustmentReasonCode ReasonCode { get; set; }
    public StockAdjustmentStatus Status { get; set; } = StockAdjustmentStatus.Draft;
    public string? Narration { get; set; }
    public int? LinkedVoucherId { get; set; }
    public VoucherHeader? LinkedVoucher { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    public string? ApprovedBy { get; set; }
    public DateTime? ApprovedAtUtc { get; set; }

    public List<StockAdjustmentLine> Lines { get; set; } = [];

    [System.ComponentModel.DataAnnotations.Timestamp]
    public byte[] RowVersion { get; set; } = [];
}
