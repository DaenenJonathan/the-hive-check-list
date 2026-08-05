using TheHive.Domain.Common;
using TheHive.Domain.Enums;

namespace TheHive.Domain.Entities;

public class BrandAction : BaseEntity
{
    public string Name { get; private set; } = string.Empty;
    public string Client { get; private set; } = string.Empty;
    public DateTime PlannedDate { get; private set; }
    public TimeSpan? PlannedDepartureTime { get; private set; }
    public TimeSpan? PlannedReturnTime { get; private set; }
    public ActionStatus Status { get; private set; } = ActionStatus.Planned;
    public string? Description { get; private set; }
    public string? Address { get; private set; }
    public string? City { get; private set; }
    public bool Sent { get; private set; }
    public DateTime? SentAt { get; private set; }
    public string? SentBy { get; private set; }
    public bool ReturnValidated { get; private set; }
    public DateTime? ReturnValidatedAt { get; private set; }
    public string? ReturnValidatedBy { get; private set; }

    private readonly List<Checklist> _checklists = [];
    public IReadOnlyCollection<Checklist> Checklists => _checklists.AsReadOnly();

    private BrandAction() { }

    public static BrandAction Create(
        string name,
        string client,
        DateTime plannedDate,
        string? description = null,
        string? address = null,
        string? city = null,
        TimeSpan? plannedDepartureTime = null,
        TimeSpan? plannedReturnTime = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentException.ThrowIfNullOrWhiteSpace(client);

        return new BrandAction
        {
            Name = name,
            Client = client,
            PlannedDate = plannedDate,
            Description = description,
            Address = address,
            City = city,
            PlannedDepartureTime = plannedDepartureTime,
            PlannedReturnTime = plannedReturnTime
        };
    }

    public void Update(
        string name,
        string client,
        DateTime plannedDate,
        string? description,
        TimeSpan? plannedDepartureTime = null,
        TimeSpan? plannedReturnTime = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentException.ThrowIfNullOrWhiteSpace(client);
        Name = name;
        Client = client;
        PlannedDate = plannedDate;
        Description = description;
        PlannedDepartureTime = plannedDepartureTime;
        PlannedReturnTime = plannedReturnTime;
    }

    public void SetStatus(ActionStatus status) => Status = status;

    public void MarkAsSent(string userId)
    {
        Sent = true;
        SentAt = DateTime.UtcNow;
        SentBy = userId;
    }

    public void ValidateReturn(string userId)
    {
        ReturnValidated = true;
        ReturnValidatedAt = DateTime.UtcNow;
        ReturnValidatedBy = userId;
    }
}
