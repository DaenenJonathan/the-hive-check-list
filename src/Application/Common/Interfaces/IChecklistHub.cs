namespace TheHive.Application.Common.Interfaces;

public interface IChecklistHub
{
    Task NotifyItemUpdatedAsync(Guid checklistId, Guid itemId, CancellationToken cancellationToken = default);
    Task NotifyChecklistUpdatedAsync(Guid checklistId, CancellationToken cancellationToken = default);
}
