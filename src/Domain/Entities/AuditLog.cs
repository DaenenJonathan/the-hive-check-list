using TheHive.Domain.Common;

namespace TheHive.Domain.Entities;

public class AuditLog : BaseEntity
{
    public string UserId { get; private set; } = string.Empty;
    public string UserName { get; private set; } = string.Empty;
    public string Action { get; private set; } = string.Empty;
    public string EntityType { get; private set; } = string.Empty;
    public Guid EntityId { get; private set; }
    public string? OldValue { get; private set; }
    public string? NewValue { get; private set; }
    public DateTime OccurredAt { get; private set; } = DateTime.UtcNow;

    private AuditLog() { }

    public static AuditLog Create(
        string userId,
        string userName,
        string action,
        string entityType,
        Guid entityId,
        string? oldValue = null,
        string? newValue = null)
    {
        return new AuditLog
        {
            UserId = userId,
            UserName = userName,
            Action = action,
            EntityType = entityType,
            EntityId = entityId,
            OldValue = oldValue,
            NewValue = newValue,
            OccurredAt = DateTime.UtcNow
        };
    }
}
