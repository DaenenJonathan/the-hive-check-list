using MediatR;
using Microsoft.EntityFrameworkCore;
using TheHive.Application.Common.Exceptions;
using TheHive.Application.Common.Interfaces;
using TheHive.Application.Common.Models;
using TheHive.Domain.Enums;

namespace TheHive.Application.Features.Actions.Commands.CancelAction;

public record CancelActionCommand(Guid ActionId) : IRequest<Result>;

public class CancelActionCommandHandler : IRequestHandler<CancelActionCommand, Result>
{
    private readonly IApplicationDbContext _context;
    private readonly IAuditService _auditService;

    public CancelActionCommandHandler(IApplicationDbContext context, IAuditService auditService)
    {
        _context = context;
        _auditService = auditService;
    }

    public async Task<Result> Handle(CancelActionCommand request, CancellationToken cancellationToken)
    {
        var action = await _context.BrandActions
            .FirstOrDefaultAsync(a => a.Id == request.ActionId, cancellationToken)
            ?? throw new NotFoundException("BrandAction", request.ActionId);

        if (action.Status == ActionStatus.Cancelled)
            return Result.Failure("Cette action est déjà annulée.");

        var oldStatus = action.Status.ToString();
        action.SetStatus(ActionStatus.Cancelled);
        await _context.SaveChangesAsync(cancellationToken);

        await _auditService.LogAsync("CancelAction", "BrandAction", action.Id,
            oldValue: oldStatus, newValue: ActionStatus.Cancelled.ToString(), cancellationToken: cancellationToken);

        return Result.Success();
    }
}
