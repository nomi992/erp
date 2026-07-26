namespace erp_backend.Inventory;

/// <summary>
/// Shared "{prefix}-{year}-{0000}" numbering scheme for every new stock/invoice document type, mirroring
/// Vouchers/VoucherNumberGenerator.cs. Each caller supplies its own year-scoped count (the query shape
/// differs per entity/type) since this stays a pure formatter rather than a DbContext-coupled helper.
/// </summary>
public static class StockDocumentNumberGenerator
{
    public static string Generate(string prefix, int year, int countThisYearSoFar) =>
        $"{prefix}-{year}-{countThisYearSoFar + 1:0000}";
}
