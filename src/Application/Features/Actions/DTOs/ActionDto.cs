using TheHive.Domain.Enums;

namespace TheHive.Application.Features.Actions.DTOs;

public class ActionDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Client { get; set; } = string.Empty;
    public DateTime PlannedDate { get; set; }
    public TimeSpan? PlannedDepartureTime { get; set; }
    public TimeSpan? PlannedReturnTime { get; set; }
    public ActionStatus Status { get; set; }
    public string? Description { get; set; }
    public string? Address { get; set; }
    public string? City { get; set; }
    public DateTime CreatedAt { get; set; }
    public int ChecklistCount { get; set; }
    public int TotalItems { get; set; }
    public int PreparedItems { get; set; }
    public bool Sent { get; set; }
    public DateTime? SentAt { get; set; }
    public bool IsReadyToSend { get; set; }
    public bool ReturnValidated { get; set; }
    public DateTime? ReturnValidatedAt { get; set; }
}
