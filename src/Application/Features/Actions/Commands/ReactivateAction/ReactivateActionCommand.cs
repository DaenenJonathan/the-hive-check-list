using MediatR;
using Microsoft.EntityFrameworkCore;
using TheHive.Application.Common.Exceptions;
using TheHive.Application.Common.Interfaces;
using TheHive.Application.Common.Models;
using TheHive.Domain.Enums;

namespace TheHive.Application.Features.Actions.Commands.ReactivateAction;

public record ReactivateActionCommand(Guid ActionId) : IRequest<Result>;

public class ReactivateActionCommandHandler : IRequestHandler<ReactivateActionCommand, Result>
{
    private readonly IApplicationDbContext _context;
    private readonly IAuditService _auditService;
    private readonly INotificationDispatcher _notificationDispatcher;

    public ReactivateActionCommandHandler(
        IApplicationDbContext context,
        IAuditService auditService,
        INotificationDispatcher notificationDispatcher)
    {
        _context = context;
        _auditService = auditService;
        _notificationDispatcher = notificationDispatcher;
    }

    public async Task<Result> Handle(ReactivateActionCommand request, CancellationToken cancellationToken)
    {
        var action = await _context.BrandActions
            .Include(a => a.Checklists)
            .FirstOrDefaultAsync(a => a.Id == request.ActionId, cancellationToken)
            ?? throw new NotFoundException("BrandAction", request.ActionId);

        if (action.Status != ActionStatus.Cancelled)
            return Result.Failure("Cette action n'est pas annulée.");

        action.SetStatus(ActionStatus.Planned);
        await _context.SaveChangesAsync(cancellationToken);

        await _auditService.LogAsync("ReactivateAction", "BrandAction", action.Id,
            oldValue: ActionStatus.Cancelled.ToString(), newValue: ActionStatus.Planned.ToString(), cancellationToken: cancellationToken);

        foreach (var checklist in action.Checklists)
        {
            await _notificationDispatcher.DispatchToRoleAsync(
                "WarehouseUser", NotificationType.ActionReactivated,
                action.Id, checklist.Id, action.Name, checklist.Name, cancellationToken);
        }

        return Result.Success();
    }
}
