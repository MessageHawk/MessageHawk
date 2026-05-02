using MessageHawk.Application.Ingest;

namespace MessageHawk.Application.Abstractions;

public interface ILogStepIngestProcessor
{
    Task ProcessAsync(LogStepIngestMessage message, CancellationToken cancellationToken = default);
}
