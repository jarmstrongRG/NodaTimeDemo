# NodaTime Demo Application

This is a complete full-stack .NET demonstration application showcasing advanced NodaTime usage for timezone conversions. The application includes a Web API backend, Blazor Server frontend, SQL Server database integration, and comprehensive testing suite.

## Features

- **Advanced Timezone Conversion**: Convert times between different timezones using NodaTime's `Instant` for precise moment-in-time representation
- **Dual Approach Comparison**: Implements both traditional DateTime and modern NodaTime Instant approaches for educational comparison
- **Database Storage**: All conversions are stored in SQL Server using Entity Framework Core with optimized NodaTime type mapping
- **History Tracking**: View and analyze your conversion history with detailed comparison data
- **Modern UI**: Clean, responsive Blazor Server interface with Bootstrap 5
- **RESTful API**: Full Web API with comprehensive timezone operations
- **Comprehensive Testing**: Full test suite with NUnit comparing both approaches and edge cases

## Architecture

The solution consists of four main projects:

1. **NodaTimeDemo.Api** - ASP.NET Core Web API backend
2. **NodaTimeDemo.Web** - Blazor Server frontend  
3. **NodaTimeDemo.Data** - Data access layer with Entity Framework Core
4. **NodaTimeDemo.Tests** - Comprehensive test suite with NUnit

## Key Improvements

### Enhanced Data Model
The application now demonstrates both traditional and modern approaches to timezone handling:

- **Traditional Approach**: Uses `DateTime` and `LocalDateTime` for backward compatibility
- **Modern Approach**: Uses `Instant` for precise, unambiguous moment-in-time representation
- **Dual Storage**: All conversions are stored with both approaches for comparison and validation

### Advanced NodaTime Features
- **Instant Precision**: Every conversion stores the exact moment using `Instant`
- **DST Handling**: Proper handling of Daylight Saving Time transitions with `AtLeniently()`
- **Timezone Validation**: Comprehensive timezone validation using IANA database
- **Immutable Types**: All NodaTime types are immutable, preventing timezone-related bugs

### Comprehensive Testing
- **Approach Comparison**: Tests verify both old and new approaches produce identical results
- **Edge Case Testing**: Includes DST transitions, leap years, and timezone boundary conditions
- **Performance Testing**: Validates the efficiency of the new Instant-based approach
- **In-Memory Database**: Uses Entity Framework's in-memory provider for fast, isolated tests

## Prerequisites

- .NET 8.0 SDK
- SQL Server LocalDB (or SQL Server)
- Visual Studio 2022 or VS Code

## Getting Started

### 1. Clone and Setup

```bash
git clone [your-repo-url]
cd NodaTimeDemo
```

### 2. Database Setup

The application uses SQL Server LocalDB by default. The database will be created automatically when you first run the API.

Connection string in `appsettings.json`:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=NodaTimeDemoDB;Trusted_Connection=true;MultipleActiveResultSets=true"
  }
}
```

### 3. Running the Application

You need to run both the API and Web projects simultaneously:

#### Option 1: Using Visual Studio
1. Set multiple startup projects:
   - Right-click the solution → Properties
   - Select "Multiple startup projects"
   - Set both `NodaTimeDemo.Api` and `NodaTimeDemo.Web` to "Start"
2. Press F5 to start both projects

#### Option 2: Using Terminal
Open two terminal windows:

Terminal 1 (API):
```bash
cd NodaTimeDemo.Api
dotnet run
```

Terminal 2 (Web):
```bash
cd NodaTimeDemo.Web
dotnet run
```

### 4. Running Tests

Execute the comprehensive test suite:

```bash
cd NodaTimeDemo.Tests
dotnet test
```

Or run with detailed output:
```bash
dotnet test --verbosity normal
```

### 5. Access the Application

- **Web Application**: https://localhost:5001
- **API Documentation**: https://localhost:7001/swagger

## Usage

### Converting Timezones

1. Navigate to the "Timezone Converter" page
2. Select a date and time
3. Choose the source timezone (e.g., "America/Santiago")
4. Choose the target timezone (e.g., "UTC")
5. Add optional notes
6. Click "Convert Time"

The conversion will be displayed and automatically saved to the database using both approaches.

### Viewing History

Visit the "Conversion History" page to see all your previous conversions with detailed comparison data between the old and new approaches.

## Advanced NodaTime Features Demonstrated

### Core Concepts
- **Instant**: Precise moment in time, independent of timezone
- **LocalDateTime**: Date and time without timezone information
- **ZonedDateTime**: Date and time with specific timezone
- **DateTimeZone**: Timezone information with rules and transitions

### Advanced Features
- **DST Handling**: Proper handling of "spring forward" and "fall back" transitions
- **Ambiguous Time Resolution**: Using `AtLeniently()` for DST overlap periods
- **Timezone Validation**: Comprehensive validation using IANA timezone database
- **Immutable Operations**: All transformations create new objects, preventing bugs

## Code Examples

### Enhanced Timezone Conversion Service
```csharp
public async Task<TimezoneConversionResult> ConvertTimeAsync(
    DateTime dateTime, 
    string fromTimeZone, 
    string toTimeZone, 
    string? notes = null)
{
    // Get timezone objects
    var sourceZone = _timeZoneProvider.GetZoneOrNull(fromTimeZone);
    var targetZone = _timeZoneProvider.GetZoneOrNull(toTimeZone);
    
    // Create LocalDateTime and convert to source timezone
    var localDateTime = LocalDateTime.FromDateTime(dateTime);
    var sourceZonedDateTime = sourceZone.AtLeniently(localDateTime);
    
    // Get the precise moment in time
    var instant = sourceZonedDateTime.ToInstant();
    
    // Convert to target timezone
    var targetZonedDateTime = sourceZonedDateTime.WithZone(targetZone);
    
    // Store both approaches for comparison
    var record = new TimezoneConversionRecord
    {
        // Modern approach (recommended)
        OriginalInstant = instant,
        ConvertedInstant = instant, // Same moment!
        OriginalUtcDateTime = instant.ToDateTimeUtc(),
        UsesNodaTimeInstant = true,
        
        // Traditional approach (for comparison)
        OriginalDateTime = dateTime,
        ConvertedDateTime = targetZonedDateTime.ToDateTimeUnspecified(),
        // ... other properties
    };
}
```

### Advanced Entity Framework Configuration
```csharp
// Configure NodaTime types in Entity Framework
entity.Property(e => e.OriginalLocalDateTime)
    .HasConversion(
        v => v.ToDateTimeUnspecified(),
        v => LocalDateTime.FromDateTime(v));

