namespace erp_backend.Accounting;

public interface IFiscalPeriodGuard
{
    Task<bool> IsDateInClosedPeriodAsync(DateTime date);
}
