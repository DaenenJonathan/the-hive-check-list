using MediatR;
using Microsoft.EntityFrameworkCore;
using TheHive.Application.Common.Exceptions;
using TheHive.Application.Common.Interfaces;
using TheHive.Application.Common.Models;

namespace TheHive.Application.Features.Brands.Commands.DeleteBrand;

public record DeleteBrandCommand(Guid Id) : IRequest<Result>;

public class DeleteBrandCommandHandler : IRequestHandler<DeleteBrandCommand, Result>
{
    private readonly IApplicationDbContext _context;
    private readonly IAuditService _auditService;

    public DeleteBrandCommandHandler(IApplicationDbContext context, IAuditService auditService)
    {
        _context = context;
        _auditService = auditService;
    }

    public async Task<Result> Handle(DeleteBrandCommand request, CancellationToken cancellationToken)
    {
        var brand = await _context.Brands
            .FirstOrDefaultAsync(b => b.Id == request.Id, cancellationToken)
            ?? throw new NotFoundException("Brand", request.Id);

        var hasActions = await _context.BrandActions.AnyAsync(a => a.BrandId == brand.Id, cancellationToken);
        if (hasActions)
            return Result.Failure("Impossible de supprimer une marque encore utilisée par des actions.");

        await _auditService.LogAsync("Delete", "Brand", brand.Id,
            oldValue: brand.Name, cancellationToken: cancellationToken);

        _context.Brands.Remove(brand);
        await _context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
