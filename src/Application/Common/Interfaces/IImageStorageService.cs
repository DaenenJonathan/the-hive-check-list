namespace TheHive.Application.Common.Interfaces;

public interface IImageStorageService
{
    Task<string> SaveAsync(Stream imageStream, string originalFileName, CancellationToken cancellationToken = default);
    Task DeleteAsync(string imagePath, CancellationToken cancellationToken = default);
}
