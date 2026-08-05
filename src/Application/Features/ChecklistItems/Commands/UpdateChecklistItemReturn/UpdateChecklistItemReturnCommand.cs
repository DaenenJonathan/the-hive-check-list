using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using TheHive.Application.Common.Exceptions;
using TheHive.Application.Common.Interfaces;
using TheHive.Application.Common.Models;

namespace TheHive.Application.Features.ChecklistItems.Commands.UpdateChecklistItemReturn;

public record UpdateChecklistItemReturnCommand(
    Guid ItemId,
    int QuantityReturned
) : IRequest<Result>;

public class UpdateChecklistItemReturnCommandValidator : AbstractValidator<UpdateChecklistItemReturnCommand>
{
    public UpdateChecklistItemReturnCommandValidator()
    {
        RuleFor(x => x.ItemId).NotEmpty();
        RuleFor(x => x.QuantityReturned).GreaterThanOrEqualTo(0);
    }
}

public class UpdateChecklistItemReturnCommandHandler : IRequestHandler<UpdateChecklistItemReturnCommand, Result>
{
    private readonly IApplicationDbContext _context;
    private readonly IAuditService _auditService;
    private readonly ICurrentUserService _currentUser;
    private readonly IChecklistHub _hub;

    public UpdateChecklistItemReturnCommandHandler(
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

    public async Task<Result> Handle(UpdateChecklistItemReturnCommand request, CancellationToken cancellationToken)
    {
        var item = await _context.ChecklistItems
            .FirstOrDefaultAsync(i => i.Id == request.ItemId, cancellationToken)
            ?? throw new NotFoundException("ChecklistItem", request.ItemId);

        var oldValue = item.QuantityReturned?.ToString() ?? "";
        item.UpdateReturn(request.QuantityReturned, _currentUser.UserId!);

        await _context.SaveChangesAsync(cancellationToken);

        await _auditService.LogAsync("UpdateReturn", "ChecklistItem", item.Id,
            oldValue: oldValue, newValue: request.QuantityReturned.ToString(), cancellationToken: cancellationToken);

        await _hub.NotifyItemUpdatedAsync(item.ChecklistId, item.Id, cancellationToken);

        return Result.Success();
    }
}
