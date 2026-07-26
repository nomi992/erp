namespace erp_backend.Reporting;

public interface IReportExporter
{
    byte[] ToPdf(string title, IReadOnlyList<string> columns, IReadOnlyList<IReadOnlyList<string>> rows);
    byte[] ToExcel(string title, IReadOnlyList<string> columns, IReadOnlyList<IReadOnlyList<string>> rows);
}
