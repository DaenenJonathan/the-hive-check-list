using Microsoft.Extensions.Configuration;
using TheHive.Application.Common.Interfaces;

namespace TheHive.Infrastructure.Storage;

public class LocalImageStorageService : IImageStorageService
{
    private static readonly HashSet<string> AllowedExtensions = new(StringComparer.OrdinalIgnoreCase)
        { ".jpg", ".jpeg", ".png", ".gif", ".webp" };

    private readonly string _basePath;

    public LocalImageStorageService(IConfiguration configuration)
    {
        _basePath = configuration["Storage:ImagesPath"]
            ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "items");
        Directory.CreateDirectory(_basePath);
    }

    public async Task<string> SaveAsync(Stream imageStream, string originalFileName, CancellationToken cancellationToken = default)
    {
        var ext = Path.GetExtension(originalFileName).ToLowerInvariant();
        if (!AllowedExtensions.Contains(ext))
            ext = ".jpg";

        var fileName = $"{Guid.NewGuid()}{ext}";
        var fullPath = Path.Combine(_basePath, fileName);

        await using var fileStream = File.Create(fullPath);
        await imageStream.CopyToAsync(fileStream, cancellationToken);

        return $"images/items/{fileName}";
    }

    public Task DeleteAsync(string imagePath, CancellationToken cancellationToken = default)
    {
        var fullPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", imagePath);
        if (File.Exists(fullPath))
            File.Delete(fullPath);
        return Task.CompletedTask;
    }
}
