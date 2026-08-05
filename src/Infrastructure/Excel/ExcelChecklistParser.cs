using ClosedXML.Excel;
using TheHive.Application.Common.Interfaces;
using TheHive.Application.Features.ExcelImports.DTOs;

namespace TheHive.Infrastructure.Excel;

public class ExcelChecklistParser : IExcelChecklistParser
{
    public async Task<ExcelImportPreviewDto> ParseAsync(Stream fileStream, string fileName, CancellationToken cancellationToken = default)
    {
        var result = new ExcelImportPreviewDto { FileName = fileName };

        try
        {
            // Buffered once so both ClosedXML and the raw OOXML zip/XML reader (for embedded
            // "image in cell" data, which ClosedXML doesn't support) can each read the bytes independently.
            using var buffer = new MemoryStream();
            await fileStream.CopyToAsync(buffer, cancellationToken);
            var bytes = buffer.ToArray();

            using var workbook = new XLWorkbook(new MemoryStream(bytes));

            var sheet = workbook.Worksheets
                .Where(w => w.Visibility == XLWorksheetVisibility.Visible)
                .FirstOrDefault(w => w.Name.Contains("Material", StringComparison.OrdinalIgnoreCase))
                ?? workbook.Worksheets.First(w => w.Visibility == XLWorksheetVisibility.Visible);

            ExcelMetadataExtractor.Extract(sheet, result);
            result.City = ExtractCity(result.AddressAction);

            var imagesByRow = ExcelEmbeddedImageReader.ReadImagesByRow(bytes, sheet.Name);
            ExcelItemTableExtractor.Extract(sheet, result, imagesByRow);

            result.SuggestedChecklistName = BuildChecklistName(result, fileName);
        }
        catch (Exception ex)
        {
            result.Errors.Add($"Erreur de lecture du fichier : {ex.Message}");
        }

        return result;
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
