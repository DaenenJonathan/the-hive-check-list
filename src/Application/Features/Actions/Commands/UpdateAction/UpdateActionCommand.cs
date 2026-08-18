using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using TheHive.Application.Common.Exceptions;
using TheHive.Application.Common.Interfaces;
using TheHive.Application.Common.Models;

namespace TheHive.Application.Features.Actions.Commands.UpdateAction;

public record UpdateActionCommand(
    Guid Id,
    string Name,
    Guid BrandId,
    DateTime PlannedDate,
    string? Description,
    TimeSpan? PlannedDepartureTime = null,
    TimeSpan? PlannedReturnTime = null
) : IRequest<Result>;

public class UpdateActionCommandValidator : AbstractValidator<UpdateActionCommand>
{
    public UpdateActionCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.BrandId).NotEmpty();
    }
}

public class UpdateActionCommandHandler : IRequestHandler<UpdateActionCommand, Result>
{
    private readonly IApplicationDbContext _context;
    private readonly IAuditService _auditService;
    private readonly ICurrentUserService _currentUser;

    public UpdateActionCommandHandler(
        IApplicationDbContext context,
        IAuditService auditService,
        ICurrentUserService currentUser)
    {
        _context = context;
        _auditService = auditService;
        _currentUser = currentUser;
    }

    public async Task<Result> Handle(UpdateActionCommand request, CancellationToken cancellationToken)
    {
        var action = await _context.BrandActions
            .FirstOrDefaultAsync(a => a.Id == request.Id, cancellationToken)
            ?? throw new NotFoundException("BrandAction", request.Id);

        var oldPlannedDate = action.PlannedDate;
        action.Update(request.Name, request.BrandId, request.PlannedDate, request.Description,
            request.PlannedDepartureTime, request.PlannedReturnTime);
        action.SetUpdated(_currentUser.UserId!);

        await _context.SaveChangesAsync(cancellationToken);

        await _auditService.LogAsync("Update", "BrandAction", action.Id,
            oldValue: oldPlannedDate.ToString("O"), newValue: request.PlannedDate.ToString("O"),
            cancellationToken: cancellationToken);

        return Result.Success();
    }
}
