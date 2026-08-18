using TheHive.Domain.Enums;

namespace TheHive.Application.Common.Interfaces;

public interface INotificationDispatcher
{
    Task DispatchToRoleAsync(
        string role,
        NotificationType type,
        Guid brandActionId,
        Guid checklistId,
        string actionName,
        string checklistName,
        CancellationToken cancellationToken = default);

    Task DispatchAccountRequestAsync(
        string requesterName,
        string requesterEmail,
        string? message,
        CancellationToken cancellationToken = default);
}
