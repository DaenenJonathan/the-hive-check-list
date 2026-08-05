using System.IO.Compression;
using System.Text.RegularExpressions;
using System.Xml.Linq;

namespace TheHive.Infrastructure.Excel;

// Modern Excel "Insert Picture in Cell" (Rich Data Types / "_localImage") stores images very
// differently from the classic floating DrawingML pictures that ClosedXML's IXLWorksheet.Pictures
// reads: the cell's cached value is an error placeholder ("#VALUE!") plus a "vm" attribute pointing
// into xl/metadata.xml, which chains through xl/richData/*.xml to a relationship id, which resolves
// to the actual xl/media/imageN.png part. ClosedXML has no support for this chain, so it's resolved
// here directly against the raw OOXML zip/XML, independently of ClosedXML's own read of the same bytes.
public static class ExcelEmbeddedImageReader
{
    private static readonly XNamespace Main = "http://schemas.openxmlformats.org/spreadsheetml/2006/main";
    private static readonly XNamespace PackageRel = "http://schemas.openxmlformats.org/package/2006/relationships";
    private static readonly XNamespace DocRel = "http://schemas.openxmlformats.org/officeDocument/2006/relationships";
    private static readonly XNamespace RichData = "http://schemas.microsoft.com/office/spreadsheetml/2017/richdata";
    private static readonly XNamespace RichValueRel = "http://schemas.microsoft.com/office/spreadsheetml/2022/richvaluerel";

    private static readonly Regex RowFromCellRef = new(@"[A-Z]+(\d+)", RegexOptions.Compiled);

    private static readonly string[] RequiredParts =
    [
        "xl/metadata.xml", "xl/richData/rdrichvalue.xml", "xl/richData/rdrichvaluestructure.xml",
        "xl/richData/richValueRel.xml", "xl/richData/_rels/richValueRel.xml.rels"
    ];

    public static IReadOnlyDictionary<int, byte[]> ReadImagesByRow(byte[] workbookBytes, string sheetName)
    {
        try
        {
            return ReadImagesByRowCore(workbookBytes, sheetName);
        }
        catch
        {
            // An unexpected/unsupported rich-data shape must never break the import - it just means
            // no embedded images are attached for this file.
            return new Dictionary<int, byte[]>();
        }
    }

    private static IReadOnlyDictionary<int, byte[]> ReadImagesByRowCore(byte[] workbookBytes, string sheetName)
    {
        using var archive = new ZipArchive(new MemoryStream(workbookBytes), ZipArchiveMode.Read);

        if (RequiredParts.Any(part => archive.GetEntry(part) is null))
            return new Dictionary<int, byte[]>();

        var sheetPath = ResolveSheetPath(archive, sheetName);
        if (sheetPath is null) return new Dictionary<int, byte[]>();

        var metadataIndexToRvIndex = ReadMetadataToRvIndex(archive);
        var rvIndexToImageIdentifier = ReadRvToImageIdentifier(archive);
        var relIdByImageIdentifier = ReadRichValueRelIds(archive);
        var mediaPathByRelId = ReadRelationshipTargets(archive);

        var imagesByRow = new Dictionary<int, byte[]>();
        var sheetDoc = LoadXml(archive, sheetPath);

        foreach (var cell in sheetDoc.Descendants(Main + "c"))
        {
            if (!int.TryParse((string?)cell.Attribute("vm"), out var vm)) continue;
            var cellRef = (string?)cell.Attribute("r");
            if (cellRef is null) continue;

            var metadataIndex = vm - 1;
            if (metadataIndex < 0 || metadataIndex >= metadataIndexToRvIndex.Count) continue;

            var rvIndex = metadataIndexToRvIndex[metadataIndex];
            if (rvIndex < 0 || !rvIndexToImageIdentifier.TryGetValue(rvIndex, out var imageIdentifier)) continue;
            if (imageIdentifier < 0 || imageIdentifier >= relIdByImageIdentifier.Count) continue;

            var relId = relIdByImageIdentifier[imageIdentifier];
            if (!mediaPathByRelId.TryGetValue(relId, out var mediaPath)) continue;

            var mediaEntry = archive.GetEntry(mediaPath);
            if (mediaEntry is null) continue;

            var rowMatch = RowFromCellRef.Match(cellRef);
            if (!rowMatch.Success) continue;

            using var entryStream = mediaEntry.Open();
            using var buffer = new MemoryStream();
            entryStream.CopyTo(buffer);
            imagesByRow[int.Parse(rowMatch.Groups[1].Value)] = buffer.ToArray();
        }

        return imagesByRow;
    }

