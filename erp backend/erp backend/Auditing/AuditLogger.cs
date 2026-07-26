using System.Security.Claims;
using System.Text.Json;
using erp_backend.Auth;
using erp_backend.Models;
using erp_backend.Repositories;

namespace erp_backend.Auditing;

public class AuditLogger : IAuditLogger
{
    private readonly IRepository<AuditLog> _logs;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ICurrentTenantContext _tenantContext;

    public AuditLogger(IRepository<AuditLog> logs, IHttpContextAccessor httpContextAccessor, ICurrentTenantContext tenantContext)
    {
        _logs = logs;
        _httpContextAccessor = httpContextAccessor;
        _tenantContext = tenantContext;
    }

    public async Task LogAsync(string entityName, int entityId, string action, object? oldValue = null, object? newValue = null)
    {
        var user = _httpContextAccessor.HttpContext?.User;
        var userIdClaim = user?.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        var log = new AuditLog
        {
            EntityName = entityName,
            EntityId = entityId,
            Action = action,
            OldValue = oldValue is null ? null : JsonSerializer.Serialize(oldValue),
            NewValue = newValue is null ? null : JsonSerializer.Serialize(newValue),
            UserId = int.TryParse(userIdClaim, out var id) ? id : null,
            Username = user?.Identity?.Name ?? "system",
            BranchId = _tenantContext.BranchId,
        };

        await _logs.AddAsync(log);
        await _logs.SaveChangesAsync();
    }
}
