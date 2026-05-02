namespace MessageHawk.Api.Models;

public sealed class CreateLogStepRequest
{
    public Guid StepId { get; set; }
    public string MessageTypeCode { get; set; } = "";
    public string MessageTypeDisplayName { get; set; } = "";
    public int SequenceNumber { get; set; }
    public string Sender { get; set; } = "";
    public string Receiver { get; set; } = "";
    public string Status { get; set; } = "";
    public DateTimeOffset OccurredAt { get; set; }
    public string? ContentType { get; set; }
    public string? BodyBase64 { get; set; }
    public Dictionary<string, string>? IndexedProperties { get; set; }
}
