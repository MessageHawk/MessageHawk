using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace MessageHawk.Infrastructure.Persistence;

public sealed class MessageHawkDbContextFactory : IDesignTimeDbContextFactory<MessageHawkDbContext>
{
    public MessageHawkDbContext CreateDbContext(string[] args)
    {
        var cs = Environment.GetEnvironmentVariable("MESSAGEHAWK_SQL_CONNECTION")
                 ?? "Server=(localdb)\\mssqllocaldb;Database=MessageHawk;Trusted_Connection=True;TrustServerCertificate=True;MultipleActiveResultSets=true";

        var options = new DbContextOptionsBuilder<MessageHawkDbContext>()
            .UseSqlServer(cs)
            .Options;

        return new MessageHawkDbContext(options);
    }
}
