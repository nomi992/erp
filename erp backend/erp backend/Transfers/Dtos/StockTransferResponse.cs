using erp_backend.Models;

namespace erp_backend.Transfers.Dtos;

public class StockTransferLineResponse
{
    public int Id { get; set; }
    public int ProductVariantId { get; set; }
    public string ProductVariantName { get; set; } = string.Empty;
    public string ProductName { get; set; } = string.Empty;
    public int UnitOfMeasureId { get; set; }
    public string UnitOfMeasureCode { get; set; } = string.Empty;
    public decimal Qty { get; set; }
    public decimal BaseQty { get; set; }
    public decimal UnitCostAtTransfer { get; set; }
    public decimal ReceivedBaseQty { get; set; }
}

public class StockTransferResponse
{
    public int Id { get; set; }
    public string TransferNo { get; set; } = string.Empty;
    public int SourceWarehouseId { get; set; }
    public string SourceWarehouseName { get; set; } = string.Empty;
    public int DestinationWarehouseId { get; set; }
    public string DestinationWarehouseName { get; set; } = string.Empty;
    public int DestinationBranchId { get; set; }
    public DateTime Date { get; set; }
    public StockTransferStatus Status { get; set; }
    public string? Narration { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
    public DateTime CreatedAtUtc { get; set; }
    public string? ApprovedBy { get; set; }
    public DateTime? ApprovedAtUtc { get; set; }
    public string? ReceivedBy { get; set; }
    public DateTime? ReceivedAtUtc { get; set; }
    public List<StockTransferLineResponse> Lines { get; set; } = [];
}

public class StockTransferListItemResponse
{
    public int Id { get; set; }
    public string TransferNo { get; set; } = string.Empty;
    public string SourceWarehouseName { get; set; } = string.Empty;
    public string DestinationWarehouseName { get; set; } = string.Empty;
    public DateTime Date { get; set; }
    public StockTransferStatus Status { get; set; }
}
