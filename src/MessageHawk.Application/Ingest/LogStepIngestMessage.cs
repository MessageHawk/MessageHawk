using System.Text.Json.Serialization;

namespace MessageHawk.Application.Ingest;

/// <summary>JSON payload on the ingest queue and from the HTTP API.</summary>
public sealed class LogStepIngestMessage
{
    [JsonPropertyName("stepId")]
    public Guid StepId { get; set; }

    [JsonPropertyName("interchangeId")]
    public Guid InterchangeId { get; set; }

    [JsonPropertyName("messageTypeCode")]
    public string MessageTypeCode { get; set; } = "";

    [JsonPropertyName("messageTypeDisplayName")]
    public string MessageTypeDisplayName { get; set; } = "";

    [JsonPropertyName("sequenceNumber")]
    public int SequenceNumber { get; set; }

    [JsonPropertyName("sender")]
    public string Sender { get; set; } = "";

    [JsonPropertyName("receiver")]
    public string Receiver { get; set; } = "";

    [JsonPropertyName("status")]
    public string Status { get; set; } = "";

    [JsonPropertyName("occurredAt")]
    public DateTimeOffset OccurredAt { get; set; }

    [JsonPropertyName("contentType")]
    public string? ContentType { get; set; }

    /// <summary>Raw message body, Base64-encoded for JSON transport.</summary>
    [JsonPropertyName("bodyBase64")]
    public string? BodyBase64 { get; set; }

    [JsonPropertyName("indexedProperties")]
    public Dictionary<string, string>? IndexedProperties { get; set; }
}
