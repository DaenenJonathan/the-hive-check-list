using TheHive.Domain.Entities;

namespace TheHive.Application.Common.Interfaces;

public interface IAuditService
{
    Task LogAsync(string action, string entityType, Guid entityId,
        string? oldValue = null, string? newValue = null,
        CancellationToken cancellationToken = default);
}
