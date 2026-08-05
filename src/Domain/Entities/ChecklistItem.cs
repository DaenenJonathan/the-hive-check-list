using TheHive.Domain.Common;
using TheHive.Domain.Enums;
using TheHive.Domain.Exceptions;

namespace TheHive.Domain.Entities;

public class ChecklistItem : BaseEntity
{
    public string MaterialName { get; private set; } = string.Empty;
    public int QuantityRequested { get; private set; }
    public int QuantityPrepared { get; private set; }
    public int? QuantityReturned { get; private set; }
    public string? Location { get; private set; }
    public string? Notes { get; private set; }
    public string? ImagePath { get; private set; }
    public string? Remark { get; private set; }
    public ChecklistItemStatus Status { get; private set; } = ChecklistItemStatus.ToPrepare;
    public string? Category { get; private set; }
    public int SortOrder { get; private set; }
    public Guid ChecklistId { get; private set; }
    public Checklist? Checklist { get; private set; }

    private ChecklistItem() { }

    public static ChecklistItem Create(
        Guid checklistId,
        string materialName,
        int quantityRequested,
        string? location = null,
        string? category = null,
        string? notes = null,
        int sortOrder = 0)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(materialName);
        if (quantityRequested <= 0)
            throw new DomainException("Quantity requested must be greater than zero.");

        return new ChecklistItem
        {
            ChecklistId = checklistId,
            MaterialName = materialName,
            QuantityRequested = quantityRequested,
            Location = location,
            Category = category,
            Notes = notes,
            SortOrder = sortOrder
        };
    }

    public void SetImage(string? imagePath) => ImagePath = imagePath;

    public void UpdateStatus(ChecklistItemStatus newStatus, int quantityPrepared, string? remark, string updatedBy)
    {
        var previousStatus = Status;
        Status = newStatus;
        QuantityPrepared = quantityPrepared;
        Remark = remark;
        SetUpdated(updatedBy);
    }

    public void UpdateReturn(int quantityReturned, string updatedBy)
    {
        if (quantityReturned < 0)
            throw new DomainException("Quantity returned cannot be negative.");
        QuantityReturned = quantityReturned;
        SetUpdated(updatedBy);
    }

    public void UpdateOrder(int sortOrder, string? category)
    {
        SortOrder = sortOrder;
        Category = category;
    }

    public void UpdateDetails(string materialName, int quantityRequested, string? location, string? category, string? notes)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(materialName);
        if (quantityRequested <= 0)
            throw new DomainException("Quantity requested must be greater than zero.");
        MaterialName = materialName;
        QuantityRequested = quantityRequested;
        Location = location;
        Category = category;
        Notes = notes;
    }
}
