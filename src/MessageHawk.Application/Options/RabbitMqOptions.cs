namespace MessageHawk.Application.Options;

public sealed class RabbitMqOptions
{
    public const string SectionName = "RabbitMq";

    public string HostName { get; set; } = "localhost";
    public int Port { get; set; } = 5672;
    public string UserName { get; set; } = "guest";
    public string Password { get; set; } = "guest";
    public string VirtualHost { get; set; } = "/";

    /// <summary>Number of shard queues; one active consumer per shard preserves per-interchange ordering.</summary>
    public int ShardCount { get; set; } = 16;

    public string ExchangeName { get; set; } = "messagehawk.ingest";
}
