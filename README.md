# NodaTime Demo Application

This is a complete full-stack .NET demonstration application showcasing the use of NodaTime for timezone conversions. The application includes a Web API backend, Blazor Server frontend, and SQL Server database integration.

## Features

- **Timezone Conversion**: Convert times between different timezones using NodaTime
- **Database Storage**: All conversions are stored in SQL Server using Entity Framework Core
- **History Tracking**: View and analyze your conversion history
- **Modern UI**: Clean, responsive Blazor Server interface with Bootstrap 5
- **API Integration**: RESTful API for timezone operations

## Architecture

The solution consists of three main projects:

1. **NodaTimeDemo.Api** - ASP.NET Core Web API backend
2. **NodaTimeDemo.Web** - Blazor Server frontend
3. **NodaTimeDemo.Data** - Data access layer with Entity Framework Core

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

### 4. Access the Application

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

The conversion will be displayed and automatically saved to the database.

### Viewing History

Visit the "Conversion History" page to see all your previous conversions with statistics.

## Key NodaTime Features Demonstrated

- **DateTimeZone**: Working with timezone objects
- **LocalDateTime**: Representing date/time without timezone
- **ZonedDateTime**: Representing date/time with timezone
- **Timezone Conversions**: Converting between different timezones
- **Database Integration**: Storing NodaTime types in SQL Server

## Code Examples

### Timezone Conversion Service
```csharp
public async Task<TimezoneConversionResult> ConvertTimeAsync(
    DateTime dateTime, 
    string fromTimeZone, 
    string toTimeZone)
{
    var sourceZone = _timeZoneProvider.GetZoneOrNull(fromTimeZone);
    var targetZone = _timeZoneProvider.GetZoneOrNull(toTimeZone);
    
    var localDateTime = LocalDateTime.FromDateTime(dateTime);
    var sourceZonedDateTime = sourceZone.AtLeniently(localDateTime);
    var targetZonedDateTime = sourceZonedDateTime.WithZone(targetZone);
    
    return new TimezoneConversionResult
    {
        OriginalDateTime = dateTime,
        ConvertedDateTime = targetZonedDateTime.ToDateTimeUnspecified(),
        // ... other properties
    };
}
```

### Entity Framework Configuration
```csharp
entity.Property(e => e.OriginalLocalDateTime)
    .HasConversion(
        v => v.ToDateTimeUnspecified(),
        v => LocalDateTime.FromDateTime(v));
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
└── README.md
```

## NodaTime Benefits

This demo showcases why NodaTime is superior to standard .NET DateTime for timezone handling:

1. **Clear API**: Explicit types for different time concepts
2. **Immutability**: All types are immutable, preventing bugs
3. **Comprehensive**: Handles all timezone complexities
4. **Performance**: Optimized for timezone operations
5. **Accurate**: Uses IANA timezone database

## License

This project is for educational purposes and demonstrates NodaTime integration in a full-stack .NET application. 