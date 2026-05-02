using System.Text.Json;
using MessageHawk.Api.Models;
using MessageHawk.Application.Abstractions;
using MessageHawk.Application.Ingest;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MessageHawk.Api.Controllers;

[ApiController]
[Authorize]
public sealed class IngestController(IMessageQueuePublisher publisher) : ControllerBase
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
    };

    [HttpPost("api/v1/interchanges/{interchangeId:guid}/steps")]
    [ProducesResponseType(StatusCodes.Status202Accepted)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> EnqueueStep(Guid interchangeId, [FromBody] CreateLogStepRequest request, CancellationToken cancellationToken)
    {
        if (request.StepId == Guid.Empty)
            return BadRequest("StepId is required.");

        var message = new LogStepIngestMessage
        {
            StepId = request.StepId,
            InterchangeId = interchangeId,
            MessageTypeCode = request.MessageTypeCode,
            MessageTypeDisplayName = request.MessageTypeDisplayName,
            SequenceNumber = request.SequenceNumber,
            Sender = request.Sender,
            Receiver = request.Receiver,
            Status = request.Status,
            OccurredAt = request.OccurredAt == default ? DateTimeOffset.UtcNow : request.OccurredAt,
            ContentType = request.ContentType,
            BodyBase64 = request.BodyBase64,
            IndexedProperties = request.IndexedProperties
        };

        var json = JsonSerializer.Serialize(message, JsonOptions);
        var bytes = System.Text.Encoding.UTF8.GetBytes(json);
        await publisher.PublishIngestAsync(interchangeId, bytes, cancellationToken).ConfigureAwait(false);
        return Accepted();
    }
}
