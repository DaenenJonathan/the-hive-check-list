using System.IO.Compression;
using System.Text;
using FluentAssertions;
using TheHive.Infrastructure.Excel;

namespace TheHive.Infrastructure.Tests.Excel;

public class ExcelDrawingIdFixerTests
{
    [Fact]
    public void EnsureUniquePictureIds_renumbers_duplicate_ids_within_a_drawing_part()
    {
        var drawingXml = """
            <xdr:wsDr xmlns:xdr="http://schemas.openxmlformats.org/drawingml/2006/spreadsheetDrawing">
              <xdr:pic><xdr:nvPicPr><xdr:cNvPr id="0" name="image1.png" /></xdr:nvPicPr></xdr:pic>
              <xdr:pic><xdr:nvPicPr><xdr:cNvPr id="0" name="image2.png" /></xdr:nvPicPr></xdr:pic>
              <xdr:pic><xdr:nvPicPr><xdr:cNvPr id="0" name="image3.png" /></xdr:nvPicPr></xdr:pic>
            </xdr:wsDr>
            """;
        var original = BuildZipWithEntry("xl/drawings/drawing1.xml", drawingXml);

        var fixedBytes = ExcelDrawingIdFixer.EnsureUniquePictureIds(original);

        var ids = ReadEntry(fixedBytes, "xl/drawings/drawing1.xml")
            .Split('\n')
            .Where(l => l.Contains("cNvPr"))
            .Select(l => System.Text.RegularExpressions.Regex.Match(l, "id=\"(\\d+)\"").Groups[1].Value)
            .ToList();

        ids.Should().OnlyHaveUniqueItems();
        ids.Should().HaveCount(3);
    }

    [Fact]
    public void EnsureUniquePictureIds_renumbers_across_multiple_drawing_parts_too()
    {
        using var input = new MemoryStream();
        using (var archive = new ZipArchive(input, ZipArchiveMode.Create, leaveOpen: true))
        {
            WriteEntry(archive, "xl/drawings/drawing1.xml",
                """<xdr:wsDr xmlns:xdr="ns"><xdr:pic><xdr:nvPicPr><xdr:cNvPr id="0" name="a.png" /></xdr:nvPicPr></xdr:pic></xdr:wsDr>""");
            WriteEntry(archive, "xl/drawings/drawing2.xml",
                """<xdr:wsDr xmlns:xdr="ns"><xdr:pic><xdr:nvPicPr><xdr:cNvPr id="0" name="b.png" /></xdr:nvPicPr></xdr:pic></xdr:wsDr>""");
        }

        var fixedBytes = ExcelDrawingIdFixer.EnsureUniquePictureIds(input.ToArray());

        var id1 = System.Text.RegularExpressions.Regex.Match(ReadEntry(fixedBytes, "xl/drawings/drawing1.xml"), "id=\"(\\d+)\"").Groups[1].Value;
        var id2 = System.Text.RegularExpressions.Regex.Match(ReadEntry(fixedBytes, "xl/drawings/drawing2.xml"), "id=\"(\\d+)\"").Groups[1].Value;

        id1.Should().NotBe(id2);
    }

    [Fact]
    public void EnsureUniquePictureIds_leaves_files_without_drawings_untouched()
    {
        var original = BuildZipWithEntry("xl/worksheets/sheet1.xml", "<worksheet />");

        var result = ExcelDrawingIdFixer.EnsureUniquePictureIds(original);

        result.Should().BeEquivalentTo(original);
    }

    private static byte[] BuildZipWithEntry(string entryName, string content)
    {
        using var stream = new MemoryStream();
        using (var archive = new ZipArchive(stream, ZipArchiveMode.Create, leaveOpen: true))
            WriteEntry(archive, entryName, content);
        return stream.ToArray();
    }

    private static void WriteEntry(ZipArchive archive, string entryName, string content)
    {
        var entry = archive.CreateEntry(entryName);
        using var writer = new StreamWriter(entry.Open(), Encoding.UTF8);
        writer.Write(content);
    }

    private static string ReadEntry(byte[] zipBytes, string entryName)
    {
        using var stream = new MemoryStream(zipBytes);
        using var archive = new ZipArchive(stream, ZipArchiveMode.Read);
        using var reader = new StreamReader(archive.GetEntry(entryName)!.Open());
        return reader.ReadToEnd();
    }
}
