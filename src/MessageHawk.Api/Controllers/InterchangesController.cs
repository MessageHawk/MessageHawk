using MessageHawk.Api.Models;
using MessageHawk.Domain.Entities;
using MessageHawk.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace MessageHawk.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/v1/interchanges")]
public sealed class InterchangesController(MessageHawkDbContext db) : ControllerBase
{
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(InterchangeDetailResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<InterchangeDetailResponse>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var entity = await db.Interchanges
            .AsNoTracking()
            .Include(i => i.MessageType)
            .Include(i => i.Steps.OrderBy(s => s.SequenceNumber))
            .FirstOrDefaultAsync(i => i.Id == id, cancellationToken);

        if (entity is null)
            return NotFound();

        await WriteViewAuditAsync("Interchange", id.ToString(), cancellationToken).ConfigureAwait(false);

        var steps = entity.Steps.Select(s => new LogStepResponse
        {
            Id = s.Id,
            SequenceNumber = s.SequenceNumber,
            Sender = s.Sender,
            Receiver = s.Receiver,
            Status = s.Status,
            OccurredAt = s.OccurredAt,
            ContentType = s.ContentType,
            BodyBase64 = s.MessageBody.Length == 0 ? null : Convert.ToBase64String(s.MessageBody),
            IndexedPropertiesJson = s.IndexedPropertiesJson
        }).ToList();

        return new InterchangeDetailResponse
        {
            Id = entity.Id,
            MessageTypeCode = entity.MessageType.Code,
            MessageTypeDisplayName = entity.MessageType.DisplayName,
            CreatedAt = entity.CreatedAt,
            UpdatedAt = entity.UpdatedAt,
            CurrentStatus = entity.CurrentStatus,
            Steps = steps
        };
    }

    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<InterchangeSummaryResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<InterchangeSummaryResponse>>> Search(
        [FromQuery] string? status,
        [FromQuery] DateTimeOffset? from,
        [FromQuery] DateTimeOffset? to,
        [FromQuery] int limit = 50,
        CancellationToken cancellationToken = default)
    {
        limit = Math.Clamp(limit, 1, 200);

        var q = db.Interchanges.AsNoTracking().Include(i => i.MessageType).AsQueryable();
        if (!string.IsNullOrWhiteSpace(status))
            q = q.Where(i => i.CurrentStatus == status);
        if (from is { } f)
            q = q.Where(i => i.UpdatedAt >= f);
        if (to is { } t)
            q = q.Where(i => i.UpdatedAt <= t);

        var rows = await q
            .OrderByDescending(i => i.UpdatedAt)
            .Take(limit)
            .Select(i => new InterchangeSummaryResponse
            {
                Id = i.Id,
                MessageTypeCode = i.MessageType.Code,
                UpdatedAt = i.UpdatedAt,
                CurrentStatus = i.CurrentStatus
            })
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        return rows;
    }

    private async Task WriteViewAuditAsync(string resourceType, string resourceId, CancellationToken cancellationToken)
    {
        var oid = User.FindFirstValue("oid") ?? User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "unknown";
        var name = User.FindFirstValue("preferred_username") ?? User.FindFirstValue(ClaimTypes.Name);

        db.AuditLogEntries.Add(new AuditLogEntry
        {
            Id = Guid.NewGuid(),
            ActorObjectId = oid,
            ActorDisplayName = name,
            ResourceType = resourceType,
            ResourceId = resourceId,
            Action = "view",
            OccurredAt = DateTimeOffset.UtcNow
        });
        await db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
    }
}
