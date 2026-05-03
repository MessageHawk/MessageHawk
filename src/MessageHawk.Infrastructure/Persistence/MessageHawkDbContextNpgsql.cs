using Microsoft.EntityFrameworkCore;

namespace MessageHawk.Infrastructure.Persistence;

public sealed class MessageHawkDbContextNpgsql(DbContextOptions<MessageHawkDbContextNpgsql> options) : MessageHawkDbContext(options);
