using ClosedXML.Excel;
using TheHive.Application.Features.ExcelImports.DTOs;
using TheHive.Infrastructure.Excel.Synonyms;

namespace TheHive.Infrastructure.Excel;

// Finds metadata by scanning for label cells matching known synonyms (see ExcelFieldSynonyms),
// then reading the value next to that label - rather than assuming fixed row/column positions.
// This tolerates the metadata block shifting by a row or two between template variants.
public static class ExcelMetadataExtractor
{
    public static void Extract(IXLWorksheet sheet, ExcelImportPreviewDto dto)
    {
        var found = new Dictionary<MetadataField, XLCellValue>();

        foreach (var row in sheet.RowsUsed())
        {
            foreach (var cell in row.CellsUsed())
            {
                var normalized = ExcelLabelMatcher.Normalize(GetText(cell.Value));
                if (ExcelLabelMatcher.MatchMetadataConcept(normalized) is not { } field || found.ContainsKey(field))
                    continue;

                // BUILD-UP/ARRIVAL and BREAK-DOWN/RETOUR each carry a date plus a start and end time
                // on the same row (see the NAME/DATE/START/END header above them) - not a single
                // adjacent value like the other metadata fields, so these two are read from the whole
                // row instead of FindValueForLabel: departure is the first time found, return the last.
                if (field is MetadataField.DepartureTime or MetadataField.ReturnTime)
                {
                    var time = field == MetadataField.DepartureTime ? FirstTimeInRow(row) : LastTimeInRow(row);
                    if (time is not { } resolvedTime) continue;

                    found[field] = cell.Value;
                    if (field == MetadataField.DepartureTime) dto.PlannedDepartureTime = resolvedTime;
                    else dto.PlannedReturnTime = resolvedTime;
                    continue;
                }

                var value = FindValueForLabel(sheet, cell.Address.RowNumber, cell.Address.ColumnNumber);
                if (value is { } resolved)
                    found[field] = resolved;
            }
        }

        dto.Client = GetString(found, MetadataField.Client);
        dto.Brand = GetString(found, MetadataField.Brand);
        dto.ProjectName = GetString(found, MetadataField.ProjectName);
        dto.ActionType = GetString(found, MetadataField.ActionType);
        dto.Account = NullIfEmpty(GetString(found, MetadataField.Account));
        dto.CostCode = NullIfEmpty(GetString(found, MetadataField.CostCode));
        dto.AddressPickUp = NullIfEmpty(GetString(found, MetadataField.AddressPickUp));
        dto.AddressAction = NullIfEmpty(GetString(found, MetadataField.AddressAction));

        if (found.TryGetValue(MetadataField.EventDate, out var dateValue))
        {
            if (dateValue.IsDateTime)
                dto.EventDate = dateValue.GetDateTime();
            else if (DateTime.TryParse(dateValue.ToString(), out var parsed))
                dto.EventDate = parsed;
        }

        foreach (var field in ExcelFieldSynonyms.MetadataLabels.Keys)
        {
            if (!found.ContainsKey(field))
                dto.Warnings.Add($"Champ '{field}' introuvable dans le fichier.");
        }
    }

    // Primary: first non-blank cell to the right on the same row, stopping if another known label
    // is crossed first (e.g. "ACCOUNT :" and "COST CODE :" share a row - the Account search must not
    // run past "COST CODE :" into its own value). Fallback: the cell directly below the label.
    private static XLCellValue? FindValueForLabel(IXLWorksheet sheet, int row, int col)
    {
        var lastColUsed = sheet.LastColumnUsed()?.ColumnNumber() ?? col;
        var lastCol = Math.Min(lastColUsed, col + 8);

        for (var c = col + 1; c <= lastCol; c++)
        {
            var cellValue = sheet.Cell(row, c).Value;
            if (cellValue.IsBlank || cellValue.IsError) continue;

            var text = GetText(cellValue);
            if (!string.IsNullOrEmpty(text) && ExcelLabelMatcher.IsAnyKnownLabel(ExcelLabelMatcher.Normalize(text)))
                break;

            return cellValue;
        }

        var below = sheet.Cell(row + 1, col).Value;
        return below.IsBlank || below.IsError ? (XLCellValue?)null : below;
    }

    private static TimeSpan? FirstTimeInRow(IXLRow row) =>
        row.CellsUsed()
            .Select(c => TimeParser.TryParse(c.Value, out var t) ? t : (TimeSpan?)null)
            .FirstOrDefault(t => t is not null);

    private static TimeSpan? LastTimeInRow(IXLRow row) =>
        row.CellsUsed()
            .Select(c => TimeParser.TryParse(c.Value, out var t) ? t : (TimeSpan?)null)
            .LastOrDefault(t => t is not null);

    private static string GetText(XLCellValue value)
    {
        if (value.IsBlank || value.IsError) return string.Empty;
        return value.ToString()?.Trim() ?? string.Empty;
    }

    private static string GetString(Dictionary<MetadataField, XLCellValue> found, MetadataField field) =>
        found.TryGetValue(field, out var value) ? GetText(value) : string.Empty;

    private static string? NullIfEmpty(string? s) => string.IsNullOrWhiteSpace(s) ? null : s;
}
