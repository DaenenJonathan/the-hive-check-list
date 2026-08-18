using TheHive.Domain.Enums;

namespace TheHive.Application.Features.Notifications.DTOs;

public class NotificationDto
{
    public Guid Id { get; set; }
    public NotificationType Type { get; set; }
    public Guid? BrandActionId { get; set; }
    public Guid? ChecklistId { get; set; }
    public string? ActionName { get; set; }
    public string? ChecklistName { get; set; }
    public string? RequesterName { get; set; }
    public string? RequesterEmail { get; set; }
    public string? Message { get; set; }
    public bool IsRead { get; set; }
    public DateTime CreatedAt { get; set; }
}
