using MessageHawk.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace MessageHawk.Infrastructure.Persistence;

public sealed class MessageHawkDbContextSqlServer(DbContextOptions<MessageHawkDbContextSqlServer> options) : MessageHawkDbContext(options)
{
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.Entity<LogStep>(e =>
        {
            e.Property(x => x.IndexedPropertiesJson).HasColumnType("nvarchar(max)");
        });
    }
}
