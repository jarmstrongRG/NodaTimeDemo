using NodaTime;
using System.ComponentModel.DataAnnotations;

namespace NodaTimeDemo.Data.Models;

public class TimezoneConversionRecord
{
    [Key]
    public int Id { get; set; }
    
    [Required]
    public string OriginalTimeZone { get; set; } = string.Empty;
    
    [Required]
    public string TargetTimeZone { get; set; } = string.Empty;
    
    // === OLD APPROACH (Keep during migration) ===
    [Required]
    public DateTime OriginalDateTime { get; set; }
    
    [Required]
    public DateTime ConvertedDateTime { get; set; }
    
    [Required]
    public DateTime CreatedAt { get; set; }
    
    public string? Notes { get; set; }
    
    // Store the original input as local date time for reference
    public LocalDateTime OriginalLocalDateTime { get; set; }
    
    // Store the converted result as local date time
    public LocalDateTime ConvertedLocalDateTime { get; set; }
    
    // === NEW APPROACH (NodaTime Instant) ===
    // The precise moment in time - this is the "golden" timestamp
    public Instant? OriginalInstant { get; set; }
    
    // For validation: should equal OriginalInstant since it's the same moment
    public Instant? ConvertedInstant { get; set; }
    
    // Alternative: Store as UTC and convert on display
    public DateTime? OriginalUtcDateTime { get; set; }
    
    // Flag to indicate which approach was used for this record
    public bool UsesNodaTimeInstant { get; set; } = false;
} 