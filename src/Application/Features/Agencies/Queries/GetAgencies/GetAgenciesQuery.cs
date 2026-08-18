using MediatR;
using Microsoft.EntityFrameworkCore;
using TheHive.Application.Common.Interfaces;
using TheHive.Application.Features.Agencies.DTOs;

namespace TheHive.Application.Features.Agencies.Queries.GetAgencies;

public record GetAgenciesQuery : IRequest<List<AgencyDto>>;

public class GetAgenciesQueryHandler : IRequestHandler<GetAgenciesQuery, List<AgencyDto>>
{
    private readonly IApplicationDbContext _context;

    public GetAgenciesQueryHandler(IApplicationDbContext context) => _context = context;

    public async Task<List<AgencyDto>> Handle(GetAgenciesQuery request, CancellationToken cancellationToken)
    {
        return await _context.Agencies
            .OrderBy(a => a.Name)
            .Select(a => new AgencyDto
            {
                Id = a.Id,
                Name = a.Name,
                Color = a.Color,
                BrandCount = a.Brands.Count
            })
            .ToListAsync(cancellationToken);
    }
}
