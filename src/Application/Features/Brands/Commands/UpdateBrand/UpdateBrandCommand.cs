using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using TheHive.Application.Common.Exceptions;
using TheHive.Application.Common.Interfaces;
using TheHive.Application.Common.Models;

namespace TheHive.Application.Features.Brands.Commands.UpdateBrand;

public record UpdateBrandCommand(Guid Id, string Name, Guid AgencyId) : IRequest<Result>;

public class UpdateBrandCommandValidator : AbstractValidator<UpdateBrandCommand>
{
    public UpdateBrandCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.AgencyId).NotEmpty();
    }
}

public class UpdateBrandCommandHandler : IRequestHandler<UpdateBrandCommand, Result>
{
    private readonly IApplicationDbContext _context;
    private readonly IAuditService _auditService;
    private readonly ICurrentUserService _currentUser;

    public UpdateBrandCommandHandler(IApplicationDbContext context, IAuditService auditService, ICurrentUserService currentUser)
    {
        _context = context;
        _auditService = auditService;
        _currentUser = currentUser;
    }

    public async Task<Result> Handle(UpdateBrandCommand request, CancellationToken cancellationToken)
    {
        var brand = await _context.Brands
            .FirstOrDefaultAsync(b => b.Id == request.Id, cancellationToken)
            ?? throw new NotFoundException("Brand", request.Id);

        var agencyExists = await _context.Agencies.AnyAsync(a => a.Id == request.AgencyId, cancellationToken);
        if (!agencyExists)
            throw new NotFoundException("Agency", request.AgencyId);

        var oldName = brand.Name;
        brand.Rename(request.Name);
        brand.SetUpdated(_currentUser.UserId!);

        await _context.SaveChangesAsync(cancellationToken);

        await _auditService.LogAsync("Update", "Brand", brand.Id,
            oldValue: oldName, newValue: request.Name, cancellationToken: cancellationToken);

        return Result.Success();
    }
}
