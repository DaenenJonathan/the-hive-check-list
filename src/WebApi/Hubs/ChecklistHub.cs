using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using TheHive.Application.Common.Interfaces;

namespace TheHive.WebApi.Hubs;

[Authorize]
public class ChecklistHub : Hub
{
    public async Task JoinChecklistGroup(string checklistId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, $"checklist-{checklistId}");
    }

    public async Task LeaveChecklistGroup(string checklistId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"checklist-{checklistId}");
    }
}

public class SignalRChecklistHub : IChecklistHub
{
    private readonly IHubContext<ChecklistHub> _hub;

    public SignalRChecklistHub(IHubContext<ChecklistHub> hub) => _hub = hub;

    public async Task NotifyItemUpdatedAsync(Guid checklistId, Guid itemId, CancellationToken cancellationToken = default)
    {
        await _hub.Clients.Group($"checklist-{checklistId}")
            .SendAsync("ItemUpdated", new { checklistId, itemId }, cancellationToken);
    }

    public async Task NotifyChecklistUpdatedAsync(Guid checklistId, CancellationToken cancellationToken = default)
    {
        await _hub.Clients.Group($"checklist-{checklistId}")
            .SendAsync("ChecklistUpdated", new { checklistId }, cancellationToken);
    }
}
