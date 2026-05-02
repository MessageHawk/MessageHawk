namespace MessageHawk.Application.Abstractions;

public interface IMessageQueuePublisher
{
    Task PublishIngestAsync(Guid interchangeId, ReadOnlyMemory<byte> jsonPayload, CancellationToken cancellationToken = default);
}
