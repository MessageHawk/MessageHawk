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

        var connectionString = configuration.GetConnectionString("DefaultConnection");
        if (string.IsNullOrWhiteSpace(connectionString))
            throw new InvalidOperationException("Connection string 'DefaultConnection' is not configured.");
        var migrationsAssembly = typeof(MessageHawkDbContext).Assembly.GetName().Name!;
        var provider = configuration.GetValue("Database:Provider", "SqlServer");

        if (string.Equals(provider, "PostgreSQL", StringComparison.OrdinalIgnoreCase)
            || string.Equals(provider, "Npgsql", StringComparison.OrdinalIgnoreCase))
        {
            services.AddDbContext<MessageHawkDbContext, MessageHawkDbContextNpgsql>(options =>
                options.UseNpgsql(connectionString, npgsql =>
                    npgsql.MigrationsAssembly(migrationsAssembly)));
        }
        else
        {
            services.AddDbContext<MessageHawkDbContext, MessageHawkDbContextSqlServer>(options =>
                options.UseSqlServer(connectionString, sql =>
                    sql.MigrationsAssembly(migrationsAssembly)));
        }

        services.AddSingleton<RabbitMqConnection>();
        services.AddSingleton<IMessageQueuePublisher, RabbitMqLogStepPublisher>();
        services.AddScoped<ILogStepIngestProcessor, LogStepIngestProcessor>();

        return services;
    }
}
