using MediatR;
using Microsoft.EntityFrameworkCore;
using TheHive.Application.Common.Exceptions;
using TheHive.Application.Common.Interfaces;
using TheHive.Application.Common.Models;
using TheHive.Domain.Enums;

namespace TheHive.Application.Features.Actions.Commands.MarkActionAsSent;

public record MarkActionAsSentCommand(Guid ActionId) : IRequest<Result>;

public class MarkActionAsSentCommandHandler : IRequestHandler<MarkActionAsSentCommand, Result>
{
    private readonly IApplicationDbContext _context;
    private readonly IAuditService _auditService;
    private readonly ICurrentUserService _currentUser;

    public MarkActionAsSentCommandHandler(
        IApplicationDbContext context,
        IAuditService auditService,
        ICurrentUserService currentUser)
    {
        _context = context;
        _auditService = auditService;
        _currentUser = currentUser;
    }

    public async Task<Result> Handle(MarkActionAsSentCommand request, CancellationToken cancellationToken)
    {
        var action = await _context.BrandActions
            .Include(a => a.Checklists).ThenInclude(c => c.Items)
            .FirstOrDefaultAsync(a => a.Id == request.ActionId, cancellationToken)
            ?? throw new NotFoundException("BrandAction", request.ActionId);

        if (action.PlannedDate.Date > DateTime.UtcNow.Date)
            return Result.Failure("L'action ne peut être marquée comme envoyée qu'à partir de sa date prévue.");

        var items = action.Checklists.SelectMany(c => c.Items).ToList();
        if (items.Count == 0)
            return Result.Failure("Aucun item à envoyer pour cette action.");
        if (items.Any(i => i.Status == ChecklistItemStatus.ToPrepare))
            return Result.Failure("Tous les items doivent être traités (préparés, manquants, annulés, ...) avant l'envoi.");

        action.MarkAsSent(_currentUser.UserId!);
        await _context.SaveChangesAsync(cancellationToken);

        await _auditService.LogAsync("MarkSent", "BrandAction", action.Id,
            oldValue: "False", newValue: "True", cancellationToken: cancellationToken);

        return Result.Success();
    }
}
