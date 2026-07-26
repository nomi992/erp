using erp_backend.Models.Abstractions;

namespace erp_backend.Models;

public class UOMConversion : ITenantScoped
{
    public int Id { get; set; }
    public int TenantId { get; set; }
    public int ProductId { get; set; }
    public Product? Product { get; set; }
    public int UnitOfMeasureId { get; set; }
    public UnitOfMeasure? UnitOfMeasure { get; set; }

    // Base-units of the product per 1 of UnitOfMeasureId (e.g. 24 for a Carton of a base-unit "Piece").
    public decimal ConversionFactor { get; set; }
}
