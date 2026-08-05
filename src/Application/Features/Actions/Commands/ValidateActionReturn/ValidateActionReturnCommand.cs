using MediatR;
using Microsoft.EntityFrameworkCore;
using TheHive.Application.Common.Exceptions;
using TheHive.Application.Common.Interfaces;
using TheHive.Application.Common.Models;

namespace TheHive.Application.Features.Actions.Commands.ValidateActionReturn;

public record ValidateActionReturnCommand(Guid ActionId) : IRequest<Result>;

public class ValidateActionReturnCommandHandler : IRequestHandler<ValidateActionReturnCommand, Result>
{
    private readonly IApplicationDbContext _context;
    private readonly IAuditService _auditService;
    private readonly ICurrentUserService _currentUser;

    public ValidateActionReturnCommandHandler(
        IApplicationDbContext context,
        IAuditService auditService,
        ICurrentUserService currentUser)
    {
        _context = context;
        _auditService = auditService;
        _currentUser = currentUser;
    }

    public async Task<Result> Handle(ValidateActionReturnCommand request, CancellationToken cancellationToken)
    {
        var action = await _context.BrandActions
            .FirstOrDefaultAsync(a => a.Id == request.ActionId, cancellationToken)
            ?? throw new NotFoundException("BrandAction", request.ActionId);

        if (!action.Sent)
            return Result.Failure("Le retour ne peut être validé que si l'action a été marquée comme envoyée.");

        var wasValidated = action.ReturnValidated;
        action.ValidateReturn(_currentUser.UserId!);

        await _context.SaveChangesAsync(cancellationToken);

        await _auditService.LogAsync("ValidateReturn", "BrandAction", action.Id,
            oldValue: wasValidated.ToString(), newValue: "True", cancellationToken: cancellationToken);

        return Result.Success();
    }
}
