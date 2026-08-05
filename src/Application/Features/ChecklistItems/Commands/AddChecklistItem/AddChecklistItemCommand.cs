using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using TheHive.Application.Common.Exceptions;
using TheHive.Application.Common.Interfaces;
using TheHive.Application.Common.Models;
using TheHive.Domain.Entities;

namespace TheHive.Application.Features.ChecklistItems.Commands.AddChecklistItem;

public record AddChecklistItemCommand(
    Guid ChecklistId,
    string MaterialName,
    int QuantityRequested,
    string? Category,
    string? Notes,
    string? Location
) : IRequest<Result<Guid>>;

public class AddChecklistItemCommandValidator : AbstractValidator<AddChecklistItemCommand>
{
    public AddChecklistItemCommandValidator()
    {
        RuleFor(x => x.ChecklistId).NotEmpty();
        RuleFor(x => x.MaterialName).NotEmpty().MaximumLength(300);
        RuleFor(x => x.QuantityRequested).GreaterThan(0);
    }
}

public class AddChecklistItemCommandHandler : IRequestHandler<AddChecklistItemCommand, Result<Guid>>
{
    private readonly IApplicationDbContext _context;
    private readonly IAuditService _auditService;
    private readonly ICurrentUserService _currentUser;

    public AddChecklistItemCommandHandler(IApplicationDbContext context, IAuditService auditService, ICurrentUserService currentUser)
    {
        _context = context;
        _auditService = auditService;
        _currentUser = currentUser;
    }

    public async Task<Result<Guid>> Handle(AddChecklistItemCommand request, CancellationToken cancellationToken)
    {
        var checklist = await _context.Checklists
            .Include(c => c.Items)
            .FirstOrDefaultAsync(c => c.Id == request.ChecklistId, cancellationToken)
            ?? throw new NotFoundException("Checklist", request.ChecklistId);

        var sortOrder = checklist.Items.Count > 0 ? checklist.Items.Max(i => i.SortOrder) + 1 : 0;

        var item = ChecklistItem.Create(
            request.ChecklistId,
            request.MaterialName,
            request.QuantityRequested,
            location: request.Location,
            category: request.Category,
            notes: request.Notes,
            sortOrder: sortOrder);
        item.SetCreated(_currentUser.UserId!);

        checklist.AddItem(item);
        _context.ChecklistItems.Add(item);
        await _context.SaveChangesAsync(cancellationToken);

        await _auditService.LogAsync("AddItem", "ChecklistItem", item.Id,
            newValue: request.MaterialName, cancellationToken: cancellationToken);

        return Result<Guid>.Success(item.Id);
    }
}
