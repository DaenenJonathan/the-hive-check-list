using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using TheHive.Application.Common.Exceptions;
using TheHive.Application.Common.Interfaces;
using TheHive.Application.Common.Models;
using TheHive.Domain.Enums;

namespace TheHive.Application.Features.ChecklistItems.Commands.UpdateChecklistItemStatus;

public record UpdateChecklistItemStatusCommand(
    Guid ItemId,
    ChecklistItemStatus Status,
    int QuantityPrepared,
    string? Remark
) : IRequest<Result>;

public class UpdateChecklistItemStatusCommandValidator : AbstractValidator<UpdateChecklistItemStatusCommand>
{
    public UpdateChecklistItemStatusCommandValidator()
    {
        RuleFor(x => x.ItemId).NotEmpty();
        RuleFor(x => x.QuantityPrepared).GreaterThanOrEqualTo(0);
    }
}

public class UpdateChecklistItemStatusCommandHandler : IRequestHandler<UpdateChecklistItemStatusCommand, Result>
{
    private readonly IApplicationDbContext _context;
    private readonly IAuditService _auditService;
    private readonly ICurrentUserService _currentUser;
    private readonly IChecklistHub _hub;
    private readonly INotificationDispatcher _notificationDispatcher;

    public UpdateChecklistItemStatusCommandHandler(
        IApplicationDbContext context,
        IAuditService auditService,
        ICurrentUserService currentUser,
        IChecklistHub hub,
        INotificationDispatcher notificationDispatcher)
    {
        _context = context;
        _auditService = auditService;
        _currentUser = currentUser;
        _hub = hub;
        _notificationDispatcher = notificationDispatcher;
    }

    public async Task<Result> Handle(UpdateChecklistItemStatusCommand request, CancellationToken cancellationToken)
    {
        var item = await _context.ChecklistItems
            .Include(i => i.Checklist).ThenInclude(c => c!.BrandAction)
            .FirstOrDefaultAsync(i => i.Id == request.ItemId, cancellationToken)
            ?? throw new NotFoundException("ChecklistItem", request.ItemId);

        if (item.Checklist?.BrandAction?.Status == ActionStatus.Cancelled)
            return Result.Failure("Impossible de modifier un article : l'action associée est annulée.");

        var oldStatus = item.Status.ToString();
        item.UpdateStatus(request.Status, request.QuantityPrepared, request.Remark, _currentUser.UserId!);

        await _context.SaveChangesAsync(cancellationToken);

        await _auditService.LogAsync("UpdateStatus", "ChecklistItem", item.Id,
            oldValue: oldStatus, newValue: request.Status.ToString(), cancellationToken: cancellationToken);

        await _hub.NotifyItemUpdatedAsync(item.ChecklistId, item.Id, cancellationToken);

        if (item.Checklist?.BrandAction is not null)
        {
            var siblingItems = await _context.ChecklistItems
                .Where(i => i.ChecklistId == item.ChecklistId)
                .ToListAsync(cancellationToken);

            await NotifyIfCompletedWithMissingAsync(
                _context, _notificationDispatcher, siblingItems,
                item.Checklist.BrandActionId, item.Checklist.Id,
                item.Checklist.BrandAction.Name, item.Checklist.Name, cancellationToken);
        }

        return Result.Success();
    }

    internal static bool IsCompletedWithMissing(IEnumerable<Domain.Entities.ChecklistItem> items) =>
        items.All(i => i.Status != ChecklistItemStatus.ToPrepare) && items.Any(i => i.Status == ChecklistItemStatus.Missing);

    internal static async Task NotifyIfCompletedWithMissingAsync(
        IApplicationDbContext context,
        INotificationDispatcher notificationDispatcher,
        IReadOnlyCollection<Domain.Entities.ChecklistItem> items,
        Guid brandActionId,
        Guid checklistId,
        string actionName,
        string checklistName,
        CancellationToken cancellationToken)
    {
        if (!IsCompletedWithMissing(items)) return;

        var alreadyNotified = await context.Notifications.AnyAsync(
            n => n.ChecklistId == checklistId && n.Type == NotificationType.ChecklistCompletedWithMissing,
            cancellationToken);
        if (alreadyNotified) return;

        await notificationDispatcher.DispatchToRoleAsync(
            "Manager", NotificationType.ChecklistCompletedWithMissing,
            brandActionId, checklistId, actionName, checklistName, cancellationToken);
    }
}
