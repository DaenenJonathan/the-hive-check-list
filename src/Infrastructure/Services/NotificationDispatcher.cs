using TheHive.Application.Common.Interfaces;
using TheHive.Domain.Entities;
using TheHive.Domain.Enums;

namespace TheHive.Infrastructure.Services;

public class NotificationDispatcher : INotificationDispatcher
{
    private readonly IApplicationDbContext _context;
    private readonly IUserDirectoryService _userDirectory;

    public NotificationDispatcher(IApplicationDbContext context, IUserDirectoryService userDirectory)
    {
        _context = context;
        _userDirectory = userDirectory;
    }

    public async Task DispatchToRoleAsync(
        string role,
        NotificationType type,
        Guid brandActionId,
        Guid checklistId,
        string actionName,
        string checklistName,
        CancellationToken cancellationToken = default)
    {
        var recipientIds = await _userDirectory.GetUserIdsByRoleAsync(role, cancellationToken);
        if (recipientIds.Count == 0) return;

        foreach (var recipientId in recipientIds)
        {
            _context.Notifications.Add(Notification.Create(
                recipientId, type, brandActionId, checklistId, actionName, checklistName));
        }

        await _context.SaveChangesAsync(cancellationToken);
    }
}
