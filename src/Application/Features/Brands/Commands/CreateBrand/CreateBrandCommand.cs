using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using TheHive.Application.Common.Exceptions;
using TheHive.Application.Common.Interfaces;
using TheHive.Application.Common.Models;
using TheHive.Application.Common.Security;
using TheHive.Domain.Entities;

namespace TheHive.Application.Features.Brands.Commands.CreateBrand;

public record CreateBrandCommand(string Name, Guid AgencyId) : IRequest<Result<Guid>>;

public class CreateBrandCommandValidator : AbstractValidator<CreateBrandCommand>
{
    public CreateBrandCommandValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.AgencyId).NotEmpty();
    }
}

public class CreateBrandCommandHandler : IRequestHandler<CreateBrandCommand, Result<Guid>>
{
    private readonly IApplicationDbContext _context;
    private readonly IAuditService _auditService;
    private readonly ICurrentUserService _currentUser;

    public CreateBrandCommandHandler(IApplicationDbContext context, IAuditService auditService, ICurrentUserService currentUser)
    {
        _context = context;
        _auditService = auditService;
        _currentUser = currentUser;
    }

    public async Task<Result<Guid>> Handle(CreateBrandCommand request, CancellationToken cancellationToken)
    {
        var agencyExists = await _context.Agencies.AnyAsync(a => a.Id == request.AgencyId, cancellationToken);
        if (!agencyExists)
            throw new NotFoundException("Agency", request.AgencyId);

        AgencyAccessGuard.EnsureCanAccessAgency(_currentUser, request.AgencyId);

        var brand = Brand.Create(request.Name, request.AgencyId);
        brand.SetCreated(_currentUser.UserId!);
        _context.Brands.Add(brand);

        await _context.SaveChangesAsync(cancellationToken);

        await _auditService.LogAsync("Create", "Brand", brand.Id,
            newValue: request.Name, cancellationToken: cancellationToken);

        return Result<Guid>.Success(brand.Id);
    }
}
