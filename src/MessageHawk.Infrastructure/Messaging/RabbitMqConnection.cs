using MessageHawk.Application.Options;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;

namespace MessageHawk.Infrastructure.Messaging;

public sealed class RabbitMqConnection(IOptions<RabbitMqOptions> options) : IDisposable
{
    private readonly IConnection _connection = Create(options.Value);

    public IModel CreateChannel() => _connection.CreateModel();

    public void Dispose() => _connection.Dispose();

    private static IConnection Create(RabbitMqOptions o)
    {
        var factory = new ConnectionFactory
        {
            HostName = o.HostName,
            Port = o.Port,
            UserName = o.UserName,
            Password = o.Password,
            VirtualHost = o.VirtualHost,
            DispatchConsumersAsync = true
        };
        return factory.CreateConnection("messagehawk");
    }
}
