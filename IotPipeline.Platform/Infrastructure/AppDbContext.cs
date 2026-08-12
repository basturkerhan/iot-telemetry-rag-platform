using IotPipeline.Platform.Features.Ingestion.Entities;
using Microsoft.EntityFrameworkCore;

namespace IotPipeline.Platform.Infrastructure;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<TelemetryRecord> TelemetryRecords => Set<TelemetryRecord>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.HasPostgresExtension("vector");

        modelBuilder.Entity<TelemetryRecord>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.DeviceId)
                .IsRequired().HasMaxLength(50);

            entity.Property(e => e.Embedding)
                .HasColumnType("vector(384)");

            entity.HasIndex(e => e.Embedding)
                .HasMethod("hnsw")
                .HasOperators("vector_cosine_ops");

            entity
                .HasIndex(e => new { e.DeviceId, e.Timestamp })
                .IsDescending(false, true);
        });
    }
}