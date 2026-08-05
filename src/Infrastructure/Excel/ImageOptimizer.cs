using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Processing;

namespace TheHive.Infrastructure.Excel;

// Product photos embedded in these Excel checklists are often multi-megabyte PNGs. Resizing and
// re-encoding to JPEG before storage keeps the app's image storage and page load times reasonable.
public static class ImageOptimizer
{
    private const int MaxDimension = 1024;
    private const int JpegQuality = 80;

    public static byte[] Optimize(byte[] originalBytes)
    {
        using var image = Image.Load(originalBytes);

        if (image.Width > MaxDimension || image.Height > MaxDimension)
        {
            image.Mutate(x => x.Resize(new ResizeOptions
            {
                Mode = ResizeMode.Max,
                Size = new Size(MaxDimension, MaxDimension)
            }));
        }

        using var output = new MemoryStream();
        image.Save(output, new JpegEncoder { Quality = JpegQuality });
        return output.ToArray();
    }
}
