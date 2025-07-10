using System.Text.Json;
using System.Text;

namespace NodaTimeDemo.Web.Services;

public class TimezoneApiService
{
    private readonly HttpClient _httpClient;
    private readonly JsonSerializerOptions _jsonOptions;

    public TimezoneApiService(HttpClient httpClient)
    {
        _httpClient = httpClient;
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };
    }

    public async Task<IEnumerable<string>> GetAvailableTimezonesAsync()
    {
        try
        {
            var response = await _httpClient.GetStringAsync("api/timezone/timezones");
            var timezones = JsonSerializer.Deserialize<IEnumerable<string>>(response, _jsonOptions);
            return timezones ?? Enumerable.Empty<string>();
        }
        catch (Exception)
        {
            return Enumerable.Empty<string>();
        }
    }

    public async Task<TimezoneConversionResult?> ConvertTimeAsync(TimezoneConversionRequest request)
    {
        try
        {
            var json = JsonSerializer.Serialize(request, _jsonOptions);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            
            var response = await _httpClient.PostAsync("api/timezone/convert", content);
            
            if (response.IsSuccessStatusCode)
            {
                var responseJson = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<TimezoneConversionResult>(responseJson, _jsonOptions);
            }
            
            return new TimezoneConversionResult
            {
                Success = false,
                ErrorMessage = "Failed to convert timezone"
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
        try
        {
            var response = await _httpClient.GetStringAsync($"api/timezone/history?limit={limit}");
            var history = JsonSerializer.Deserialize<IEnumerable<TimezoneConversionRecord>>(response, _jsonOptions);
            return history ?? Enumerable.Empty<TimezoneConversionRecord>();
        }
        catch (Exception)
        {
            return Enumerable.Empty<TimezoneConversionRecord>();
        }
    }
}

// Data models for API communication
public class TimezoneConversionRequest
{
    public DateTime DateTime { get; set; }
    public string FromTimeZone { get; set; } = string.Empty;
    public string ToTimeZone { get; set; } = string.Empty;
    public string? Notes { get; set; }
}

public class TimezoneConversionResult
{
    public bool Success { get; set; }
    public string? ErrorMessage { get; set; }
    public DateTime OriginalDateTime { get; set; }
    public DateTime ConvertedDateTime { get; set; }
    public string OriginalTimeZone { get; set; } = string.Empty;
    public string TargetTimeZone { get; set; } = string.Empty;
    public int ConversionId { get; set; }
}

public class TimezoneConversionRecord
{
    public int Id { get; set; }
    public string OriginalTimeZone { get; set; } = string.Empty;
    public string TargetTimeZone { get; set; } = string.Empty;
    public DateTime OriginalDateTime { get; set; }
    public DateTime ConvertedDateTime { get; set; }
    public DateTime CreatedAt { get; set; }
    public string? Notes { get; set; }
} 