entity.Property(e => e.OriginalInstant)
    .HasConversion(
        v => v.HasValue ? v.Value.ToDateTimeUtc() : (DateTime?)null,
        v => v.HasValue ? Instant.FromDateTimeUtc(v.Value) : (Instant?)null);
```

### Comprehensive Testing Examples
```csharp
[Test]
[TestCase("2024-03-10 02:30:00", "America/New_York", "UTC", Description = "DST Transition")]
[TestCase("2024-07-15 14:30:00", "America/Los_Angeles", "Europe/London", Description = "Summer Time")]
public async Task ConversionApproaches_ShouldProduceSameResults(
    string dateTimeStr, string fromTz, string toTz)
{
    // Test verifies both approaches produce identical results
    var result = await _service.ConvertTimeAsync(
        DateTime.Parse(dateTimeStr), fromTz, toTz);
    
    // Both approaches should represent the same moment
    Assert.That(result.OriginalInstant.Value, 
        Is.EqualTo(result.ConvertedInstant.Value));
}
```

## API Endpoints

- `GET /api/timezone/timezones` - Get all available timezones
- `POST /api/timezone/convert` - Convert time between timezones
- `GET /api/timezone/history` - Get conversion history

## Project Structure

```
NodaTimeDemo/
├── NodaTimeDemo.Api/
│   ├── Controllers/
│   │   └── TimezoneController.cs
│   ├── Services/
│   │   └── TimezoneConversionService.cs
│   └── Program.cs
├── NodaTimeDemo.Data/
│   ├── Models/
│   │   └── TimezoneConversionRecord.cs
│   ├── Migrations/
│   │   └── 20250710134840_AddNodaTimeInstantColumns.cs
│   └── NodaTimeDemoDbContext.cs
├── NodaTimeDemo.Web/
│   ├── Pages/
│   │   ├── Index.razor
│   │   ├── TimezoneConverter.razor
│   │   └── ConversionHistory.razor
│   ├── Services/
│   │   └── TimezoneApiService.cs
│   └── Shared/
│       ├── MainLayout.razor
│       └── NavMenu.razor
├── NodaTimeDemo.Tests/
│   ├── TimezoneConversionComparisonTests.cs
│   ├── UnitTest1.cs
│   └── GlobalUsings.cs
└── README.md
```

## Database Schema

The application uses a sophisticated database schema supporting both approaches:

### TimezoneConversions Table
- **Legacy columns**: `OriginalDateTime`, `ConvertedDateTime` (for compatibility)
- **Modern columns**: `OriginalInstant`, `ConvertedInstant`, `OriginalUtcDateTime`
- **Metadata**: `UsesNodaTimeInstant` flag, `Notes` for tracking approach used

## NodaTime Benefits Demonstrated

This demo showcases why NodaTime is superior to standard .NET DateTime for timezone handling:

1. **Precision**: `Instant` provides exact moment-in-time representation
2. **Immutability**: All types are immutable, preventing timezone-related bugs
3. **Clarity**: Explicit types for different time concepts eliminate ambiguity
4. **Comprehensive**: Handles all timezone complexities including DST transitions
5. **Performance**: Optimized for timezone operations with lazy loading
6. **Accuracy**: Uses IANA timezone database for most up-to-date timezone rules
7. **Testability**: Easy to test with predictable, immutable operations

## Testing Strategy

The comprehensive test suite demonstrates:

- **Approach Comparison**: Validates both old and new approaches produce identical results
- **Edge Case Handling**: Tests DST transitions, leap years, and timezone boundaries
- **Performance Validation**: Compares performance characteristics of both approaches
- **Data Integrity**: Ensures database storage and retrieval maintains precision
- **Error Handling**: Validates proper exception handling for invalid timezones

## Migration Notes

The application demonstrates a real-world migration scenario:

1. **Backward Compatibility**: Maintains existing DateTime-based columns
2. **Gradual Migration**: New records use both approaches for comparison
3. **Validation**: Tests ensure both approaches produce identical results
4. **Performance**: New approach optimizes for timezone operations

## License

This project is for educational purposes and demonstrates advanced NodaTime integration in a full-stack .NET application with comprehensive testing. 