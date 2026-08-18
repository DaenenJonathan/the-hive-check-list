using TheHive.Application.Common.Models;
using TheHive.Application.Features.Users.DTOs;

namespace TheHive.Application.Common.Interfaces;

public interface IUserAdminService
{
    Task<List<UserDto>> GetUsersAsync(CancellationToken cancellationToken = default);

    Task<Result> UpdateUserRoleAsync(string userId, string role, Guid? agencyId, IReadOnlyList<Guid> brandIds, CancellationToken cancellationToken = default);

    Task<Result<CreateUserResult>> CreateUserAsync(string email, string firstName, string lastName, string role, Guid? agencyId, IReadOnlyList<Guid> brandIds, CancellationToken cancellationToken = default);

    Task<Result<string>> ResetPasswordAsync(string userId, CancellationToken cancellationToken = default);
}
