using MediatR;
using Microsoft.EntityFrameworkCore;
using TheHive.Application.Common.Exceptions;
using TheHive.Application.Common.Interfaces;
using TheHive.Application.Common.Models;
using TheHive.Domain.Enums;

namespace TheHive.Application.Features.ChecklistItems.Commands.DeleteChecklistItem;

public record DeleteChecklistItemCommand(Guid ItemId) : IRequest<Result>;

public class DeleteChecklistItemCommandHandler : IRequestHandler<DeleteChecklistItemCommand, Result>
{
    private readonly IApplicationDbContext _context;
    private readonly IAuditService _auditService;
    private readonly ICurrentUserService _currentUser;
    private readonly INotificationDispatcher _notificationDispatcher;

    public DeleteChecklistItemCommandHandler(
        IApplicationDbContext context,
        IAuditService auditService,
        ICurrentUserService currentUser,
        INotificationDispatcher notificationDispatcher)
    {
        _context = context;
        _auditService = auditService;
        _currentUser = currentUser;
        _notificationDispatcher = notificationDispatcher;
    }

    public async Task<Result> Handle(DeleteChecklistItemCommand request, CancellationToken cancellationToken)
    {
        var item = await _context.ChecklistItems
            .Include(i => i.Checklist).ThenInclude(c => c!.BrandAction)
            .FirstOrDefaultAsync(i => i.Id == request.ItemId, cancellationToken)
            ?? throw new NotFoundException("ChecklistItem", request.ItemId);

        await _auditService.LogAsync("DeleteItem", "ChecklistItem", item.Id,
            oldValue: item.MaterialName, cancellationToken: cancellationToken);

        var checklist = item.Checklist;

        _context.ChecklistItems.Remove(item);
        await _context.SaveChangesAsync(cancellationToken);

        if (checklist?.BrandAction is not null)
        {
            await _notificationDispatcher.DispatchToRoleAsync(
                "WarehouseUser", NotificationType.ItemsChangedOnAction,
                checklist.BrandActionId, checklist.Id, checklist.BrandAction.Name, checklist.Name,
                cancellationToken);
        }

        return Result.Success();
    }
}
