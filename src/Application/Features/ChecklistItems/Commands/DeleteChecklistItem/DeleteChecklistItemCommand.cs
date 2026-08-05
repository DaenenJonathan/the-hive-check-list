using MediatR;
using Microsoft.EntityFrameworkCore;
using TheHive.Application.Common.Exceptions;
using TheHive.Application.Common.Interfaces;
using TheHive.Application.Common.Models;

namespace TheHive.Application.Features.ChecklistItems.Commands.DeleteChecklistItem;

public record DeleteChecklistItemCommand(Guid ItemId) : IRequest<Result>;

public class DeleteChecklistItemCommandHandler : IRequestHandler<DeleteChecklistItemCommand, Result>
{
    private readonly IApplicationDbContext _context;
    private readonly IAuditService _auditService;
    private readonly ICurrentUserService _currentUser;

    public DeleteChecklistItemCommandHandler(IApplicationDbContext context, IAuditService auditService, ICurrentUserService currentUser)
    {
        _context = context;
        _auditService = auditService;
        _currentUser = currentUser;
    }

    public async Task<Result> Handle(DeleteChecklistItemCommand request, CancellationToken cancellationToken)
    {
        var item = await _context.ChecklistItems
            .FirstOrDefaultAsync(i => i.Id == request.ItemId, cancellationToken)
            ?? throw new NotFoundException("ChecklistItem", request.ItemId);

        await _auditService.LogAsync("DeleteItem", "ChecklistItem", item.Id,
            oldValue: item.MaterialName, cancellationToken: cancellationToken);

        _context.ChecklistItems.Remove(item);
        await _context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
