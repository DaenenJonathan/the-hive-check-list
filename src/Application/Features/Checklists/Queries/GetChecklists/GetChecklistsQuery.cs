using MediatR;
using Microsoft.EntityFrameworkCore;
using TheHive.Application.Common.Interfaces;
using TheHive.Application.Features.Checklists.DTOs;

namespace TheHive.Application.Features.Checklists.Queries.GetChecklists;

public record GetChecklistsQuery(Guid? BrandActionId = null) : IRequest<List<ChecklistDto>>;

public class GetChecklistsQueryHandler : IRequestHandler<GetChecklistsQuery, List<ChecklistDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public GetChecklistsQueryHandler(IApplicationDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<List<ChecklistDto>> Handle(GetChecklistsQuery request, CancellationToken cancellationToken)
    {
        var query = _context.Checklists
            .Include(c => c.BrandAction).ThenInclude(a => a!.Brand)
            .Include(c => c.Items)
            .AsQueryable();

        if (request.BrandActionId.HasValue)
            query = query.Where(c => c.BrandActionId == request.BrandActionId.Value);

        if (_currentUser.Role == "AgencyManager" && Guid.TryParse(_currentUser.AgencyId, out var agencyId))
            query = query.Where(c => c.BrandAction!.Brand!.AgencyId == agencyId);

        if (_currentUser.Role == "Manager")
            query = query.Where(c => _currentUser.BrandIds.Contains(c.BrandAction!.BrandId));

        return await query
            .OrderByDescending(c => c.CreatedAt)
            .Select(c => new ChecklistDto
            {
                Id = c.Id,
                Name = c.Name,
                Version = c.Version,
                Status = c.Status,
                ImportedAt = c.ImportedAt,
                SourceFileName = c.SourceFileName,
                EventDate = c.EventDate,
                BrandActionId = c.BrandActionId,
                BrandActionName = c.BrandAction != null ? c.BrandAction.Name : null,
                BrandActionAddress = c.BrandAction != null ? c.BrandAction.Address : null,
                BrandActionCity = c.BrandAction != null ? c.BrandAction.City : null,
                BrandActionStatus = c.BrandAction != null ? c.BrandAction.Status : Domain.Enums.ActionStatus.Planned,
                BrandActionPlannedDepartureTime = c.BrandAction != null ? c.BrandAction.PlannedDepartureTime : null,
                BrandActionPlannedReturnTime = c.BrandAction != null ? c.BrandAction.PlannedReturnTime : null,
                CreatedAt = c.CreatedAt,
                TotalItems = c.Items.Count,
                PreparedItems = c.Items.Count(i => i.Status == Domain.Enums.ChecklistItemStatus.Prepared)
            })
            .ToListAsync(cancellationToken);
    }
}
