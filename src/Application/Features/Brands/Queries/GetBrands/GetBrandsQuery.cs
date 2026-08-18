using MediatR;
using Microsoft.EntityFrameworkCore;
using TheHive.Application.Common.Interfaces;
using TheHive.Application.Features.Brands.DTOs;

namespace TheHive.Application.Features.Brands.Queries.GetBrands;

public record GetBrandsQuery(Guid? AgencyId) : IRequest<List<BrandDto>>;

public class GetBrandsQueryHandler : IRequestHandler<GetBrandsQuery, List<BrandDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public GetBrandsQueryHandler(IApplicationDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<List<BrandDto>> Handle(GetBrandsQuery request, CancellationToken cancellationToken)
    {
        // An AgencyManager can only ever browse the brands of their own agency,
        // regardless of what AgencyId filter the caller passed in.
        var agencyId = request.AgencyId;
        if (_currentUser.Role == "AgencyManager")
            agencyId = Guid.TryParse(_currentUser.AgencyId, out var ownAgencyId) ? ownAgencyId : Guid.Empty;

        var query = _context.Brands.Include(b => b.Agency).AsQueryable();
        if (agencyId is { } id)
            query = query.Where(b => b.AgencyId == id);

        // A Manager can only ever browse the brands they've been assigned - same idea as the
        // AgencyManager narrowing above, so the create-Action brand dropdown is properly scoped.
        if (_currentUser.Role == "Manager")
            query = query.Where(b => _currentUser.BrandIds.Contains(b.Id));

        return await query
            .OrderBy(b => b.Name)
            .Select(b => new BrandDto
            {
                Id = b.Id,
                Name = b.Name,
                AgencyId = b.AgencyId,
                AgencyName = b.Agency!.Name,
                AgencyColor = b.Agency!.Color
            })
            .ToListAsync(cancellationToken);
    }
}
