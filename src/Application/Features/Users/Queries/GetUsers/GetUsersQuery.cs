using MediatR;
using TheHive.Application.Common.Interfaces;
using TheHive.Application.Features.Users.DTOs;

namespace TheHive.Application.Features.Users.Queries.GetUsers;

public record GetUsersQuery : IRequest<List<UserDto>>;

public class GetUsersQueryHandler : IRequestHandler<GetUsersQuery, List<UserDto>>
{
    private readonly IUserAdminService _userAdminService;

    public GetUsersQueryHandler(IUserAdminService userAdminService) => _userAdminService = userAdminService;

    public Task<List<UserDto>> Handle(GetUsersQuery request, CancellationToken cancellationToken)
        => _userAdminService.GetUsersAsync(cancellationToken);
}
