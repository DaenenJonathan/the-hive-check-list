using MediatR;
using Microsoft.EntityFrameworkCore;
using TheHive.Application.Common.Exceptions;
using TheHive.Application.Common.Interfaces;
using TheHive.Application.Features.Actions.DTOs;

namespace TheHive.Application.Features.Actions.Queries.GetActionReturns;

public record GetActionReturnsQuery(Guid ActionId) : IRequest<ActionReturnsDto>;

public class GetActionReturnsQueryHandler : IRequestHandler<GetActionReturnsQuery, ActionReturnsDto>
{
    private readonly IApplicationDbContext _context;

    public GetActionReturnsQueryHandler(IApplicationDbContext context) => _context = context;

    public async Task<ActionReturnsDto> Handle(GetActionReturnsQuery request, CancellationToken cancellationToken)
    {
        var action = await _context.BrandActions
            .FirstOrDefaultAsync(a => a.Id == request.ActionId, cancellationToken)
            ?? throw new NotFoundException("BrandAction", request.ActionId);

        var items = await _context.ChecklistItems
            .Include(i => i.Checklist)
            .Where(i => i.Checklist!.BrandActionId == request.ActionId)
            .OrderBy(i => i.Checklist!.Name).ThenBy(i => i.SortOrder)
            .Select(i => new ActionReturnItemDto
            {
                ItemId = i.Id,
                ChecklistId = i.ChecklistId,
                ChecklistName = i.Checklist!.Name,
                MaterialName = i.MaterialName,
                Category = i.Category,
                Location = i.Location,
                QuantityRequested = i.QuantityRequested,
                QuantityPrepared = i.QuantityPrepared,
                QuantityReturned = i.QuantityReturned
            })
            .ToListAsync(cancellationToken);

        return new ActionReturnsDto
        {
            ActionId = action.Id,
            Sent = action.Sent,
            SentAt = action.SentAt,
            ReturnValidated = action.ReturnValidated,
            ReturnValidatedAt = action.ReturnValidatedAt,
            Items = items
        };
    }
}
