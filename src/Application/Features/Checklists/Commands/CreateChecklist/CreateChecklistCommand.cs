using FluentValidation;
using MediatR;
using TheHive.Application.Common.Interfaces;
using TheHive.Application.Common.Models;
using TheHive.Domain.Entities;

namespace TheHive.Application.Features.Checklists.Commands.CreateChecklist;

public record CreateChecklistCommand(string Name, Guid BrandActionId, DateTime? EventDate = null) : IRequest<Result<Guid>>;

public class CreateChecklistCommandValidator : AbstractValidator<CreateChecklistCommand>
{
    public CreateChecklistCommandValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.BrandActionId).NotEmpty();
    }
}

public class CreateChecklistCommandHandler : IRequestHandler<CreateChecklistCommand, Result<Guid>>
{
    private readonly IApplicationDbContext _context;
    private readonly IAuditService _auditService;
    private readonly ICurrentUserService _currentUser;

    public CreateChecklistCommandHandler(
        IApplicationDbContext context,
        IAuditService auditService,
        ICurrentUserService currentUser)
    {
        _context = context;
        _auditService = auditService;
        _currentUser = currentUser;
    }

    public async Task<Result<Guid>> Handle(CreateChecklistCommand request, CancellationToken cancellationToken)
    {
        var checklist = Checklist.Create(request.Name, request.BrandActionId, eventDate: request.EventDate);
        checklist.SetCreated(_currentUser.UserId!);

        _context.Checklists.Add(checklist);
        await _context.SaveChangesAsync(cancellationToken);

        await _auditService.LogAsync("Create", "Checklist", checklist.Id,
            newValue: request.Name, cancellationToken: cancellationToken);

        return Result<Guid>.Success(checklist.Id);
    }
}
