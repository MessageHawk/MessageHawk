namespace MessageHawk.Domain.Entities;

public class LogStep
{
    public Guid Id { get; set; }
    public Guid InterchangeId { get; set; }
    public Interchange Interchange { get; set; } = null!;

    public int SequenceNumber { get; set; }
    public required string Sender { get; set; }
    public required string Receiver { get; set; }
    public required string Status { get; set; }
    public DateTimeOffset OccurredAt { get; set; }

    public byte[] MessageBody { get; set; } = Array.Empty<byte>();
    public string? ContentType { get; set; }
    public string? IndexedPropertiesJson { get; set; }

    public DateTimeOffset CreatedAt { get; set; }
}
