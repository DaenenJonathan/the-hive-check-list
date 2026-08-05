using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
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
    TimeSpan? PlannedReturnTime = null,
    Guid? TemplateChecklistId = null
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

        Checklist? checklist = null;
        if (request.TemplateChecklistId is { } templateId)
        {
            var template = await _context.Checklists
                .Include(c => c.Items)
                .FirstOrDefaultAsync(c => c.Id == templateId, cancellationToken);
            if (template is null)
                return Result<Guid>.Failure("Checklist modèle introuvable.");

            checklist = Checklist.Create(template.Name, action.Id, eventDate: request.PlannedDate);
            checklist.SetCreated(_currentUser.UserId!);

            foreach (var item in template.Items.OrderBy(i => i.SortOrder))
            {
                var clone = ChecklistItem.Create(
                    checklist.Id, item.MaterialName, item.QuantityRequested,
                    location: item.Location, category: item.Category, notes: item.Notes,
                    sortOrder: item.SortOrder);
                clone.SetCreated(_currentUser.UserId!);
                checklist.AddItem(clone);
            }

            _context.Checklists.Add(checklist);
        }

        await _context.SaveChangesAsync(cancellationToken);

        await _auditService.LogAsync("Create", "BrandAction", action.Id,
            newValue: request.Name, cancellationToken: cancellationToken);
        if (checklist is not null)
            await _auditService.LogAsync("CreateFromTemplate", "Checklist", checklist.Id,
                oldValue: request.TemplateChecklistId.ToString(), newValue: $"{checklist.Items.Count} articles",
                cancellationToken: cancellationToken);

        return Result<Guid>.Success(action.Id);
    }
}
