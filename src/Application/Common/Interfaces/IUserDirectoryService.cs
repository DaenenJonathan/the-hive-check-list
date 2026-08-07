namespace TheHive.Application.Common.Interfaces;

public interface IUserDirectoryService
{
    Task<List<string>> GetUserIdsByRoleAsync(string role, CancellationToken cancellationToken = default);
}
