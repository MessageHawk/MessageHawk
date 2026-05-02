using MessageHawk.Application.Abstractions;
using MessageHawk.Application.Options;
using MessageHawk.Infrastructure.Ingest;
using MessageHawk.Infrastructure.Messaging;
using MessageHawk.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace MessageHawk.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddMessageHawkInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.Configure<RabbitMqOptions>(configuration.GetSection(RabbitMqOptions.SectionName));

        services.AddDbContext<MessageHawkDbContext>(options =>
            options.UseSqlServer(
                configuration.GetConnectionString("DefaultConnection"),
                sql => sql.MigrationsAssembly(typeof(MessageHawkDbContext).Assembly.GetName().Name)));

        services.AddSingleton<RabbitMqConnection>();
        services.AddSingleton<IMessageQueuePublisher, RabbitMqLogStepPublisher>();
        services.AddScoped<ILogStepIngestProcessor, LogStepIngestProcessor>();

        return services;
    }
}
