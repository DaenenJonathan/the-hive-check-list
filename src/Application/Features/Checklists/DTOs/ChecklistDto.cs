using TheHive.Domain.Enums;

namespace TheHive.Application.Features.Checklists.DTOs;

public class ChecklistDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int Version { get; set; }
    public ChecklistStatus Status { get; set; }
    public DateTime? ImportedAt { get; set; }
    public string? SourceFileName { get; set; }
    public DateTime? EventDate { get; set; }
    public Guid BrandActionId { get; set; }
    public string? BrandActionName { get; set; }
    public string? BrandActionAddress { get; set; }
    public string? BrandActionCity { get; set; }
    public DateTime CreatedAt { get; set; }
    public int TotalItems { get; set; }
    public int PreparedItems { get; set; }
}

public class ChecklistDetailDto : ChecklistDto
{
    public List<ChecklistItemDto> Items { get; set; } = [];
}

public class ChecklistItemDto
{
    public Guid Id { get; set; }
    public string MaterialName { get; set; } = string.Empty;
    public int QuantityRequested { get; set; }
    public int QuantityPrepared { get; set; }
    public int? QuantityReturned { get; set; }
    public string? Location { get; set; }
    public string? Notes { get; set; }
    public string? ImagePath { get; set; }
    public string? Remark { get; set; }
    public ChecklistItemStatus Status { get; set; }
    public string? Category { get; set; }
    public int SortOrder { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string? UpdatedBy { get; set; }
}
