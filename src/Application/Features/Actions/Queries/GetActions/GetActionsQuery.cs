using MediatR;
using Microsoft.EntityFrameworkCore;
using TheHive.Application.Common.Interfaces;
using TheHive.Application.Features.Actions.DTOs;
using TheHive.Domain.Enums;

namespace TheHive.Application.Features.Actions.Queries.GetActions;

public record GetActionsQuery : IRequest<List<ActionDto>>;

public class GetActionsQueryHandler : IRequestHandler<GetActionsQuery, List<ActionDto>>
{
    private readonly IApplicationDbContext _context;

    public GetActionsQueryHandler(IApplicationDbContext context) => _context = context;

    public async Task<List<ActionDto>> Handle(GetActionsQuery request, CancellationToken cancellationToken)
    {
        return await _context.BrandActions
            .Include(a => a.Checklists).ThenInclude(c => c.Items)
            .OrderByDescending(a => a.PlannedDate)
            .Select(a => new ActionDto
            {
                Id = a.Id,
                Name = a.Name,
                Client = a.Client,
                PlannedDate = a.PlannedDate,
                PlannedDepartureTime = a.PlannedDepartureTime,
                PlannedReturnTime = a.PlannedReturnTime,
                Status = a.Status,
                Description = a.Description,
                Address = a.Address,
                City = a.City,
                CreatedAt = a.CreatedAt,
                ChecklistCount = a.Checklists.Count,
                TotalItems = a.Checklists.SelectMany(c => c.Items).Count(),
                PreparedItems = a.Checklists.SelectMany(c => c.Items).Count(i => i.Status == ChecklistItemStatus.Prepared),
                Sent = a.Sent,
                SentAt = a.SentAt,
                IsReadyToSend = a.Checklists.SelectMany(c => c.Items).Any()
                    && !a.Checklists.SelectMany(c => c.Items).Any(i => i.Status == ChecklistItemStatus.ToPrepare),
                ReturnValidated = a.ReturnValidated,
                ReturnValidatedAt = a.ReturnValidatedAt
            })
            .ToListAsync(cancellationToken);
    }
}
