using Microsoft.EntityFrameworkCore;
using NodaTime;
using NodaTimeDemo.Data.Models;

namespace NodaTimeDemo.Data;

public class NodaTimeDemoDbContext : DbContext
{
    public NodaTimeDemoDbContext(DbContextOptions<NodaTimeDemoDbContext> options)
        : base(options)
    {
    }

    public DbSet<TimezoneConversionRecord> TimezoneConversions { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure NodaTime types
        modelBuilder.Entity<TimezoneConversionRecord>(entity =>
        {
            entity.HasKey(e => e.Id);
            
            entity.Property(e => e.OriginalTimeZone)
                .IsRequired()
                .HasMaxLength(100);
                
            entity.Property(e => e.TargetTimeZone)
                .IsRequired()
                .HasMaxLength(100);
                
            entity.Property(e => e.Notes)
                .HasMaxLength(500);

            // === OLD APPROACH (LocalDateTime) ===
            entity.Property(e => e.OriginalLocalDateTime)
                .HasConversion(
                    v => v.ToDateTimeUnspecified(),
                    v => LocalDateTime.FromDateTime(v));
                    
            entity.Property(e => e.ConvertedLocalDateTime)
                .HasConversion(
                    v => v.ToDateTimeUnspecified(),
                    v => LocalDateTime.FromDateTime(v));

            // === NEW APPROACH (Instant) ===
            // Store NodaTime Instant as UTC DateTime in database
            entity.Property(e => e.OriginalInstant)
                .HasConversion(
                    v => v.HasValue ? v.Value.ToDateTimeUtc() : (DateTime?)null,
                    v => v.HasValue ? Instant.FromDateTimeUtc(v.Value) : (Instant?)null);
                    
            entity.Property(e => e.ConvertedInstant)
                .HasConversion(
                    v => v.HasValue ? v.Value.ToDateTimeUtc() : (DateTime?)null,
                    v => v.HasValue ? Instant.FromDateTimeUtc(v.Value) : (Instant?)null);
                    
            // Alternative UTC storage (for comparison)
            entity.Property(e => e.OriginalUtcDateTime)
                .HasColumnType("datetime2");
                
            // Migration flag
            entity.Property(e => e.UsesNodaTimeInstant)
                .HasDefaultValue(false);
        });
    }
} 