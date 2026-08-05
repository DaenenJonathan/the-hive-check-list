using ClosedXML.Excel;
using FluentAssertions;
using SixLabors.ImageSharp;
using TheHive.Infrastructure.Excel;

namespace TheHive.Infrastructure.Tests.Excel;

public class ExcelEmbeddedImageReaderTests
{
    private static byte[] ReadFixture(string fileName) =>
        File.ReadAllBytes(Path.Combine(AppContext.BaseDirectory, "Fixtures", fileName));

    [Fact]
    public void Zyla_resolves_every_linked_image_cell_to_valid_image_bytes()
    {
        var bytes = ReadFixture("Butik_Zyla.xlsm");

        var images = ExcelEmbeddedImageReader.ReadImagesByRow(bytes, "Material list");

        images.Should().HaveCount(18);
        images.Should().ContainKey(25); // "ZYLA POWER BOOST"

        // Every resolved entry must be real, decodable image data - not just arbitrary bytes.
        foreach (var (_, data) in images)
            Image.Identify(data).Should().NotBeNull();
    }

    [Fact]
    public void BasePhonebooth_only_resolves_the_two_rows_that_actually_have_a_linked_picture()
    {
        var bytes = ReadFixture("Butik_BasePhonebooth.xlsm");

        var images = ExcelEmbeddedImageReader.ReadImagesByRow(bytes, "Material list");

        // Rows 34/35 = "CLEANING SPRAY" / "CLEANING CLOTH", the only two picture cells in this file.
        images.Keys.Should().BeEquivalentTo([34, 35]);
    }

    [Fact]
    public void TelenetMixMatch_does_not_resolve_rows_with_no_picture_cell()
    {
        var bytes = ReadFixture("Butik_TelenetMixMatch.xlsm");

        var images = ExcelEmbeddedImageReader.ReadImagesByRow(bytes, "Material list");

        // Row 31 ("CTA PANEEL") and row 32 ("VERLENGKABEL") have a blank Picture cell in the real file.
        images.Should().NotContainKey(31);
        images.Should().NotContainKey(32);
        images.Should().ContainKey(27); // "MIX&MATCH stand"
    }

    [Fact]
    public void ReadImagesByRow_returns_empty_for_a_workbook_without_rich_data_images()
    {
        using var workbook = new XLWorkbook();
        workbook.AddWorksheet("Material list");
        using var stream = new MemoryStream();
        workbook.SaveAs(stream);

        var images = ExcelEmbeddedImageReader.ReadImagesByRow(stream.ToArray(), "Material list");

        images.Should().BeEmpty();
    }

    [Fact]
    public void ReadImagesByRow_returns_empty_for_garbage_bytes_instead_of_throwing()
    {
        var images = ExcelEmbeddedImageReader.ReadImagesByRow([1, 2, 3, 4], "Material list");

        images.Should().BeEmpty();
    }
}