    private static string? ResolveSheetPath(ZipArchive archive, string sheetName)
    {
        var workbookDoc = LoadXml(archive, "xl/workbook.xml");
        var rId = workbookDoc.Descendants(Main + "sheet")
            .FirstOrDefault(s => string.Equals((string?)s.Attribute("name"), sheetName, StringComparison.OrdinalIgnoreCase))
            ?.Attribute(DocRel + "id")?.Value;
        if (rId is null) return null;

        var relsDoc = LoadXml(archive, "xl/_rels/workbook.xml.rels");
        var target = relsDoc.Descendants(PackageRel + "Relationship")
            .FirstOrDefault(r => (string?)r.Attribute("Id") == rId)
            ?.Attribute("Target")?.Value;

        return target is null ? null : NormalizePath("xl/" + target.TrimStart('/'));
    }

    // metadataIndex (0-based, = vm - 1) -> index into rdrichvalue.xml's <rv> list, or -1 if unresolved.
    private static List<int> ReadMetadataToRvIndex(ZipArchive archive)
    {
        var doc = LoadXml(archive, "xl/metadata.xml");

        var metadataTypes = doc.Descendants(Main + "metadataType").ToList();
        var richValueTypeIndex = metadataTypes.FindIndex(t =>
            string.Equals((string?)t.Attribute("name"), "XLRICHVALUE", StringComparison.OrdinalIgnoreCase));
        if (richValueTypeIndex < 0) return [];

        var richValueTypeId = (richValueTypeIndex + 1).ToString(); // metadataType index is 1-based in <rc t="...">

        return doc.Descendants(Main + "valueMetadata").Elements(Main + "bk")
            .Select(bk =>
            {
                var rc = bk.Elements(Main + "rc").FirstOrDefault(e => (string?)e.Attribute("t") == richValueTypeId);
                return rc is null ? -1 : (int?)rc.Attribute("v") ?? -1;
            })
            .ToList();
    }

    // rvIndex (0-based, document order in rdrichvalue.xml) -> LocalImageIdentifier, only for rv entries
    // whose structure actually carries a "_rvRel:LocalImageIdentifier" key (i.e. is an image).
    private static Dictionary<int, int> ReadRvToImageIdentifier(ZipArchive archive)
    {
        var structureDoc = LoadXml(archive, "xl/richData/rdrichvaluestructure.xml");
        var keyPositionByStructure = structureDoc.Root!.Elements(RichData + "s")
            .Select(s => s.Elements(RichData + "k").ToList().FindIndex(k => (string?)k.Attribute("n") == "_rvRel:LocalImageIdentifier"))
            .ToList();

        var valueDoc = LoadXml(archive, "xl/richData/rdrichvalue.xml");
        var result = new Dictionary<int, int>();

        var rvIndex = 0;
        foreach (var rv in valueDoc.Root!.Elements(RichData + "rv"))
        {
            var structureIndex = (int?)rv.Attribute("s") ?? -1;
            if (structureIndex >= 0 && structureIndex < keyPositionByStructure.Count)
            {
                var position = keyPositionByStructure[structureIndex];
                var values = rv.Elements(RichData + "v").ToList();
                if (position >= 0 && position < values.Count && int.TryParse(values[position].Value, out var identifier))
                    result[rvIndex] = identifier;
            }

            rvIndex++;
        }

        return result;
    }

    // 0-based index (matching LocalImageIdentifier) -> relationship id.
    private static List<string> ReadRichValueRelIds(ZipArchive archive)
    {
        var doc = LoadXml(archive, "xl/richData/richValueRel.xml");
        return doc.Root!.Elements(RichValueRel + "rel")
            .Select(e => (string?)e.Attribute(DocRel + "id") ?? string.Empty)
            .ToList();
    }

    private static Dictionary<string, string> ReadRelationshipTargets(ZipArchive archive)
    {
        var doc = LoadXml(archive, "xl/richData/_rels/richValueRel.xml.rels");
        var result = new Dictionary<string, string>();

        foreach (var rel in doc.Root!.Elements(PackageRel + "Relationship"))
        {
            var id = (string?)rel.Attribute("Id");
            var target = (string?)rel.Attribute("Target");
            if (id is null || target is null) continue;

            // Targets are relative to xl/richData/, e.g. "../media/image1.png" -> "xl/media/image1.png".
            result[id] = NormalizePath("xl/richData/" + target);
        }

        return result;
    }

    private static string NormalizePath(string path)
    {
        var stack = new Stack<string>();
        foreach (var part in path.Split('/'))
        {
            if (part == "..") { if (stack.Count > 0) stack.Pop(); }
            else if (part.Length > 0 && part != ".") stack.Push(part);
        }

        return string.Join("/", stack.Reverse());
    }

    private static XDocument LoadXml(ZipArchive archive, string entryPath)
    {
        var entry = archive.GetEntry(entryPath) ?? throw new FileNotFoundException(entryPath);
        using var stream = entry.Open();
        return XDocument.Load(stream);
    }
}
