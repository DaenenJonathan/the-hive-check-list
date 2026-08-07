using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using TheHive.Application.Common.Interfaces;
using TheHive.Application.Common.Models;
using TheHive.Domain.Enums;

namespace TheHive.Application.Features.Checklists.Commands.BatchUpdateItemStatus;

public record ItemStatusUpdateDto(Guid ItemId, ChecklistItemStatus Status, int QuantityPrepared, string? Remark);

public record BatchUpdateItemStatusCommand(Guid ChecklistId, List<ItemStatusUpdateDto> Items) : IRequest<Result>;

public class BatchUpdateItemStatusCommandValidator : AbstractValidator<BatchUpdateItemStatusCommand>
{
    public BatchUpdateItemStatusCommandValidator()
    {
        RuleFor(x => x.ChecklistId).NotEmpty();
        RuleFor(x => x.Items).NotEmpty();
        RuleForEach(x => x.Items).ChildRules(item =>
        {
            item.RuleFor(i => i.ItemId).NotEmpty();
            item.RuleFor(i => i.QuantityPrepared).GreaterThanOrEqualTo(0);
        });
    }
}

public class BatchUpdateItemStatusCommandHandler : IRequestHandler<BatchUpdateItemStatusCommand, Result>
{
    private readonly IApplicationDbContext _context;
    private readonly IAuditService _auditService;
    private readonly ICurrentUserService _currentUser;
    private readonly IChecklistHub _hub;

    public BatchUpdateItemStatusCommandHandler(
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

    public async Task<Result> Handle(BatchUpdateItemStatusCommand request, CancellationToken cancellationToken)
    {
        var actionStatus = await _context.Checklists
            .Where(c => c.Id == request.ChecklistId)
            .Select(c => c.BrandAction!.Status)
            .FirstOrDefaultAsync(cancellationToken);

        if (actionStatus == ActionStatus.Cancelled)
            return Result.Failure("Impossible de modifier les articles : l'action associée est annulée.");

        var items = await _context.ChecklistItems
            .Where(i => i.ChecklistId == request.ChecklistId)
            .ToListAsync(cancellationToken);

        foreach (var dto in request.Items)
        {
            var item = items.FirstOrDefault(i => i.Id == dto.ItemId);
            if (item is null) continue;

            var oldStatus = item.Status.ToString();
            item.UpdateStatus(dto.Status, dto.QuantityPrepared, dto.Remark, _currentUser.UserId!);

            await _auditService.LogAsync("UpdateStatus", "ChecklistItem", item.Id,
                oldValue: oldStatus, newValue: dto.Status.ToString(), cancellationToken: cancellationToken);
        }

        await _context.SaveChangesAsync(cancellationToken);
        await _hub.NotifyChecklistUpdatedAsync(request.ChecklistId, cancellationToken);

        return Result.Success();
    }
}
