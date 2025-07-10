using NodaTime;
using NodaTime.TimeZones;
using NodaTimeDemo.Data;
using NodaTimeDemo.Data.Models;

namespace NodaTimeDemo.Api.Services;

public class TimezoneConversionService
{
    private readonly NodaTimeDemoDbContext _context;
    private readonly IDateTimeZoneProvider _timeZoneProvider;

    public TimezoneConversionService(NodaTimeDemoDbContext context, IDateTimeZoneProvider timeZoneProvider)
    {
        _context = context;
        _timeZoneProvider = timeZoneProvider;
    }

    public async Task<TimezoneConversionResult> ConvertTimeAsync(
        DateTime dateTime, 
        string fromTimeZone, 
        string toTimeZone, 
        string? notes = null)
    {
        try
        {
            // Get the timezone objects
            var sourceZone = _timeZoneProvider.GetZoneOrNull(fromTimeZone);
            var targetZone = _timeZoneProvider.GetZoneOrNull(toTimeZone);

            if (sourceZone == null)
                throw new ArgumentException($"Source timezone '{fromTimeZone}' not found");
            
            if (targetZone == null)
                throw new ArgumentException($"Target timezone '{toTimeZone}' not found");

            // Create a LocalDateTime from the input
            var localDateTime = LocalDateTime.FromDateTime(dateTime);
            
            // Convert to ZonedDateTime in the source timezone
            var sourceZonedDateTime = sourceZone.AtLeniently(localDateTime);
            
            // Convert to the target timezone
            var targetZonedDateTime = sourceZonedDateTime.WithZone(targetZone);
            
            // === NEW APPROACH: Get the precise Instant ===
            var instant = sourceZonedDateTime.ToInstant();
            
            // Create conversion record with BOTH old and new approaches
            var record = new TimezoneConversionRecord
            {
                // === OLD APPROACH (Keep for compatibility) ===
                OriginalTimeZone = fromTimeZone,
                TargetTimeZone = toTimeZone,
                OriginalDateTime = dateTime,
                ConvertedDateTime = targetZonedDateTime.ToDateTimeUnspecified(),
                OriginalLocalDateTime = localDateTime,
                ConvertedLocalDateTime = targetZonedDateTime.LocalDateTime,
                CreatedAt = DateTime.UtcNow,
                Notes = notes,
                
                // === NEW APPROACH (NodaTime Instant) ===
                OriginalInstant = instant,
                ConvertedInstant = instant, // Same moment in time!
                OriginalUtcDateTime = instant.ToDateTimeUtc(),
                UsesNodaTimeInstant = true,
            };

            // Add notes about the approach being used
            if (string.IsNullOrEmpty(notes))
            {
                record.Notes = "Dual approach: Old DateTime + New NodaTime Instant";
            }
            else
            {
                record.Notes = $"{notes} | Dual approach: Old DateTime + New NodaTime Instant";
            }

            _context.TimezoneConversions.Add(record);
            await _context.SaveChangesAsync();

            return new TimezoneConversionResult
            {
                Success = true,
                OriginalDateTime = dateTime,
                ConvertedDateTime = targetZonedDateTime.ToDateTimeUnspecified(),
                OriginalTimeZone = fromTimeZone,
                TargetTimeZone = toTimeZone,
                OriginalZonedDateTime = sourceZonedDateTime,
                TargetZonedDateTime = targetZonedDateTime,
                ConversionId = record.Id,
                
                // NEW: Include Instant for validation
                OriginalInstant = instant
            };
        }
        catch (Exception ex)
        {
            return new TimezoneConversionResult
            {
                Success = false,
                ErrorMessage = ex.Message
            };
        }
    }

    public async Task<IEnumerable<TimezoneConversionRecord>> GetConversionHistoryAsync(int limit = 50)
    {
        return await Task.FromResult(_context.TimezoneConversions
            .OrderByDescending(x => x.CreatedAt)
            .Take(limit)
            .ToList());
    }

