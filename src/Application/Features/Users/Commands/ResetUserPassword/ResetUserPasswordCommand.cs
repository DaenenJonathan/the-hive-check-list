using FluentValidation;
using MediatR;
using TheHive.Application.Common.Interfaces;
using TheHive.Application.Common.Models;

namespace TheHive.Application.Features.Users.Commands.ResetUserPassword;

public record ResetUserPasswordCommand(string UserId) : IRequest<Result<string>>;

public class ResetUserPasswordCommandValidator : AbstractValidator<ResetUserPasswordCommand>
{
    public ResetUserPasswordCommandValidator()
    {
        RuleFor(x => x.UserId).NotEmpty();
    }
}

public class ResetUserPasswordCommandHandler : IRequestHandler<ResetUserPasswordCommand, Result<string>>
{
    private readonly IUserAdminService _userAdminService;

    public ResetUserPasswordCommandHandler(IUserAdminService userAdminService) => _userAdminService = userAdminService;

    public Task<Result<string>> Handle(ResetUserPasswordCommand request, CancellationToken cancellationToken)
        => _userAdminService.ResetPasswordAsync(request.UserId, cancellationToken);
}
