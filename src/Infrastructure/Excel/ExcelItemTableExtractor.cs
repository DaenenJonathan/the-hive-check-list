using System.Text;
using ClosedXML.Excel;
using TheHive.Application.Features.ExcelImports.DTOs;
using TheHive.Infrastructure.Excel.Synonyms;

namespace TheHive.Infrastructure.Excel;

// Finds the item table by scoring rows against known column-header synonyms rather than assuming a
// fixed start row/columns. The Butik template repeats the header row before every category block, so
// the column mapping is re-detected every time a qualifying header row is seen.
public static class ExcelItemTableExtractor
{
    private static readonly HashSet<string> FooterKeywords = new(StringComparer.OrdinalIgnoreCase)
    {
        "WAREHOUSE", "PREPARATION :", "RETOUR :", "EXTRA INFO",
        "Upon collection", "The receipt", "The organisation", "Materials are",
        "WAREHOUSE PREPARATION", "SIGANTURE", "SIGNATURE FOR", "NAME RESPONSIBLE"
    };

    // Text that appears in PICTURE cells when there is no embedded image.
    private static readonly HashSet<string> ImagePlaceholders = new(StringComparer.OrdinalIgnoreCase)
    {
        "no image", "n/a", "pas de photo", "no photo", "pas d'image", "aucune image",
        "no picture", "pas de picture", "image", "photo", "-", "/"
    };

    public static void Extract(IXLWorksheet sheet, ExcelImportPreviewDto dto, IReadOnlyDictionary<int, byte[]> imagesByRow)
    {
        Dictionary<ItemColumn, int>? currentMapping = null;
        var currentCategory = string.Empty;
        var sortOrder = 0;
        var mappingStartRow = -1;

        foreach (var row in sheet.RowsUsed())
        {
            var cells = row.CellsUsed()
                .Select(c => (Column: c.Address.ColumnNumber, Text: GetText(c.Value)))
                .Where(c => !string.IsNullOrEmpty(c.Text))
                .ToList();

            if (cells.Count == 0) continue;

            // 1. Header row: re-detect the column mapping every time one appears.
            var matched = ExcelLabelMatcher.MatchColumnConcepts(cells);
            if (matched.ContainsKey(ItemColumn.Name) && matched.Count >= 2)
            {
                currentMapping = matched;
                mappingStartRow = mappingStartRow < 0 ? row.RowNumber() : mappingStartRow;
                continue;
            }

            // 2. Footer: stop the table entirely once we hit the legal/signature boilerplate.
            if (cells.Any(c => IsFooterText(c.Text)))
                break;

            // 3. Item row: only possible once a column mapping is known.
            if (currentMapping is { } mapping && mapping.TryGetValue(ItemColumn.Name, out var nameCol))
            {
                var nameText = GetCellText(cells, nameCol);
                if (!string.IsNullOrWhiteSpace(nameText))
                {
                    var qty = 0;
                    var hasQty = mapping.TryGetValue(ItemColumn.Units, out var unitsCol) &&
                                 QuantityParser.TryParse(row.Cell(unitsCol).Value, out qty) &&
                                 qty > 0;

                    if (hasQty)
                    {
                        var notes = mapping.TryGetValue(ItemColumn.Notes, out var notesCol)
                            ? GetText(row.Cell(notesCol).Value)
                            : null;

                        dto.Items.Add(new ExcelImportItemDto
                        {
                            MaterialName = nameText.Trim(),
                            QuantityRequested = qty,
                            Category = NullIfEmpty(currentCategory),
                            Notes = NullIfEmpty(notes),
                            SortOrder = sortOrder++,
                            ImageData = TryOptimizeImage(imagesByRow, row.RowNumber(), nameText.Trim(), dto)
                        });
                    }
                    else
                    {
                        dto.Warnings.Add($"Ligne {row.RowNumber()} : '{nameText.Trim()}' ignorée (quantité manquante ou invalide).");
                    }

                    continue;
                }
            }

            // 4. Category header row: exactly one meaningful cell (ignoring image placeholders).
            var meaningful = cells.Where(c => !IsImagePlaceholder(c.Text)).ToList();
            if (meaningful.Count == 1)
            {
                currentCategory = meaningful[0].Text.Trim();
                continue;
            }

            // 5. Otherwise: blank/noise row, skip.
        }

        if (currentMapping is null)
        {
            var firstRow = sheet.FirstRowUsed()?.RowNumber() ?? 1;
            AddDiagnosticDump(sheet, dto, firstRow, "Aucune colonne 'NAME' reconnue dans le fichier.");
        }
        else if (dto.Items.Count == 0)
        {
            AddDiagnosticDump(sheet, dto, mappingStartRow, "Aucun item détecté après la ligne d'en-tête.");
        }
    }

    private static void AddDiagnosticDump(IXLWorksheet sheet, ExcelImportPreviewDto dto, int startRow, string message)
    {
        var lastRow = sheet.LastRowUsed()?.RowNumber() ?? startRow;
        var sample = new StringBuilder();
        sample.AppendLine(message);

        foreach (var row in sheet.RowsUsed())
        {
            if (row.RowNumber() < startRow || row.RowNumber() > Math.Min(startRow + 15, lastRow)) continue;

            var cellsText = row.CellsUsed()
                .Select(c => $"col{c.Address.ColumnNumber}='{GetText(c.Value)}'");
            sample.AppendLine($"  Ligne {row.RowNumber()}: {string.Join(" ", cellsText)}");
        }

        dto.Errors.Add(sample.ToString());
    }

    private static byte[]? TryOptimizeImage(IReadOnlyDictionary<int, byte[]> imagesByRow, int rowNumber, string itemName, ExcelImportPreviewDto dto)
    {
        if (!imagesByRow.TryGetValue(rowNumber, out var rawBytes)) return null;

        try
        {
            return ImageOptimizer.Optimize(rawBytes);
        }
        catch (Exception ex)
        {
            dto.Warnings.Add($"Image de '{itemName}' (ligne {rowNumber}) ignorée : {ex.Message}");
            return null;
        }
    }

    private static string GetCellText(List<(int Column, string Text)> cells, int column) =>
        cells.FirstOrDefault(c => c.Column == column).Text ?? string.Empty;

    private static bool IsFooterText(string text) =>
        FooterKeywords.Any(k => text.StartsWith(k, StringComparison.OrdinalIgnoreCase));

    private static bool IsImagePlaceholder(string text)
    {
        var trimmed = text.Trim();
        if (ImagePlaceholders.Contains(trimmed)) return true;
        return trimmed.StartsWith("no image", StringComparison.OrdinalIgnoreCase)
            || trimmed.StartsWith("pas de", StringComparison.OrdinalIgnoreCase)
            || trimmed.StartsWith("aucun", StringComparison.OrdinalIgnoreCase);
    }

    private static string GetText(XLCellValue value)
    {
        if (value.IsBlank || value.IsError) return string.Empty;
        return value.ToString()?.Trim() ?? string.Empty;
    }

    private static string? NullIfEmpty(string? s) => string.IsNullOrWhiteSpace(s) ? null : s;
}