    public IEnumerable<TimezoneInfo> GetAvailableTimeZones()
    {
        // Windows-style curated timezone list - only the most commonly used ones
        var windowsStyleTimezones = new Dictionary<string, string>
        {
            // UTC First
            { "UTC", "(UTC) Coordinated Universal Time" },
            
            // North America - Major Time Zones
            { "America/New_York", "(UTC-05:00) Eastern Time (US & Canada)" },
            { "America/Chicago", "(UTC-06:00) Central Time (US & Canada)" },
            { "America/Denver", "(UTC-07:00) Mountain Time (US & Canada)" },
            { "America/Los_Angeles", "(UTC-08:00) Pacific Time (US & Canada)" },
            { "America/Phoenix", "(UTC-07:00) Arizona" },
            { "America/Anchorage", "(UTC-09:00) Alaska" },
            { "Pacific/Honolulu", "(UTC-10:00) Hawaii" },
            
            // Canada
            { "America/Toronto", "(UTC-05:00) Eastern Time (Canada)" },
            { "America/Vancouver", "(UTC-08:00) Pacific Time (Canada)" },
            { "America/Halifax", "(UTC-04:00) Atlantic Time (Canada)" },
            { "America/St_Johns", "(UTC-03:30) Newfoundland Time" },
            
            // Caribbean & Central America
            { "America/Santo_Domingo", "(UTC-04:00) Dominican Republic" },
            { "America/Mexico_City", "(UTC-06:00) Mexico City" },
            
            // South America - Major Cities
            { "America/Santiago", "(UTC-03:00) Santiago, Chile" },
            { "America/Sao_Paulo", "(UTC-03:00) São Paulo" },
            { "America/Argentina/Buenos_Aires", "(UTC-03:00) Buenos Aires" },
            { "America/Bogota", "(UTC-05:00) Bogotá" },
            { "America/Lima", "(UTC-05:00) Lima" },
            
            // Europe - Major Cities
            { "Europe/London", "(UTC+00:00) London" },
            { "Europe/Paris", "(UTC+01:00) Paris" },
            { "Europe/Berlin", "(UTC+01:00) Berlin" },
            { "Europe/Rome", "(UTC+01:00) Rome" },
            { "Europe/Madrid", "(UTC+01:00) Madrid" },
            { "Europe/Amsterdam", "(UTC+01:00) Amsterdam" },
            { "Europe/Brussels", "(UTC+01:00) Brussels" },
            { "Europe/Vienna", "(UTC+01:00) Vienna" },
            { "Europe/Zurich", "(UTC+01:00) Zurich" },
            { "Europe/Stockholm", "(UTC+01:00) Stockholm" },
            { "Europe/Oslo", "(UTC+01:00) Oslo" },
            { "Europe/Copenhagen", "(UTC+01:00) Copenhagen" },
            { "Europe/Helsinki", "(UTC+02:00) Helsinki" },
            { "Europe/Moscow", "(UTC+03:00) Moscow" },
            
            // Asia - Major Cities
            { "Asia/Tokyo", "(UTC+09:00) Tokyo" },
            { "Asia/Shanghai", "(UTC+08:00) Beijing, Shanghai" },
            { "Asia/Hong_Kong", "(UTC+08:00) Hong Kong" },
            { "Asia/Singapore", "(UTC+08:00) Singapore" },
            { "Asia/Seoul", "(UTC+09:00) Seoul" },
            { "Asia/Bangkok", "(UTC+07:00) Bangkok" },
            { "Asia/Jakarta", "(UTC+07:00) Jakarta" },
            { "Asia/Manila", "(UTC+08:00) Manila" },
            { "Asia/Kolkata", "(UTC+05:30) Mumbai, New Delhi" },
            { "Asia/Dubai", "(UTC+04:00) Dubai" },
            { "Asia/Riyadh", "(UTC+03:00) Riyadh" },
            
            // Australia & Pacific - Major Cities
            { "Australia/Sydney", "(UTC+10:00) Sydney" },
            { "Australia/Melbourne", "(UTC+10:00) Melbourne" },
            { "Australia/Brisbane", "(UTC+10:00) Brisbane" },
            { "Australia/Perth", "(UTC+08:00) Perth" },
            { "Australia/Adelaide", "(UTC+09:30) Adelaide" },
            { "Pacific/Auckland", "(UTC+12:00) Auckland" },
            
            // Africa - Only Major Cities
            { "Africa/Cairo", "(UTC+02:00) Cairo" },
            { "Africa/Johannesburg", "(UTC+02:00) Johannesburg" }
        };

        var result = new List<TimezoneInfo>();
        
        // Add only the curated Windows-style timezones
        foreach (var mapping in windowsStyleTimezones)
        {
            if (_timeZoneProvider.GetZoneOrNull(mapping.Key) != null)
            {
                result.Add(new TimezoneInfo
                {
                    Id = mapping.Key,
                    DisplayName = mapping.Value
                });
            }
        }

        // Sort by UTC offset for Windows-like experience
        return result.OrderBy(t => t.DisplayName.Contains("UTC-") ? 
                                   int.Parse(t.DisplayName.Substring(t.DisplayName.IndexOf("UTC-") + 4, 2)) * -1 : 
                                   t.DisplayName.Contains("UTC+") ? 
                                   int.Parse(t.DisplayName.Substring(t.DisplayName.IndexOf("UTC+") + 4, 2)) : 0)
                     .ThenBy(t => t.DisplayName);
    }
}

public class TimezoneConversionResult
{
    public bool Success { get; set; }
    public string? ErrorMessage { get; set; }
    public DateTime OriginalDateTime { get; set; }
    public DateTime ConvertedDateTime { get; set; }
    public string OriginalTimeZone { get; set; } = string.Empty;
    public string TargetTimeZone { get; set; } = string.Empty;
    public ZonedDateTime? OriginalZonedDateTime { get; set; }
    public ZonedDateTime? TargetZonedDateTime { get; set; }
    public int ConversionId { get; set; }
    public Instant? OriginalInstant { get; set; }
}

public class TimezoneInfo
{
    public string Id { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
} 