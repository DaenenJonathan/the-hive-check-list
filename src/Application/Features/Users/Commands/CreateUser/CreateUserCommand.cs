using FluentValidation;
using MediatR;
using TheHive.Application.Common.Constants;
using TheHive.Application.Common.Interfaces;
using TheHive.Application.Common.Models;
using TheHive.Application.Features.Users.DTOs;

namespace TheHive.Application.Features.Users.Commands.CreateUser;

public record CreateUserCommand(
    string Email, string FirstName, string LastName, string Role, Guid? AgencyId, IReadOnlyList<Guid>? BrandIds = null
) : IRequest<Result<CreateUserResult>>;

public class CreateUserCommandValidator : AbstractValidator<CreateUserCommand>
{
    public CreateUserCommandValidator()
    {
        RuleFor(x => x.Email).NotEmpty().EmailAddress();
        RuleFor(x => x.FirstName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.LastName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Role).NotEmpty().Must(r => Roles.All.Contains(r))
            .WithMessage("Rôle invalide.");
        RuleFor(x => x.AgencyId).NotEmpty()
            .When(x => x.Role is "AgencyManager" or "Manager")
            .WithMessage("Une agence doit être sélectionnée pour ce rôle.");
        RuleFor(x => x.AgencyId).Null()
            .When(x => x.Role is not ("AgencyManager" or "Manager"))
            .WithMessage("Seuls un AgencyManager ou un Manager peuvent être rattachés à une agence.");
        RuleFor(x => x.BrandIds).Must(b => b is { Count: > 0 })
            .When(x => x.Role == "Manager")
            .WithMessage("Au moins une marque doit être sélectionnée pour un Manager.");
        RuleFor(x => x.BrandIds).Must(b => b is null or { Count: 0 })
            .When(x => x.Role != "Manager")
            .WithMessage("Seul un Manager peut être rattaché à des marques.");
    }
}

public class CreateUserCommandHandler : IRequestHandler<CreateUserCommand, Result<CreateUserResult>>
{
    private readonly IUserAdminService _userAdminService;

    public CreateUserCommandHandler(IUserAdminService userAdminService) => _userAdminService = userAdminService;

    public Task<Result<CreateUserResult>> Handle(CreateUserCommand request, CancellationToken cancellationToken)
        => _userAdminService.CreateUserAsync(
            request.Email, request.FirstName, request.LastName, request.Role, request.AgencyId,
            request.BrandIds ?? [], cancellationToken);
}
