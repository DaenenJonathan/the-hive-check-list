using TheHive.Domain.Common;
using TheHive.Domain.Enums;

namespace TheHive.Domain.Entities;

public class Notification : BaseEntity
{
    public string RecipientUserId { get; private set; } = string.Empty;
    public NotificationType Type { get; private set; }
    public Guid? BrandActionId { get; private set; }
    public Guid? ChecklistId { get; private set; }
    public string? ActionName { get; private set; }
    public string? ChecklistName { get; private set; }
    public string? RequesterName { get; private set; }
    public string? RequesterEmail { get; private set; }
    public string? Message { get; private set; }
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

    public static Notification CreateAccountRequest(
        string recipientUserId,
        string requesterName,
        string requesterEmail,
        string? message)
    {
        return new Notification
        {
            RecipientUserId = recipientUserId,
            Type = NotificationType.AccountRequested,
            RequesterName = requesterName,
            RequesterEmail = requesterEmail,
            Message = message
        };
    }

    public void MarkRead()
    {
        IsRead = true;
        ReadAt = DateTime.UtcNow;
    }
}
