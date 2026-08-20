namespace TheHive.Application.Features.ExcelImports.DTOs;

public class ExcelImportPreviewDto
{
    public string FileName { get; set; } = string.Empty;

    // Metadata extracted from the Excel header
    public string Client { get; set; } = string.Empty;
    public string Brand { get; set; } = string.Empty;
    public string ProjectName { get; set; } = string.Empty;
    public string ActionType { get; set; } = string.Empty;
    public DateTime? EventDate { get; set; }
    public TimeSpan? PlannedDepartureTime { get; set; }
    public TimeSpan? PlannedReturnTime { get; set; }
    public string? Account { get; set; }
    public string? CostCode { get; set; }
    public string? AddressAction { get; set; }
    public string? AddressPickUp { get; set; }
    public string? City { get; set; }

    // Derived checklist name (can be overridden by user)
    public string SuggestedChecklistName { get; set; } = string.Empty;

    // The sheet actually used to produce Items/metadata above, and every other visible sheet in the
    // workbook that also looks like a checklist (i.e. has at least one item). Populated whenever the
    // workbook has more than one such sheet, so the caller can offer a picker instead of guessing.
    public string? SelectedSheetName { get; set; }
    public List<ExcelSheetOptionDto> AvailableSheets { get; set; } = [];

    public List<ExcelImportItemDto> Items { get; set; } = [];
    public List<string> Errors { get; set; } = [];
    public List<string> Warnings { get; set; } = [];
    public bool IsValid => Errors.Count == 0 && Items.Count > 0;
}

public record ExcelSheetOptionDto(string Name, int ItemCount);

public class ExcelImportItemDto
{
    public string MaterialName { get; set; } = string.Empty;
    public int QuantityRequested { get; set; }
    public string? Category { get; set; }
    public string? Notes { get; set; }
    public int SortOrder { get; set; }

    // Optimized (resized/compressed) image bytes extracted from the source Excel file, if the item
    // row had a linked picture. Null when the item has no image.
    public byte[]? ImageData { get; set; }
}
