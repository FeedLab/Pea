using Microsoft.EntityFrameworkCore;
using Pea.Data.Entities;

namespace Pea.Data;

/// <summary>
/// Database context for Pea Meter application
/// </summary>
public class PeaDbContext : DbContext
{
    private readonly string connectionString;

    public PeaDbContext(string connectionString)
    {
        this.connectionString = connectionString;
    }

    public PeaDbContext(DbContextOptions<PeaDbContext> options) : base(options)
    {
        connectionString = string.Empty;
    }

    internal DbSet<MeterReadingEntity> MeterReadings { get; set; } = null!;

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured && !string.IsNullOrEmpty(connectionString))
        {
            optionsBuilder.UseSqlite(connectionString);
        }
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure MeterReadingEntity
        modelBuilder.Entity<MeterReadingEntity>(entity =>
        {
            entity.HasIndex(e => e.PeriodStart);
            entity.HasIndex(e => e.UserId);
            entity.HasIndex(e => new { e.UserId, e.PeriodStart }).IsUnique();
        });
    }
}
