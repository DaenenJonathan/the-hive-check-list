using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using TheHive.Application.Common.Exceptions;
using TheHive.Application.Common.Interfaces;
using TheHive.Application.Common.Models;
using TheHive.Domain.Enums;

namespace TheHive.Application.Features.ChecklistItems.Commands.UpdateChecklistItemStatus;

public record UpdateChecklistItemStatusCommand(
    Guid ItemId,
    ChecklistItemStatus Status,
    int QuantityPrepared,
    string? Remark
) : IRequest<Result>;

public class UpdateChecklistItemStatusCommandValidator : AbstractValidator<UpdateChecklistItemStatusCommand>
{
    public UpdateChecklistItemStatusCommandValidator()
    {
        RuleFor(x => x.ItemId).NotEmpty();
        RuleFor(x => x.QuantityPrepared).GreaterThanOrEqualTo(0);
    }
}

public class UpdateChecklistItemStatusCommandHandler : IRequestHandler<UpdateChecklistItemStatusCommand, Result>
{
    private readonly IApplicationDbContext _context;
    private readonly IAuditService _auditService;
    private readonly ICurrentUserService _currentUser;
    private readonly IChecklistHub _hub;

    public UpdateChecklistItemStatusCommandHandler(
        IApplicationDbContext context,
        IAuditService auditService,
        ICurrentUserService currentUser,
        IChecklistHub hub)
    {
        _context = context;
        _auditService = auditService;
        _currentUser = currentUser;
        _hub = hub;
    }

    public async Task<Result> Handle(UpdateChecklistItemStatusCommand request, CancellationToken cancellationToken)
    {
        var item = await _context.ChecklistItems
            .FirstOrDefaultAsync(i => i.Id == request.ItemId, cancellationToken)
            ?? throw new NotFoundException("ChecklistItem", request.ItemId);

        var oldStatus = item.Status.ToString();
        item.UpdateStatus(request.Status, request.QuantityPrepared, request.Remark, _currentUser.UserId!);

        await _context.SaveChangesAsync(cancellationToken);

        await _auditService.LogAsync("UpdateStatus", "ChecklistItem", item.Id,
            oldValue: oldStatus, newValue: request.Status.ToString(), cancellationToken: cancellationToken);

        await _hub.NotifyItemUpdatedAsync(item.ChecklistId, item.Id, cancellationToken);

        return Result.Success();
    }
}
