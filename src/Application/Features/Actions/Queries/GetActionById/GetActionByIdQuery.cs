using MediatR;
using Microsoft.EntityFrameworkCore;
using TheHive.Application.Common.Exceptions;
using TheHive.Application.Common.Interfaces;
using TheHive.Application.Common.Security;
using TheHive.Application.Features.Actions.DTOs;
using TheHive.Domain.Enums;

namespace TheHive.Application.Features.Actions.Queries.GetActionById;

public record GetActionByIdQuery(Guid Id) : IRequest<ActionDto>;

public class GetActionByIdQueryHandler : IRequestHandler<GetActionByIdQuery, ActionDto>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public GetActionByIdQueryHandler(IApplicationDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<ActionDto> Handle(GetActionByIdQuery request, CancellationToken cancellationToken)
    {
        var action = await _context.BrandActions
            .Include(a => a.Brand).ThenInclude(b => b!.Agency)
            .Include(a => a.Checklists).ThenInclude(c => c.Items)
            .FirstOrDefaultAsync(a => a.Id == request.Id, cancellationToken)
            ?? throw new NotFoundException("BrandAction", request.Id);

        AgencyAccessGuard.EnsureCanAccessAgency(_currentUser, action.Brand?.AgencyId);
        BrandAccessGuard.EnsureCanAccessBrand(_currentUser, action.BrandId);

        return new ActionDto
        {
            Id = action.Id,
            Name = action.Name,
            BrandId = action.BrandId,
            BrandName = action.Brand!.Name,
            AgencyId = action.Brand!.AgencyId,
            AgencyName = action.Brand!.Agency!.Name,
            PlannedDate = action.PlannedDate,
            PlannedDepartureTime = action.PlannedDepartureTime,
            PlannedReturnTime = action.PlannedReturnTime,
            Status = action.Status,
            Description = action.Description,
            Address = action.Address,
            City = action.City,
            CreatedAt = action.CreatedAt,
            ChecklistCount = action.Checklists.Count,
            SingleChecklistId = action.Checklists.Count == 1 ? action.Checklists.Select(c => c.Id).First() : (Guid?)null,
            TotalItems = action.Checklists.SelectMany(c => c.Items).Count(),
            PreparedItems = action.Checklists.SelectMany(c => c.Items).Count(i => i.Status == ChecklistItemStatus.Prepared),
            Sent = action.Sent,
            SentAt = action.SentAt,
            IsReadyToSend = action.Checklists.SelectMany(c => c.Items).Any()
                && !action.Checklists.SelectMany(c => c.Items).Any(i => i.Status == ChecklistItemStatus.ToPrepare),
            ReturnValidated = action.ReturnValidated,
            ReturnValidatedAt = action.ReturnValidatedAt
        };
    }
}
