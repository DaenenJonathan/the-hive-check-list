using ClosedXML.Excel;
using FluentAssertions;
using SixLabors.ImageSharp;
using TheHive.Infrastructure.Excel;

namespace TheHive.Infrastructure.Tests.Excel;

public class ExcelFloatingPictureReaderTests
{
    private static byte[] ReadFixture(string fileName) =>
        File.ReadAllBytes(Path.Combine(AppContext.BaseDirectory, "Fixtures", fileName));

    [Fact]
    public void CanadaDry_resolves_every_classic_floating_picture_to_its_anchor_row()
    {
        var bytes = ReadFixture("Template_CanadaDry.xlsx");
        using var workbook = new XLWorkbook(new MemoryStream(bytes));
        var sheet = workbook.Worksheets.First();

        var images = ExcelFloatingPictureReader.ReadImagesByRow(sheet);

        // 11 distinct anchor rows: two of the file's 12 floating pictures share row 28/29 and
        // collapse to a single entry, matching how imagesByRow already works for in-cell images.
        images.Should().HaveCount(11);

        foreach (var (_, data) in images)
            Image.Identify(data).Should().NotBeNull();
    }

    [Fact]
    public void ReadImagesByRow_returns_empty_for_a_worksheet_without_floating_pictures()
    {
        using var workbook = new XLWorkbook();
        var sheet = workbook.AddWorksheet("Material list");

        var images = ExcelFloatingPictureReader.ReadImagesByRow(sheet);

        images.Should().BeEmpty();
    }
}
