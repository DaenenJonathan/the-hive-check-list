using ClosedXML.Excel;
using TheHive.Application.Common.Interfaces;
using TheHive.Application.Features.ExcelImports.DTOs;

namespace TheHive.Infrastructure.Excel;

public class ExcelChecklistParser : IExcelChecklistParser
{
    public async Task<ExcelImportPreviewDto> ParseAsync(Stream fileStream, string fileName, string? sheetName = null, CancellationToken cancellationToken = default)
    {
        var result = new ExcelImportPreviewDto { FileName = fileName };

        try
        {
            // Buffered once so both ClosedXML and the raw OOXML zip/XML reader (for embedded
            // "image in cell" data, which ClosedXML doesn't support) can each read the bytes independently.
            using var buffer = new MemoryStream();
            await fileStream.CopyToAsync(buffer, cancellationToken);
            var bytes = ExcelDrawingIdFixer.EnsureUniquePictureIds(buffer.ToArray());

            using var workbook = new XLWorkbook(new MemoryStream(bytes));
            var visibleSheets = workbook.Worksheets.Where(w => w.Visibility == XLWorksheetVisibility.Visible).ToList();

            // Some workbooks carry several sheets that each look like their own checklist (e.g. a
            // "BIG"/"SMALL" wave of the same event) alongside blank templates or info tabs. Count real
            // items per sheet so the caller can offer a picker instead of us silently guessing wrong.
            var itemCountBySheet = visibleSheets.ToDictionary(w => w.Name, CountItems);
            result.AvailableSheets = visibleSheets
                .Where(w => itemCountBySheet[w.Name] > 0)
                .Select(w => new ExcelSheetOptionDto(w.Name, itemCountBySheet[w.Name]))
                .ToList();

            // Item count is a poor "which sheet is real" signal on its own (a bundled EXAMPLE/tutorial
            // tab can easily out-count the genuine data), so beyond the "Material" name match, the
            // default is just the first non-empty sheet in tab order - a reasonable starting point that
            // the caller's sheet picker (AvailableSheets) lets the user override with one click.
            var sheet = (sheetName is not null ? visibleSheets.FirstOrDefault(w => w.Name == sheetName) : null)
                ?? visibleSheets.FirstOrDefault(w => w.Name.Contains("Material", StringComparison.OrdinalIgnoreCase))
                ?? visibleSheets.FirstOrDefault(w => itemCountBySheet[w.Name] > 0)
                ?? visibleSheets.First();
            result.SelectedSheetName = sheet.Name;

            ExcelMetadataExtractor.Extract(sheet, result);
            result.City = ExtractCity(result.AddressAction);

            var imagesByRow = MergeImagesByRow(
                ExcelEmbeddedImageReader.ReadImagesByRow(bytes, sheet.Name),
                ExcelFloatingPictureReader.ReadImagesByRow(sheet));
            ExcelItemTableExtractor.Extract(sheet, result, imagesByRow);

            result.SuggestedChecklistName = BuildChecklistName(result, fileName);
        }
        catch (Exception ex)
        {
            result.Errors.Add($"Erreur de lecture du fichier : {ex.Message}");
        }

        return result;
    }

    private static readonly IReadOnlyDictionary<int, byte[]> NoImages = new Dictionary<int, byte[]>();

    // Cheap dry run (no image extraction) just to know whether a sheet looks like a real checklist.
    private static int CountItems(IXLWorksheet sheet)
    {
        var scratch = new ExcelImportPreviewDto();
        ExcelItemTableExtractor.Extract(sheet, scratch, NoImages);
        return scratch.Items.Count;
    }

    // In-cell rich-data images take priority; floating pictures only fill rows that have none yet
    // (a row is never expected to carry both, but this keeps the merge deterministic if it ever does).
    private static IReadOnlyDictionary<int, byte[]> MergeImagesByRow(
        IReadOnlyDictionary<int, byte[]> inCellImagesByRow, IReadOnlyDictionary<int, byte[]> floatingImagesByRow)
    {
        var merged = new Dictionary<int, byte[]>(inCellImagesByRow);
        foreach (var (row, image) in floatingImagesByRow)
            merged.TryAdd(row, image);

        return merged;
    }

    // The city follows the 4-digit postal code in the "Adresse Action" field, e.g. "Rue Example 12, 1000 Bruxelles"
    private static string? ExtractCity(string? address)
    {
        if (string.IsNullOrWhiteSpace(address)) return null;

        var match = System.Text.RegularExpressions.Regex.Match(address, @"\b\d{4}\b\s+([^,\r\n]+)");
        if (!match.Success) return null;

        var city = match.Groups[1].Value.Trim();
        return string.IsNullOrWhiteSpace(city) ? null : city;
    }

    private static string BuildChecklistName(ExcelImportPreviewDto dto, string fileName)
    {
        var parts = new[] { dto.Brand, dto.ProjectName, dto.ActionType }
            .Where(p => !string.IsNullOrWhiteSpace(p));
        var name = string.Join(" - ", parts);
        return string.IsNullOrWhiteSpace(name) ? Path.GetFileNameWithoutExtension(fileName) : name;
    }
}
