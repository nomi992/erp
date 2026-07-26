using erp_backend.Models.Abstractions;

namespace erp_backend.Models;

/// <summary>
/// Fast-path "current on-hand" snapshot per (ProductVariant, Warehouse), kept in lockstep with every
/// StockLedgerEntry insert so reports don't have to sum the whole ledger.
/// </summary>
public class StockBalance : IBranchScoped
{
    public int Id { get; set; }
    public int TenantId { get; set; }
    public int BranchId { get; set; }
    public int ProductVariantId { get; set; }
    public ProductVariant? ProductVariant { get; set; }
    public int WarehouseId { get; set; }
    public Warehouse? Warehouse { get; set; }
    public decimal QuantityOnHand { get; set; }
    public decimal AverageCost { get; set; }
    public decimal? ReorderLevel { get; set; }
    public DateTime? LastMovementAtUtc { get; set; }
}
