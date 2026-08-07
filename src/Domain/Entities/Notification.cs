using TheHive.Domain.Common;
using TheHive.Domain.Enums;

namespace TheHive.Domain.Entities;

public class Notification : BaseEntity
{
    public string RecipientUserId { get; private set; } = string.Empty;
    public NotificationType Type { get; private set; }
    public Guid BrandActionId { get; private set; }
    public Guid ChecklistId { get; private set; }
    public string ActionName { get; private set; } = string.Empty;
    public string ChecklistName { get; private set; } = string.Empty;
    public bool IsRead { get; private set; }
    public DateTime? ReadAt { get; private set; }

    private Notification() { }

    public static Notification Create(
        string recipientUserId,
        NotificationType type,
        Guid brandActionId,
        Guid checklistId,
        string actionName,
        string checklistName)
    {
        return new Notification
        {
            RecipientUserId = recipientUserId,
            Type = type,
            BrandActionId = brandActionId,
            ChecklistId = checklistId,
            ActionName = actionName,
            ChecklistName = checklistName
        };
    }

    public void MarkRead()
    {
        IsRead = true;
        ReadAt = DateTime.UtcNow;
    }
}
