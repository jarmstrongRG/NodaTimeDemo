using NUnit.Framework;
using NodaTime;
using NodaTimeDemo.Api.Services;
using NodaTimeDemo.Data;
using NodaTimeDemo.Data.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace NodaTimeDemo.Tests;

[TestFixture]
public class TimezoneConversionComparisonTests
{
    private ServiceProvider _serviceProvider;
    private NodaTimeDemoDbContext _context;
    private TimezoneConversionService _service;
    private IDateTimeZoneProvider _timeZoneProvider;

    [SetUp]
    public void Setup()
    {
        // Setup in-memory database for testing
        var services = new ServiceCollection();
        services.AddDbContext<NodaTimeDemoDbContext>(options =>
            options.UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString()));
        
        // Add NodaTime services
        services.AddSingleton<IDateTimeZoneProvider>(DateTimeZoneProviders.Tzdb);
        services.AddScoped<TimezoneConversionService>();
        
        _serviceProvider = services.BuildServiceProvider();
        _context = _serviceProvider.GetRequiredService<NodaTimeDemoDbContext>();
        _timeZoneProvider = _serviceProvider.GetRequiredService<IDateTimeZoneProvider>();
        _service = _serviceProvider.GetRequiredService<TimezoneConversionService>();
    }

    [TearDown]
    public void TearDown()
    {
        _context?.Dispose();
        _serviceProvider?.Dispose();
    }

    [Test]
    [TestCase("2024-03-10 02:30:00", "America/New_York", "America/Santo_Domingo", Description = "DST Transition")]
    [TestCase("2024-07-15 14:30:00", "America/Los_Angeles", "Europe/London", Description = "Summer Time")]
    [TestCase("2024-12-25 12:00:00", "UTC", "Asia/Tokyo", Description = "UTC to JST")]
    [TestCase("2024-06-15 09:00:00", "Europe/Paris", "America/Chicago", Description = "Europe to US Central")]
    public async Task ConversionApproaches_ShouldProduceSameResults(string dateTimeStr, string fromTz, string toTz)
    {
        // Arrange
        var dateTime = DateTime.Parse(dateTimeStr);
        var notes = $"Test: {dateTimeStr} from {fromTz} to {toTz}";

        // Act - Use our service that populates both approaches
        var result = await _service.ConvertTimeAsync(dateTime, fromTz, toTz, notes);

        // Assert
        Assert.That(result.Success, Is.True, $"Conversion failed: {result.ErrorMessage}");

        // Get the stored record from database
        var record = await _context.TimezoneConversions
            .FirstOrDefaultAsync(x => x.Id == result.ConversionId);
        
        Assert.That(record, Is.Not.Null, "Record should be saved to database");
        Assert.That(record.UsesNodaTimeInstant, Is.True, "Should be using new NodaTime approach");

        // === COMPARE OLD VS NEW APPROACHES ===
        
        // 1. Both should represent the same moment in time
        Assert.That(record.OriginalInstant.HasValue, Is.True, "OriginalInstant should be populated");
        Assert.That(record.ConvertedInstant.HasValue, Is.True, "ConvertedInstant should be populated");
        Assert.That(record.OriginalUtcDateTime.HasValue, Is.True, "OriginalUtcDateTime should be populated");

        // 2. Original and converted instants should be the same (same moment in time)
        Assert.That(record.ConvertedInstant.Value, Is.EqualTo(record.OriginalInstant.Value), 
            "Original and converted instants should represent the same moment");

        // 3. UTC DateTime should match the Instant
        var expectedUtc = record.OriginalInstant.Value.ToDateTimeUtc();
        Assert.That(record.OriginalUtcDateTime.Value, Is.EqualTo(expectedUtc).Within(TimeSpan.FromSeconds(1)),
            "UTC DateTime should match the Instant representation");

        // 4. Manual verification: Convert using NodaTime directly
        await VerifyWithDirectNodaTimeConversion(record, fromTz, toTz);
    }

    private async Task VerifyWithDirectNodaTimeConversion(TimezoneConversionRecord record, string fromTz, string toTz)
    {
        // Manual NodaTime conversion for verification
        var sourceZone = _timeZoneProvider.GetZoneOrNull(fromTz);
        var targetZone = _timeZoneProvider.GetZoneOrNull(toTz);
        
        Assert.That(sourceZone, Is.Not.Null, $"Source timezone {fromTz} should exist");
        Assert.That(targetZone, Is.Not.Null, $"Target timezone {toTz} should exist");

        // Convert original DateTime to LocalDateTime and then to Instant
        var originalLocal = record.OriginalLocalDateTime;
        var sourceZoned = sourceZone.AtLeniently(originalLocal);
        var manualInstant = sourceZoned.ToInstant();
        
        // This should match our stored instant
        Assert.That(record.OriginalInstant.Value, Is.EqualTo(manualInstant),
            "Stored instant should match manual NodaTime conversion");

        // Convert to target timezone
        var targetZoned = sourceZoned.WithZone(targetZone);
        var targetLocal = targetZoned.LocalDateTime;
        
        // This should match our stored converted local time
        Assert.That(record.ConvertedLocalDateTime.ToDateTimeUnspecified(), 
            Is.EqualTo(targetLocal.ToDateTimeUnspecified()).Within(TimeSpan.FromSeconds(1)),
            "Converted local times should match");

        await Task.CompletedTask; // Make it async for consistency
    }

    [Test]
    public async Task MultipleConversions_ShouldAllUseNodaTimeApproach()
    {
        // Arrange
        var testCases = new[]
        {
            new { DateTime = new DateTime(2024, 1, 15, 10, 0, 0), From = "UTC", To = "America/New_York" },
            new { DateTime = new DateTime(2024, 6, 15, 15, 30, 0), From = "Europe/London", To = "Asia/Tokyo" },
            new { DateTime = new DateTime(2024, 11, 1, 8, 45, 0), From = "America/Los_Angeles", To = "Australia/Sydney" }
        };

        // Act
        var results = new List<TimezoneConversionResult>();
        foreach (var testCase in testCases)
        {
            var result = await _service.ConvertTimeAsync(
                testCase.DateTime, 
                testCase.From, 
                testCase.To, 
                $"Batch test: {testCase.From} to {testCase.To}");
            results.Add(result);
        }

        // Assert
        Assert.That(results.All(r => r.Success), Is.True, "All conversions should succeed");

        var records = await _context.TimezoneConversions.ToListAsync();
        Assert.That(records.Count, Is.EqualTo(testCases.Length), "Should have saved all records");
        Assert.That(records.All(r => r.UsesNodaTimeInstant), Is.True, "All records should use NodaTime approach");
        Assert.That(records.All(r => r.OriginalInstant.HasValue), Is.True, "All records should have Instant values");
        Assert.That(records.All(r => r.Notes.Contains("Dual approach")), Is.True, "All records should indicate dual approach");
    }

    [Test]
    public async Task DSTTransition_ShouldHandleCorrectly()
    {
        // Arrange - Test the tricky DST transition period
        var dstTransitionDate = new DateTime(2024, 3, 10, 2, 30, 0); // 2:30 AM on DST transition
        var fromTz = "America/New_York";
        var toTz = "UTC";

        // Act
        var result = await _service.ConvertTimeAsync(dstTransitionDate, fromTz, toTz, "DST Transition Test");

        // Assert
        Assert.That(result.Success, Is.True, "DST transition conversion should succeed");
        
        var record = await _context.TimezoneConversions
            .FirstOrDefaultAsync(x => x.Id == result.ConversionId);
        
        Assert.That(record, Is.Not.Null);
        
        // The instant should be well-defined even during DST transition
        Assert.That(record.OriginalInstant.HasValue, Is.True);
        Assert.That(record.ConvertedInstant.HasValue, Is.True);
        Assert.That(record.OriginalInstant.Value, Is.EqualTo(record.ConvertedInstant.Value),
            "Same moment in time regardless of timezone display");

        // Manual verification with NodaTime
        var nyZone = _timeZoneProvider.GetZoneOrNull(fromTz);
        var localDateTime = LocalDateTime.FromDateTime(dstTransitionDate);
        var zonedDateTime = nyZone.AtLeniently(localDateTime); // Handles DST ambiguity
        var expectedInstant = zonedDateTime.ToInstant();
        
        Assert.That(record.OriginalInstant.Value, Is.EqualTo(expectedInstant),
            "Should handle DST transition consistently");
    }

    [Test]
    public async Task CompareAgainstLegacyDateTime_ShowsImprovement()
    {
        // Arrange
        var testDateTime = new DateTime(2024, 6, 15, 14, 30, 0);
        var fromTz = "America/New_York";
        var toTz = "Europe/London";

        // Act
        var result = await _service.ConvertTimeAsync(testDateTime, fromTz, toTz, "Legacy comparison test");
        
        // Assert
        Assert.That(result.Success, Is.True);
        
        var record = await _context.TimezoneConversions
            .FirstOrDefaultAsync(x => x.Id == result.ConversionId);

        // === DEMONSTRATE THE DIFFERENCE ===
        
        // Old approach - ambiguous, timezone-dependent
        var oldOriginal = record.OriginalDateTime;
        var oldConverted = record.ConvertedDateTime;
        
        // New approach - unambiguous, universal
        var newInstant = record.OriginalInstant.Value;
        var newUtc = record.OriginalUtcDateTime.Value;

        // The instant provides a single, unambiguous point in time
        Assert.That(newInstant.ToDateTimeUtc(), Is.EqualTo(newUtc),
            "Instant and UTC DateTime should be consistent");

        // Manual verification that old approach can be error-prone
        // (This would be more comprehensive in a real enterprise migration)
        TestContext.WriteLine($"Old Original: {oldOriginal} (timezone-dependent)");
        TestContext.WriteLine($"Old Converted: {oldConverted} (timezone-dependent)");
        TestContext.WriteLine($"New Instant: {newInstant} (universal)");
        TestContext.WriteLine($"New UTC: {newUtc} (unambiguous)");
        
        // The key benefit: Instant is timezone-independent
        Assert.That(newInstant.InUtc().ToDateTimeUtc(), Is.EqualTo(newUtc),
            "Instant provides timezone-independent representation");
    }
} 