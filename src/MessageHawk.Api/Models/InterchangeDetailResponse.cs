namespace MessageHawk.Api.Models;

public sealed class InterchangeDetailResponse
{
    public Guid Id { get; init; }
    public string MessageTypeCode { get; init; } = "";
    public string MessageTypeDisplayName { get; init; } = "";
    public DateTimeOffset CreatedAt { get; init; }
    public DateTimeOffset UpdatedAt { get; init; }
    public string? CurrentStatus { get; init; }
    public IReadOnlyList<LogStepResponse> Steps { get; init; } = Array.Empty<LogStepResponse>();
}

public sealed class LogStepResponse
{
    public Guid Id { get; init; }
    public int SequenceNumber { get; init; }
    public string Sender { get; init; } = "";
    public string Receiver { get; init; } = "";
    public string Status { get; init; } = "";
    public DateTimeOffset OccurredAt { get; init; }
    public string? ContentType { get; init; }
    public string? BodyBase64 { get; init; }
    public string? IndexedPropertiesJson { get; init; }
}

public sealed class InterchangeSummaryResponse
{
    public Guid Id { get; init; }
    public string MessageTypeCode { get; init; } = "";
    public DateTimeOffset UpdatedAt { get; init; }
    public string? CurrentStatus { get; init; }
}
