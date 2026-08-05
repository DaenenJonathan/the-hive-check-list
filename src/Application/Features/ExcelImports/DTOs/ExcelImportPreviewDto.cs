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
    public string? Account { get; set; }
    public string? CostCode { get; set; }
    public string? AddressAction { get; set; }
    public string? AddressPickUp { get; set; }
    public string? City { get; set; }

    // Derived checklist name (can be overridden by user)
    public string SuggestedChecklistName { get; set; } = string.Empty;

    public List<ExcelImportItemDto> Items { get; set; } = [];
    public List<string> Errors { get; set; } = [];
    public List<string> Warnings { get; set; } = [];
    public bool IsValid => Errors.Count == 0 && Items.Count > 0;
}

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
