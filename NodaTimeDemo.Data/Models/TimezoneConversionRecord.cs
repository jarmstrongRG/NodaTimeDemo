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
} 