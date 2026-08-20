using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using TheHive.Application.Common.Exceptions;
using TheHive.Application.Common.Interfaces;
using TheHive.Application.Common.Models;
using TheHive.Application.Common.Security;
using TheHive.Domain.Entities;

namespace TheHive.Application.Features.Actions.Commands.CreateAction;

public record CreateActionCommand(
    string Name,
    Guid BrandId,
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
        RuleFor(x => x.BrandId).NotEmpty();
        // An action is always prepared ahead of time - same-day creation is never realistic, so the
        // planned date must be at least tomorrow, not just "not in the past".
        RuleFor(x => x.PlannedDate.Date).GreaterThan(DateTime.UtcNow.Date)
            .WithMessage("La date planifiée doit être au moins le lendemain.");
    }
}

public class CreateActionCommandHandler : IRequestHandler<CreateActionCommand, Result<Guid>>
{
    private readonly IApplicationDbContext _context;
    private readonly IAuditService _auditService;
    private readonly ICurrentUserService _currentUser;
    private readonly IImageStorageService _imageStorage;

    public CreateActionCommandHandler(
        IApplicationDbContext context,
        IAuditService auditService,
        ICurrentUserService currentUser,
        IImageStorageService imageStorage)
    {
        _context = context;
        _auditService = auditService;
        _currentUser = currentUser;
        _imageStorage = imageStorage;
    }

    public async Task<Result<Guid>> Handle(CreateActionCommand request, CancellationToken cancellationToken)
    {
        var brand = await _context.Brands
            .FirstOrDefaultAsync(b => b.Id == request.BrandId, cancellationToken)
            ?? throw new NotFoundException("Brand", request.BrandId);

        AgencyAccessGuard.EnsureCanAccessAgency(_currentUser, brand.AgencyId);
        BrandAccessGuard.EnsureCanAccessBrand(_currentUser, brand.Id);

        var action = BrandAction.Create(request.Name, request.BrandId, request.PlannedDate, request.Description,
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
                if (item.ImagePath is not null)
                {
                    var copiedImagePath = await _imageStorage.CopyAsync(item.ImagePath, cancellationToken);
                    clone.SetImage(copiedImagePath);
                }
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
