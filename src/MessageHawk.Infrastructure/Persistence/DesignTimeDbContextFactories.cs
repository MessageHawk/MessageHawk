using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace MessageHawk.Infrastructure.Persistence;

public sealed class MessageHawkDbContextSqlServerFactory : IDesignTimeDbContextFactory<MessageHawkDbContextSqlServer>
{
    public MessageHawkDbContextSqlServer CreateDbContext(string[] args)
    {
        var cs = Environment.GetEnvironmentVariable("MESSAGEHAWK_SQL_CONNECTION")
                 ?? "Server=(localdb)\\mssqllocaldb;Database=MessageHawk;Trusted_Connection=True;TrustServerCertificate=True;MultipleActiveResultSets=true";

        var options = new DbContextOptionsBuilder<MessageHawkDbContextSqlServer>()
            .UseSqlServer(cs, sql => sql.MigrationsAssembly(typeof(MessageHawkDbContextSqlServer).Assembly.GetName().Name))
            .Options;

        return new MessageHawkDbContextSqlServer(options);
    }
}

public sealed class MessageHawkDbContextNpgsqlFactory : IDesignTimeDbContextFactory<MessageHawkDbContextNpgsql>
{
    public MessageHawkDbContextNpgsql CreateDbContext(string[] args)
    {
        var cs = Environment.GetEnvironmentVariable("MESSAGEHAWK_POSTGRES_CONNECTION")
                 ?? "Host=127.0.0.1;Port=5432;Database=messagehawk;Username=postgres;Password=postgres";

        var options = new DbContextOptionsBuilder<MessageHawkDbContextNpgsql>()
            .UseNpgsql(cs, npgsql => npgsql.MigrationsAssembly(typeof(MessageHawkDbContextNpgsql).Assembly.GetName().Name))
            .Options;

        return new MessageHawkDbContextNpgsql(options);
    }
}
