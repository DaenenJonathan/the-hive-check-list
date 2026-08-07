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

    public ReactivateActionCommandHandler(IApplicationDbContext context, IAuditService auditService)
    {
        _context = context;
        _auditService = auditService;
    }

    public async Task<Result> Handle(ReactivateActionCommand request, CancellationToken cancellationToken)
    {
        var action = await _context.BrandActions
            .FirstOrDefaultAsync(a => a.Id == request.ActionId, cancellationToken)
            ?? throw new NotFoundException("BrandAction", request.ActionId);

        if (action.Status != ActionStatus.Cancelled)
            return Result.Failure("Cette action n'est pas annulée.");

        action.SetStatus(ActionStatus.Planned);
        await _context.SaveChangesAsync(cancellationToken);

        await _auditService.LogAsync("ReactivateAction", "BrandAction", action.Id,
            oldValue: ActionStatus.Cancelled.ToString(), newValue: ActionStatus.Planned.ToString(), cancellationToken: cancellationToken);

        return Result.Success();
    }
}
