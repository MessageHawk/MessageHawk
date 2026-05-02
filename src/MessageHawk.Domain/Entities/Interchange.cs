namespace MessageHawk.Domain.Entities;

public class Interchange
{
    public Guid Id { get; set; }
    public int MessageTypeId { get; set; }
    public MessageType MessageType { get; set; } = null!;

    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
    public int? LastStepSequence { get; set; }
    public string? CurrentStatus { get; set; }

    public ICollection<LogStep> Steps { get; set; } = new List<LogStep>();
}
