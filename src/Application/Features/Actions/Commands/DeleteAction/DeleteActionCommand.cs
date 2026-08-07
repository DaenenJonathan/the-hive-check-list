using MediatR;
using Microsoft.EntityFrameworkCore;
using TheHive.Application.Common.Exceptions;
using TheHive.Application.Common.Interfaces;
using TheHive.Application.Common.Models;

namespace TheHive.Application.Features.Actions.Commands.DeleteAction;

public record DeleteActionCommand(Guid ActionId) : IRequest<Result>;

public class DeleteActionCommandHandler : IRequestHandler<DeleteActionCommand, Result>
{
    private readonly IApplicationDbContext _context;
    private readonly IAuditService _auditService;

    public DeleteActionCommandHandler(IApplicationDbContext context, IAuditService auditService)
    {
        _context = context;
        _auditService = auditService;
    }

    public async Task<Result> Handle(DeleteActionCommand request, CancellationToken cancellationToken)
    {
        var action = await _context.BrandActions
            .FirstOrDefaultAsync(a => a.Id == request.ActionId, cancellationToken)
            ?? throw new NotFoundException("BrandAction", request.ActionId);

        await _auditService.LogAsync("DeleteAction", "BrandAction", action.Id,
            oldValue: action.Name, cancellationToken: cancellationToken);

        _context.BrandActions.Remove(action);
        await _context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
