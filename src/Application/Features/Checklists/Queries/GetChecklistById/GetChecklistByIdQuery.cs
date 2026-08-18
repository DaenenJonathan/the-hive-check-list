using MediatR;
using Microsoft.EntityFrameworkCore;
using TheHive.Application.Common.Exceptions;
using TheHive.Application.Common.Interfaces;
using TheHive.Application.Common.Security;
using TheHive.Application.Features.Checklists.DTOs;

namespace TheHive.Application.Features.Checklists.Queries.GetChecklistById;

public record GetChecklistByIdQuery(Guid Id) : IRequest<ChecklistDetailDto>;

public class GetChecklistByIdQueryHandler : IRequestHandler<GetChecklistByIdQuery, ChecklistDetailDto>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public GetChecklistByIdQueryHandler(IApplicationDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<ChecklistDetailDto> Handle(GetChecklistByIdQuery request, CancellationToken cancellationToken)
    {
        var checklist = await _context.Checklists
            .Include(c => c.BrandAction).ThenInclude(a => a!.Brand)
            .Include(c => c.Items)
            .FirstOrDefaultAsync(c => c.Id == request.Id, cancellationToken)
            ?? throw new NotFoundException("Checklist", request.Id);

        AgencyAccessGuard.EnsureCanAccessAgency(_currentUser, checklist.BrandAction?.Brand?.AgencyId);
        BrandAccessGuard.EnsureCanAccessBrand(_currentUser, checklist.BrandAction?.BrandId);

        return new ChecklistDetailDto
        {
            Id = checklist.Id,
            Name = checklist.Name,
            Version = checklist.Version,
            Status = checklist.Status,
            ImportedAt = checklist.ImportedAt,
            SourceFileName = checklist.SourceFileName,
            EventDate = checklist.EventDate,
            BrandActionId = checklist.BrandActionId,
            BrandActionName = checklist.BrandAction?.Name,
            BrandActionAddress = checklist.BrandAction?.Address,
            BrandActionCity = checklist.BrandAction?.City,
            BrandActionStatus = checklist.BrandAction?.Status ?? Domain.Enums.ActionStatus.Planned,
            CreatedAt = checklist.CreatedAt,
            TotalItems = checklist.Items.Count,
            PreparedItems = checklist.Items.Count(i => i.Status == Domain.Enums.ChecklistItemStatus.Prepared),
            Items = checklist.Items.OrderBy(i => i.SortOrder).Select(i => new ChecklistItemDto
            {
                Id = i.Id,
                MaterialName = i.MaterialName,
                QuantityRequested = i.QuantityRequested,
                QuantityPrepared = i.QuantityPrepared,
                QuantityReturned = i.QuantityReturned,
                Location = i.Location,
                Notes = i.Notes,
                ImagePath = i.ImagePath,
                Remark = i.Remark,
                Status = i.Status,
                Category = i.Category,
                SortOrder = i.SortOrder,
                UpdatedAt = i.UpdatedAt,
                UpdatedBy = i.UpdatedBy
            }).ToList()
        };
    }
}
