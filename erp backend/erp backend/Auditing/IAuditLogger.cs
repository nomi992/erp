namespace erp_backend.Auditing;

public interface IAuditLogger
{
    Task LogAsync(string entityName, int entityId, string action, object? oldValue = null, object? newValue = null);
}
