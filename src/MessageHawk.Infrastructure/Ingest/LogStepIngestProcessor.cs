using System.Text.Json;
using MessageHawk.Application.Abstractions;
using MessageHawk.Application.Ingest;
using MessageHawk.Domain.Entities;
using MessageHawk.Infrastructure.Persistence;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace MessageHawk.Infrastructure.Ingest;

public sealed class LogStepIngestProcessor(
    MessageHawkDbContext db,
    ILogger<LogStepIngestProcessor> logger) : ILogStepIngestProcessor
{
    private static readonly JsonSerializerOptions JsonOpts = new() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

    public async Task ProcessAsync(LogStepIngestMessage message, CancellationToken cancellationToken = default)
    {
        if (await db.LogSteps.AsNoTracking().AnyAsync(s => s.Id == message.StepId, cancellationToken))
            return;

        var body = DecodeBody(message.BodyBase64);

        await using var tx = await db.Database.BeginTransactionAsync(cancellationToken);
        try
        {
            var type = await db.MessageTypes.FirstOrDefaultAsync(t => t.Code == message.MessageTypeCode, cancellationToken);
            if (type is null)
            {
                type = new MessageType
                {
                    Code = message.MessageTypeCode,
                    DisplayName = string.IsNullOrWhiteSpace(message.MessageTypeDisplayName)
                        ? message.MessageTypeCode
                        : message.MessageTypeDisplayName
                };
                db.MessageTypes.Add(type);
                await db.SaveChangesAsync(cancellationToken);
            }

            var interchange = await db.Interchanges
                .FirstOrDefaultAsync(i => i.Id == message.InterchangeId, cancellationToken);

            if (interchange is null)
            {
                interchange = new Interchange
                {
                    Id = message.InterchangeId,
                    MessageTypeId = type.Id,
                    CreatedAt = DateTimeOffset.UtcNow,
                    UpdatedAt = DateTimeOffset.UtcNow,
                    LastStepSequence = null,
                    CurrentStatus = message.Status
                };
                db.Interchanges.Add(interchange);
                await db.SaveChangesAsync(cancellationToken);
            }

            interchange = await db.Interchanges
                .FirstAsync(i => i.Id == message.InterchangeId, cancellationToken);

            if (interchange.LastStepSequence is { } lastSeq && message.SequenceNumber != lastSeq + 1)
            {
                throw new InvalidOperationException(
                    $"Out-of-order step for interchange {message.InterchangeId}: expected sequence {lastSeq + 1}, got {message.SequenceNumber}.");
            }

            string? indexedJson = null;
            if (message.IndexedProperties is { Count: > 0 })
                indexedJson = JsonSerializer.Serialize(message.IndexedProperties, JsonOpts);

            db.LogSteps.Add(new LogStep
            {
                Id = message.StepId,
                InterchangeId = message.InterchangeId,
                SequenceNumber = message.SequenceNumber,
                Sender = message.Sender,
                Receiver = message.Receiver,
                Status = message.Status,
                OccurredAt = message.OccurredAt,
                MessageBody = body,
                ContentType = message.ContentType,
                IndexedPropertiesJson = indexedJson,
                CreatedAt = DateTimeOffset.UtcNow
            });

            interchange.LastStepSequence = message.SequenceNumber;
            interchange.UpdatedAt = DateTimeOffset.UtcNow;
            interchange.CurrentStatus = message.Status;

            await db.SaveChangesAsync(cancellationToken);
            await tx.CommitAsync(cancellationToken);
        }
        catch (DbUpdateException ex) when (IsUniqueConstraintViolation(ex))
        {
            await tx.RollbackAsync(cancellationToken);
            if (await db.LogSteps.AsNoTracking().AnyAsync(s => s.Id == message.StepId, cancellationToken))
                return;
            logger.LogWarning(ex, "Unique constraint violation for interchange {InterchangeId} but step {StepId} not present; rethrowing", message.InterchangeId, message.StepId);
            throw;
        }
        catch
        {
            await tx.RollbackAsync(cancellationToken);
            throw;
        }
    }

    private static byte[] DecodeBody(string? bodyBase64)
    {
        if (string.IsNullOrEmpty(bodyBase64))
            return Array.Empty<byte>();
        return Convert.FromBase64String(bodyBase64);
    }

    private static bool IsUniqueConstraintViolation(DbUpdateException ex) =>
        ex.InnerException is SqlException sql && (sql.Number == 2601 || sql.Number == 2627);
}
