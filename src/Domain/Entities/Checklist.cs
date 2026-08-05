using TheHive.Domain.Common;
using TheHive.Domain.Enums;
using TheHive.Domain.Exceptions;

namespace TheHive.Domain.Entities;

public class Checklist : BaseEntity
{
    public string Name { get; private set; } = string.Empty;
    public int Version { get; private set; } = 1;
    public ChecklistStatus Status { get; private set; } = ChecklistStatus.Draft;
    public DateTime? ImportedAt { get; private set; }
    public string? SourceFileName { get; private set; }
    public DateTime? EventDate { get; private set; }
    public Guid BrandActionId { get; private set; }
    public BrandAction? BrandAction { get; private set; }

    private readonly List<ChecklistItem> _items = [];
    public IReadOnlyCollection<ChecklistItem> Items => _items.AsReadOnly();

    private Checklist() { }

    public static Checklist Create(string name, Guid brandActionId, string? sourceFileName = null, DateTime? eventDate = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        return new Checklist
        {
            Name = name,
            BrandActionId = brandActionId,
            SourceFileName = sourceFileName,
            ImportedAt = sourceFileName != null ? DateTime.UtcNow : null,
            EventDate = eventDate
        };
    }

    public void SetEventDate(DateTime? date) => EventDate = date;

    public void AddItem(ChecklistItem item)
    {
        if (Status == ChecklistStatus.Archived)
            throw new DomainException("Cannot modify an archived checklist.");
        _items.Add(item);
    }

    public void Update(string name)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        if (Status == ChecklistStatus.Archived)
            throw new DomainException("Cannot modify an archived checklist.");
        Name = name;
    }

    public void Activate()
    {
        if (Status != ChecklistStatus.Draft)
            throw new DomainException("Only draft checklists can be activated.");
        Status = ChecklistStatus.Active;
    }

    public void Archive()
    {
        Status = ChecklistStatus.Archived;
    }

    public void IncrementVersion()
    {
        Version++;
    }
}
