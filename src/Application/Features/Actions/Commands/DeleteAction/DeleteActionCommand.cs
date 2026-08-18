using MediatR;
using Microsoft.EntityFrameworkCore;
using TheHive.Application.Common.Exceptions;
using TheHive.Application.Common.Interfaces;
using TheHive.Application.Common.Models;
using TheHive.Application.Common.Security;

namespace TheHive.Application.Features.Actions.Commands.DeleteAction;

public record DeleteActionCommand(Guid ActionId) : IRequest<Result>;

public class DeleteActionCommandHandler : IRequestHandler<DeleteActionCommand, Result>
{
    private readonly IApplicationDbContext _context;
    private readonly IAuditService _auditService;
    private readonly ICurrentUserService _currentUser;

    public DeleteActionCommandHandler(IApplicationDbContext context, IAuditService auditService, ICurrentUserService currentUser)
    {
        _context = context;
        _auditService = auditService;
        _currentUser = currentUser;
    }

    public async Task<Result> Handle(DeleteActionCommand request, CancellationToken cancellationToken)
    {
        var action = await _context.BrandActions
            .Include(a => a.Brand)
            .FirstOrDefaultAsync(a => a.Id == request.ActionId, cancellationToken)
            ?? throw new NotFoundException("BrandAction", request.ActionId);

        AgencyAccessGuard.EnsureCanAccessAgency(_currentUser, action.Brand?.AgencyId);
        BrandAccessGuard.EnsureCanAccessBrand(_currentUser, action.BrandId);

        await _auditService.LogAsync("DeleteAction", "BrandAction", action.Id,
            oldValue: action.Name, cancellationToken: cancellationToken);

        _context.BrandActions.Remove(action);
        await _context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
