using Microsoft.Extensions.Configuration;
using TheHive.Application.Common.Interfaces;
using TheHive.Infrastructure.Excel;

namespace TheHive.Infrastructure.Storage;

public class LocalImageStorageService : IImageStorageService
{
    private readonly string _basePath;

    public LocalImageStorageService(IConfiguration configuration)
    {
        _basePath = configuration["Storage:ImagesPath"]
            ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "items");
        Directory.CreateDirectory(_basePath);
    }

    // Every image reaching this service (manual upload or already-optimized Excel import
    // bytes) is re-encoded here, so storage size stays bounded regardless of the source.
    public async Task<string> SaveAsync(Stream imageStream, string originalFileName, CancellationToken cancellationToken = default)
    {
        using var buffer = new MemoryStream();
        await imageStream.CopyToAsync(buffer, cancellationToken);
        var optimized = ImageOptimizer.Optimize(buffer.ToArray());

        var fileName = $"{Guid.NewGuid()}.jpg";
        var fullPath = Path.Combine(_basePath, fileName);
        await File.WriteAllBytesAsync(fullPath, optimized, cancellationToken);

        return $"images/items/{fileName}";
    }

    // Used when cloning a checklist item (e.g. creating an action from a template): each
    // item must own its own file so deleting/replacing one image never affects the other.
    public Task<string?> CopyAsync(string sourceImagePath, CancellationToken cancellationToken = default)
    {
        var sourceFullPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", sourceImagePath);
        if (!File.Exists(sourceFullPath))
            return Task.FromResult<string?>(null);

        var fileName = $"{Guid.NewGuid()}.jpg";
        var destFullPath = Path.Combine(_basePath, fileName);
        File.Copy(sourceFullPath, destFullPath);

        return Task.FromResult<string?>($"images/items/{fileName}");
    }

    public Task DeleteAsync(string imagePath, CancellationToken cancellationToken = default)
    {
        var fullPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", imagePath);
        if (File.Exists(fullPath))
            File.Delete(fullPath);
        return Task.CompletedTask;
    }
}
