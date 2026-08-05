namespace TheHive.Domain.Enums;

public enum ChecklistItemStatus
{
    ToPrepare = 0,
    Prepared = 1,
    Missing = 2,
    PartiallyPrepared = 3,
    Loaded = 4,
    Cancelled = 5,
    Replaced = 6
}
