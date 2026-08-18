using FluentValidation;
using MediatR;
using TheHive.Application.Common.Interfaces;
using TheHive.Application.Common.Models;

namespace TheHive.Application.Features.Users.Commands.DeleteUser;

public record DeleteUserCommand(string UserId) : IRequest<Result>;

public class DeleteUserCommandValidator : AbstractValidator<DeleteUserCommand>
{
    public DeleteUserCommandValidator()
    {
        RuleFor(x => x.UserId).NotEmpty();
    }
}

public class DeleteUserCommandHandler : IRequestHandler<DeleteUserCommand, Result>
{
    private readonly IUserAdminService _userAdminService;
    private readonly ICurrentUserService _currentUser;

    public DeleteUserCommandHandler(IUserAdminService userAdminService, ICurrentUserService currentUser)
    {
        _userAdminService = userAdminService;
        _currentUser = currentUser;
    }

    public Task<Result> Handle(DeleteUserCommand request, CancellationToken cancellationToken)
    {
        if (request.UserId == _currentUser.UserId)
            return Task.FromResult(Result.Failure("Vous ne pouvez pas supprimer votre propre compte."));

        return _userAdminService.DeleteUserAsync(request.UserId, cancellationToken);
    }
}
