using MessageHawk.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace MessageHawk.Infrastructure.Persistence;

public abstract class MessageHawkDbContext : DbContext
{
    protected MessageHawkDbContext(DbContextOptions options) : base(options)
    {
    }

    public DbSet<MessageType> MessageTypes => Set<MessageType>();
    public DbSet<Interchange> Interchanges => Set<Interchange>();
    public DbSet<LogStep> LogSteps => Set<LogStep>();
    public DbSet<AuditLogEntry> AuditLogEntries => Set<AuditLogEntry>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<MessageType>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Code).HasMaxLength(128).IsRequired();
            e.Property(x => x.DisplayName).HasMaxLength(512).IsRequired();
            e.HasIndex(x => x.Code).IsUnique();
        });

        modelBuilder.Entity<Interchange>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.CurrentStatus).HasMaxLength(256);
            e.HasOne(x => x.MessageType)
                .WithMany(t => t.Interchanges)
                .HasForeignKey(x => x.MessageTypeId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<LogStep>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Sender).HasMaxLength(512).IsRequired();
            e.Property(x => x.Receiver).HasMaxLength(512).IsRequired();
            e.Property(x => x.Status).HasMaxLength(256).IsRequired();
            e.Property(x => x.ContentType).HasMaxLength(256);
            e.Property(x => x.MessageBody).IsRequired();
            e.HasOne(x => x.Interchange)
                .WithMany(i => i.Steps)
                .HasForeignKey(x => x.InterchangeId)
                .OnDelete(DeleteBehavior.Cascade);
            e.HasIndex(x => new { x.InterchangeId, x.SequenceNumber }).IsUnique();
        });

        modelBuilder.Entity<AuditLogEntry>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.ActorObjectId).HasMaxLength(128).IsRequired();
            e.Property(x => x.ActorDisplayName).HasMaxLength(512);
            e.Property(x => x.ResourceType).HasMaxLength(64).IsRequired();
            e.Property(x => x.ResourceId).HasMaxLength(128).IsRequired();
            e.Property(x => x.Action).HasMaxLength(64).IsRequired();
            e.Property(x => x.Details).HasMaxLength(4000);
            e.HasIndex(x => x.OccurredAt);
        });
    }
}
