using FluentValidation;
using MediatR;
using TheHive.Application.Common.Interfaces;
using TheHive.Application.Common.Models;
using TheHive.Domain.Entities;

namespace TheHive.Application.Features.Agencies.Commands.CreateAgency;

public record CreateAgencyCommand(string Name, string Color) : IRequest<Result<Guid>>;

public class CreateAgencyCommandValidator : AbstractValidator<CreateAgencyCommand>
{
    public CreateAgencyCommandValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Color).NotEmpty().Matches("^#[0-9A-Fa-f]{6}$");
    }
}

public class CreateAgencyCommandHandler : IRequestHandler<CreateAgencyCommand, Result<Guid>>
{
    private readonly IApplicationDbContext _context;
    private readonly IAuditService _auditService;
    private readonly ICurrentUserService _currentUser;

    public CreateAgencyCommandHandler(IApplicationDbContext context, IAuditService auditService, ICurrentUserService currentUser)
    {
        _context = context;
        _auditService = auditService;
        _currentUser = currentUser;
    }

    public async Task<Result<Guid>> Handle(CreateAgencyCommand request, CancellationToken cancellationToken)
    {
        var agency = Agency.Create(request.Name, request.Color);
        agency.SetCreated(_currentUser.UserId!);
        _context.Agencies.Add(agency);

        await _context.SaveChangesAsync(cancellationToken);

        await _auditService.LogAsync("Create", "Agency", agency.Id,
            newValue: request.Name, cancellationToken: cancellationToken);

        return Result<Guid>.Success(agency.Id);
    }
}
