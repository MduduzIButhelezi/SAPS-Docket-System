using System.Security.Claims;
using SAPS.DocketSystem.Api.Data;
using SAPS.DocketSystem.Api.Models;

namespace SAPS.DocketSystem.Api.Services;

public interface IAuditService
{
    Task WriteAsync(ClaimsPrincipal user, string action, string entityType, string entityId, string? details = null);
}

public class AuditService(AppDbContext db, IHttpContextAccessor accessor) : IAuditService
{
    public async Task WriteAsync(ClaimsPrincipal user, string action, string entityType, string entityId, string? details = null)
    {
        int? userId = int.TryParse(user.FindFirstValue(ClaimTypes.NameIdentifier), out var id) ? id : null;
        db.AuditLogs.Add(new AuditLog
        {
            UserId = userId,
            Actor = user.Identity?.Name ?? "Public",
            Action = action,
            EntityType = entityType,
            EntityId = entityId,
            Details = details,
            IpAddress = accessor.HttpContext?.Connection.RemoteIpAddress?.ToString()
        });
        await db.SaveChangesAsync();
    }
}
