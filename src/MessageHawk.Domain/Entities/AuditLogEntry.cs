namespace MessageHawk.Domain.Entities;

public class AuditLogEntry
{
    public Guid Id { get; set; }
    public required string ActorObjectId { get; set; }
    public string? ActorDisplayName { get; set; }
    public required string ResourceType { get; set; }
    public required string ResourceId { get; set; }
    public required string Action { get; set; }
    public DateTimeOffset OccurredAt { get; set; }
    public string? Details { get; set; }
}
