using FluentValidation;
using MediatR;
using TheHive.Application.Common.Interfaces;
using TheHive.Application.Common.Models;
using TheHive.Domain.Entities;

namespace TheHive.Application.Features.Actions.Commands.CreateAction;

public record CreateActionCommand(
    string Name,
    string Client,
    DateTime PlannedDate,
    string? Description,
    TimeSpan? PlannedDepartureTime = null,
    TimeSpan? PlannedReturnTime = null
) : IRequest<Result<Guid>>;

public class CreateActionCommandValidator : AbstractValidator<CreateActionCommand>
{
    public CreateActionCommandValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Client).NotEmpty().MaximumLength(200);
        RuleFor(x => x.PlannedDate).GreaterThan(DateTime.UtcNow.AddDays(-1));
    }
}

public class CreateActionCommandHandler : IRequestHandler<CreateActionCommand, Result<Guid>>
{
    private readonly IApplicationDbContext _context;
    private readonly IAuditService _auditService;
    private readonly ICurrentUserService _currentUser;

    public CreateActionCommandHandler(
        IApplicationDbContext context,
        IAuditService auditService,
        ICurrentUserService currentUser)
    {
        _context = context;
        _auditService = auditService;
        _currentUser = currentUser;
    }

    public async Task<Result<Guid>> Handle(CreateActionCommand request, CancellationToken cancellationToken)
    {
        var action = BrandAction.Create(request.Name, request.Client, request.PlannedDate, request.Description,
            plannedDepartureTime: request.PlannedDepartureTime, plannedReturnTime: request.PlannedReturnTime);
        action.SetCreated(_currentUser.UserId!);

        _context.BrandActions.Add(action);
        await _context.SaveChangesAsync(cancellationToken);

        await _auditService.LogAsync("Create", "BrandAction", action.Id,
            newValue: request.Name, cancellationToken: cancellationToken);

        return Result<Guid>.Success(action.Id);
    }
}
