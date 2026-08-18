using MediatR;
using Microsoft.EntityFrameworkCore;
using TheHive.Application.Common.Exceptions;
using TheHive.Application.Common.Interfaces;
using TheHive.Application.Common.Models;

namespace TheHive.Application.Features.Agencies.Commands.DeleteAgency;

public record DeleteAgencyCommand(Guid Id) : IRequest<Result>;

public class DeleteAgencyCommandHandler : IRequestHandler<DeleteAgencyCommand, Result>
{
    private readonly IApplicationDbContext _context;
    private readonly IAuditService _auditService;

    public DeleteAgencyCommandHandler(IApplicationDbContext context, IAuditService auditService)
    {
        _context = context;
        _auditService = auditService;
    }

    public async Task<Result> Handle(DeleteAgencyCommand request, CancellationToken cancellationToken)
    {
        var agency = await _context.Agencies
            .Include(a => a.Brands)
            .FirstOrDefaultAsync(a => a.Id == request.Id, cancellationToken)
            ?? throw new NotFoundException("Agency", request.Id);

        if (agency.Brands.Count > 0)
            return Result.Failure("Impossible de supprimer une agence qui possède encore des marques.");

        await _auditService.LogAsync("Delete", "Agency", agency.Id,
            oldValue: agency.Name, cancellationToken: cancellationToken);

        _context.Agencies.Remove(agency);
        await _context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
