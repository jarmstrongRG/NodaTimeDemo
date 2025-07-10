using Microsoft.AspNetCore.Mvc;
using NodaTimeDemo.Api.Services;
using NodaTimeDemo.Data.Models;

namespace NodaTimeDemo.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TimezoneController : ControllerBase
{
    private readonly TimezoneConversionService _timezoneService;

    public TimezoneController(TimezoneConversionService timezoneService)
    {
        _timezoneService = timezoneService;
    }

    [HttpPost("convert")]
    public async Task<ActionResult<TimezoneConversionResult>> ConvertTime([FromBody] TimezoneConversionRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var result = await _timezoneService.ConvertTimeAsync(
            request.DateTime,
            request.FromTimeZone,
            request.ToTimeZone,
            request.Notes);

        if (!result.Success)
        {
            return BadRequest(result.ErrorMessage);
        }

        return Ok(result);
    }

    [HttpGet("timezones")]
    public ActionResult<IEnumerable<TimezoneInfo>> GetAvailableTimezones()
    {
        var timezones = _timezoneService.GetAvailableTimeZones();
        return Ok(timezones);
    }

    [HttpGet("history")]
    public async Task<ActionResult<IEnumerable<TimezoneConversionRecord>>> GetConversionHistory([FromQuery] int limit = 50)
    {
        var history = await _timezoneService.GetConversionHistoryAsync(limit);
        return Ok(history);
    }
}

public class TimezoneConversionRequest
{
    public DateTime DateTime { get; set; }
    public string FromTimeZone { get; set; } = string.Empty;
    public string ToTimeZone { get; set; } = string.Empty;
    public string? Notes { get; set; }
} 