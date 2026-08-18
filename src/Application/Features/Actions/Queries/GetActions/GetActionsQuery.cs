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
    private readonly ICurrentUserService _currentUser;

    public GetActionsQueryHandler(IApplicationDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<List<ActionDto>> Handle(GetActionsQuery request, CancellationToken cancellationToken)
    {
        var query = _context.BrandActions
            .Include(a => a.Brand).ThenInclude(b => b!.Agency)
            .Include(a => a.Checklists).ThenInclude(c => c.Items)
            .AsQueryable();

        if (_currentUser.Role == "AgencyManager" && Guid.TryParse(_currentUser.AgencyId, out var agencyId))
            query = query.Where(a => a.Brand!.AgencyId == agencyId);

        if (_currentUser.Role == "Manager")
            query = query.Where(a => _currentUser.BrandIds.Contains(a.BrandId));

        return await query
            .OrderByDescending(a => a.PlannedDate)
            .Select(a => new ActionDto
            {
                Id = a.Id,
                Name = a.Name,
                BrandId = a.BrandId,
                BrandName = a.Brand!.Name,
                AgencyId = a.Brand!.AgencyId,
                AgencyName = a.Brand!.Agency!.Name,
                PlannedDate = a.PlannedDate,
                PlannedDepartureTime = a.PlannedDepartureTime,
                PlannedReturnTime = a.PlannedReturnTime,
                Status = a.Status,
                Description = a.Description,
                Address = a.Address,
                City = a.City,
                CreatedAt = a.CreatedAt,
                ChecklistCount = a.Checklists.Count,
                SingleChecklistId = a.Checklists.Count == 1 ? a.Checklists.Select(c => c.Id).First() : (Guid?)null,
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
