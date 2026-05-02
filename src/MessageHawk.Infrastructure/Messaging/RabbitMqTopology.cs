using MessageHawk.Application.Options;
using RabbitMQ.Client;

namespace MessageHawk.Infrastructure.Messaging;

public static class RabbitMqTopology
{
    public static void EnsureIngestTopology(IModel channel, RabbitMqOptions options)
    {
        channel.ExchangeDeclare(
            options.ExchangeName,
            ExchangeType.Direct,
            durable: true,
            autoDelete: false);

        for (var i = 0; i < options.ShardCount; i++)
        {
            var queue = QueueName(i);
            channel.QueueDeclare(queue, durable: true, exclusive: false, autoDelete: false);
            channel.QueueBind(queue, options.ExchangeName, routingKey: i.ToString());
        }
    }

    public static string QueueName(int shardIndex) => $"messagehawk.ingest.shard.{shardIndex}";
}
