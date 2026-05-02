using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using MessageHawk.Application.Abstractions;
using MessageHawk.Application.Ingest;
using MessageHawk.Application.Options;
using MessageHawk.Infrastructure.Messaging;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace MessageHawk.Worker;

public sealed class ShardIngestConsumerHostedService(
    RabbitMqConnection connection,
    IOptions<RabbitMqOptions> rabbitOptions,
    IOptions<WorkerOptions> workerOptions,
    IServiceScopeFactory scopeFactory,
    ILogger<ShardIngestConsumerHostedService> logger) : BackgroundService
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var opts = rabbitOptions.Value;
        var worker = workerOptions.Value;
        var shards = worker.AssignedShardIndex is { } single
            ? new[] { single }
            : Enumerable.Range(0, opts.ShardCount).ToArray();

        if (worker.AssignedShardIndex is { } assigned && (assigned < 0 || assigned >= opts.ShardCount))
            throw new InvalidOperationException($"Worker:AssignedShardIndex {assigned} is out of range for RabbitMq:ShardCount {opts.ShardCount}.");

        logger.LogInformation("Ingest consumer starting for shard(s): {Shards}", string.Join(", ", shards));

        var tasks = shards.Select(s => ConsumeShardAsync(s, stoppingToken)).ToArray();
        await Task.WhenAll(tasks).ConfigureAwait(false);
    }

    private async Task ConsumeShardAsync(int shardIndex, CancellationToken stoppingToken)
    {
        var opts = rabbitOptions.Value;
        try
        {
            var channel = connection.CreateChannel();
            try
            {
                RabbitMqTopology.EnsureIngestTopology(channel, opts);
                var queue = RabbitMqTopology.QueueName(shardIndex);
                channel.BasicQos(0, 1, false);

                var consumer = new AsyncEventingBasicConsumer(channel);
                consumer.Received += (_, ea) => HandleAsync(channel, ea, shardIndex, stoppingToken);

                channel.BasicConsume(queue, autoAck: false, consumer);

                try
                {
                    await Task.Delay(Timeout.Infinite, stoppingToken).ConfigureAwait(false);
                }
                catch (OperationCanceledException)
                {
                    // shutting down
                }
            }
            finally
            {
                channel.Dispose();
            }
        }
        catch (Exception ex)
        {
            logger.LogCritical(ex, "Shard {Shard} consumer failed to start or crashed", shardIndex);
            throw;
        }
    }

    private async Task HandleAsync(IModel channel, BasicDeliverEventArgs ea, int shardIndex, CancellationToken ct)
    {
        try
        {
            var json = Encoding.UTF8.GetString(ea.Body.Span);
            var msg = JsonSerializer.Deserialize<LogStepIngestMessage>(json, JsonOptions);
            if (msg is null)
            {
                logger.LogWarning("Shard {Shard}: null message body, dead-lettering", shardIndex);
                channel.BasicNack(ea.DeliveryTag, multiple: false, requeue: false);
                return;
            }

            await using var scope = scopeFactory.CreateAsyncScope();
            var processor = scope.ServiceProvider.GetRequiredService<ILogStepIngestProcessor>();
            await processor.ProcessAsync(msg, ct).ConfigureAwait(false);
            channel.BasicAck(ea.DeliveryTag, multiple: false);
        }
        catch (OperationCanceledException) when (ct.IsCancellationRequested)
        {
            channel.BasicNack(ea.DeliveryTag, multiple: false, requeue: true);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Shard {Shard}: ingest processing failed", shardIndex);
            channel.BasicNack(ea.DeliveryTag, multiple: false, requeue: false);
        }
    }
}
