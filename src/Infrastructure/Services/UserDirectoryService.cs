using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using TheHive.Application.Common.Interfaces;
using TheHive.Infrastructure.Identity;

namespace TheHive.Infrastructure.Services;

public class UserDirectoryService : IUserDirectoryService
{
    private readonly UserManager<AppUser> _userManager;

    public UserDirectoryService(UserManager<AppUser> userManager) => _userManager = userManager;

    public async Task<List<string>> GetUserIdsByRoleAsync(string role, CancellationToken cancellationToken = default)
    {
        return await _userManager.Users
            .Where(u => u.Role == role)
            .Select(u => u.Id)
            .ToListAsync(cancellationToken);
    }
}
