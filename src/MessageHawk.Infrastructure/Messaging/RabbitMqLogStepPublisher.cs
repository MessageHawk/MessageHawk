using MessageHawk.Application.Abstractions;
using MessageHawk.Application.Options;
using MessageHawk.Application.Sharding;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;

namespace MessageHawk.Infrastructure.Messaging;

public sealed class RabbitMqLogStepPublisher(
    RabbitMqConnection connection,
    IOptions<RabbitMqOptions> options) : IMessageQueuePublisher
{
    private readonly RabbitMqOptions _options = options.Value;

    public Task PublishIngestAsync(Guid interchangeId, ReadOnlyMemory<byte> jsonPayload, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var shard = InterchangeSharding.GetShardIndex(interchangeId, _options.ShardCount);
        using var channel = connection.CreateChannel();
        RabbitMqTopology.EnsureIngestTopology(channel, _options);

        var props = channel.CreateBasicProperties();
        props.ContentType = "application/json";
        props.DeliveryMode = 2;

        var body = jsonPayload.ToArray();
        channel.BasicPublish(
            exchange: _options.ExchangeName,
            routingKey: shard.ToString(),
            mandatory: false,
            basicProperties: props,
            body: body);

        return Task.CompletedTask;
    }
}
