using FluentAssertions;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.PixelFormats;
using TheHive.Infrastructure.Excel;

namespace TheHive.Infrastructure.Tests.Excel;

public class ImageOptimizerTests
{
    private static byte[] CreatePng(int width, int height)
    {
        using var image = new Image<Rgba32>(width, height);
        using var stream = new MemoryStream();
        image.Save(stream, new PngEncoder());
        return stream.ToArray();
    }

    [Fact]
    public void Optimize_shrinks_an_oversized_image_to_the_max_dimension()
    {
        var original = CreatePng(2000, 1500);

        var optimized = ImageOptimizer.Optimize(original);

        using var image = Image.Load(optimized);
        image.Width.Should().BeLessOrEqualTo(1024);
        image.Height.Should().BeLessOrEqualTo(1024);
    }

    [Fact]
    public void Optimize_leaves_dimensions_unchanged_when_already_within_bounds()
    {
        var original = CreatePng(200, 100);

        var optimized = ImageOptimizer.Optimize(original);

        using var image = Image.Load(optimized);
        image.Width.Should().Be(200);
        image.Height.Should().Be(100);
    }

    [Fact]
    public void Optimize_always_outputs_jpeg()
    {
        var original = CreatePng(200, 100);

        var optimized = ImageOptimizer.Optimize(original);

        var format = Image.DetectFormat(optimized);
        format.Name.Should().Be("JPEG");
    }

    [Fact]
    public void Optimize_reduces_file_size_of_a_large_image()
    {
        var original = CreatePng(2000, 1500);

        var optimized = ImageOptimizer.Optimize(original);

        optimized.Length.Should().BeLessThan(original.Length);
    }
}
