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

            // Configure NodaTime LocalDateTime properties
            entity.Property(e => e.OriginalLocalDateTime)
                .HasConversion(
                    v => v.ToDateTimeUnspecified(),
                    v => LocalDateTime.FromDateTime(v));
                    
            entity.Property(e => e.ConvertedLocalDateTime)
                .HasConversion(
                    v => v.ToDateTimeUnspecified(),
                    v => LocalDateTime.FromDateTime(v));
        });
    }
} 