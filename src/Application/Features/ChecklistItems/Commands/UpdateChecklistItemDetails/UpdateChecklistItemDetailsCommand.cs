using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using TheHive.Application.Common.Exceptions;
using TheHive.Application.Common.Interfaces;
using TheHive.Application.Common.Models;

namespace TheHive.Application.Features.ChecklistItems.Commands.UpdateChecklistItemDetails;

public record UpdateChecklistItemDetailsCommand(
    Guid ItemId,
    string MaterialName,
    int QuantityRequested,
    string? Category,
    string? Notes,
    string? Location
) : IRequest<Result>;

public class UpdateChecklistItemDetailsCommandValidator : AbstractValidator<UpdateChecklistItemDetailsCommand>
{
    public UpdateChecklistItemDetailsCommandValidator()
    {
        RuleFor(x => x.ItemId).NotEmpty();
        RuleFor(x => x.MaterialName).NotEmpty().MaximumLength(300);
        RuleFor(x => x.QuantityRequested).GreaterThan(0);
    }
}

public class UpdateChecklistItemDetailsCommandHandler : IRequestHandler<UpdateChecklistItemDetailsCommand, Result>
{
    private readonly IApplicationDbContext _context;
    private readonly IAuditService _auditService;
    private readonly ICurrentUserService _currentUser;

    public UpdateChecklistItemDetailsCommandHandler(IApplicationDbContext context, IAuditService auditService, ICurrentUserService currentUser)
    {
        _context = context;
        _auditService = auditService;
        _currentUser = currentUser;
    }

    public async Task<Result> Handle(UpdateChecklistItemDetailsCommand request, CancellationToken cancellationToken)
    {
        var item = await _context.ChecklistItems
            .FirstOrDefaultAsync(i => i.Id == request.ItemId, cancellationToken)
            ?? throw new NotFoundException("ChecklistItem", request.ItemId);

        var oldValue = item.MaterialName;
        item.UpdateDetails(request.MaterialName, request.QuantityRequested, request.Location, request.Category, request.Notes);
        item.SetUpdated(_currentUser.UserId!);

        await _context.SaveChangesAsync(cancellationToken);

        await _auditService.LogAsync("UpdateDetails", "ChecklistItem", item.Id,
            oldValue: oldValue, newValue: request.MaterialName, cancellationToken: cancellationToken);

        return Result.Success();
    }
}
