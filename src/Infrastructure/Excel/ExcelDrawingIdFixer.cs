using System.IO.Compression;
using System.Text.RegularExpressions;

namespace TheHive.Infrastructure.Excel;

// ClosedXML requires every <xdr:cNvPr id="…"> in the workbook's drawing parts to be unique and throws
// ArgumentException ("The picture ID 'X' already exists") the moment it finds a duplicate. Real-world
// files built by repeatedly pasting images (rather than inserting them one at a time) commonly leave
// every picture's id at 0 - Excel itself doesn't care, but ClosedXML does. Renumbering them to be
// unique before handing the bytes to ClosedXML keeps those files importable without patching the library.
public static class ExcelDrawingIdFixer
{
    private static readonly Regex DrawingPartName = new(@"^xl/drawings/drawing\d+\.xml$", RegexOptions.Compiled);
    private static readonly Regex CNvPrId = new(@"(<xdr:cNvPr\b[^>]*\bid="")(\d+)("")", RegexOptions.Compiled);

    public static byte[] EnsureUniquePictureIds(byte[] originalBytes)
    {
        using var input = new MemoryStream(originalBytes);
        var rewritten = new Dictionary<string, string>();
        var nextId = 1;

        using (var archive = new ZipArchive(input, ZipArchiveMode.Read, leaveOpen: true))
        {
            foreach (var entry in archive.Entries.Where(e => DrawingPartName.IsMatch(e.FullName)))
            {
                using var reader = new StreamReader(entry.Open());
                var xml = reader.ReadToEnd();
                rewritten[entry.FullName] = CNvPrId.Replace(xml, m => $"{m.Groups[1].Value}{nextId++}{m.Groups[3].Value}");
            }
        }

        if (rewritten.Count == 0) return originalBytes;

        input.Position = 0;
        using var output = new MemoryStream();
        using (var sourceArchive = new ZipArchive(input, ZipArchiveMode.Read, leaveOpen: true))
        using (var outArchive = new ZipArchive(output, ZipArchiveMode.Create, leaveOpen: true))
        {
            foreach (var sourceEntry in sourceArchive.Entries)
            {
                var newEntry = outArchive.CreateEntry(sourceEntry.FullName, CompressionLevel.Optimal);
                using var newEntryStream = newEntry.Open();
                if (rewritten.TryGetValue(sourceEntry.FullName, out var newXml))
                {
                    using var writer = new StreamWriter(newEntryStream);
                    writer.Write(newXml);
                }
                else
                {
                    using var sourceStream = sourceEntry.Open();
                    sourceStream.CopyTo(newEntryStream);
                }
            }
        }

        return output.ToArray();
    }
}
