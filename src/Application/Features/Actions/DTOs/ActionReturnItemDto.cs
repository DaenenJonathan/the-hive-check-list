namespace TheHive.Application.Features.Actions.DTOs;

public class ActionReturnsDto
{
    public Guid ActionId { get; set; }
    public bool Sent { get; set; }
    public DateTime? SentAt { get; set; }
    public bool ReturnValidated { get; set; }
    public DateTime? ReturnValidatedAt { get; set; }
    public string? MaterialPhotoPath { get; set; }
    public string? ConsumablesPhotoPath { get; set; }
    public List<ActionReturnItemDto> Items { get; set; } = [];
}

public class ActionReturnItemDto
{
    public Guid ItemId { get; set; }
    public Guid ChecklistId { get; set; }
    public string ChecklistName { get; set; } = string.Empty;
    public string MaterialName { get; set; } = string.Empty;
    public string? Category { get; set; }
    public string? Location { get; set; }
    public int QuantityRequested { get; set; }
    public int QuantityPrepared { get; set; }
    public int? QuantityReturned { get; set; }
}
