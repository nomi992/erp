namespace erp_backend.Transfers.Dtos;

public class StockTransferLineRequest
{
    public int ProductVariantId { get; set; }
    public int UnitOfMeasureId { get; set; }
    public decimal Qty { get; set; }
}

public class StockTransferRequest
{
    public int SourceWarehouseId { get; set; }
    public int DestinationWarehouseId { get; set; }
    public DateTime Date { get; set; }
    public string? Narration { get; set; }
    public List<StockTransferLineRequest> Lines { get; set; } = [];
}

public class StockTransferReceiveLineRequest
{
    public int LineId { get; set; }
    public decimal ReceivedBaseQty { get; set; }
}

public class StockTransferReceiveRequest
{
    public List<StockTransferReceiveLineRequest> Lines { get; set; } = [];
}
