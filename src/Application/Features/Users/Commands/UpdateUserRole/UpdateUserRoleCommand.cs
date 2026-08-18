using FluentValidation;
using MediatR;
using TheHive.Application.Common.Constants;
using TheHive.Application.Common.Interfaces;
using TheHive.Application.Common.Models;

namespace TheHive.Application.Features.Users.Commands.UpdateUserRole;

public record UpdateUserRoleCommand(string UserId, string Role, Guid? AgencyId, IReadOnlyList<Guid>? BrandIds = null) : IRequest<Result>;

public class UpdateUserRoleCommandValidator : AbstractValidator<UpdateUserRoleCommand>
{
    public UpdateUserRoleCommandValidator()
    {
        RuleFor(x => x.UserId).NotEmpty();
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

public class UpdateUserRoleCommandHandler : IRequestHandler<UpdateUserRoleCommand, Result>
{
    private readonly IUserAdminService _userAdminService;

    public UpdateUserRoleCommandHandler(IUserAdminService userAdminService) => _userAdminService = userAdminService;

    public Task<Result> Handle(UpdateUserRoleCommand request, CancellationToken cancellationToken)
        => _userAdminService.UpdateUserRoleAsync(request.UserId, request.Role, request.AgencyId, request.BrandIds ?? [], cancellationToken);
}
