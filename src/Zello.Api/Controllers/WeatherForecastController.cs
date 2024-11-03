using Microsoft.AspNetCore.Mvc;

namespace Zello.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class WeatherForecastController : ControllerBase
{
    private static readonly string[] Summaries =
    {
        "Freezing",
        "Bracing",
        "Chilly",
        "Cool",
        "Mild",
        "Warm",
        "Balmy",
        "Hot",
        "Sweltering",
        "Scorching",
    };

    [HttpGet]
    [ProducesResponseType(typeof(WeatherForecastResponse), StatusCodes.Status200OK)]
    public IActionResult Get()
    {
        var forecast = Enumerable
            .Range(1, 5)
            .Select(index => new WeatherForecastData
            {
                Date = DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
                TemperatureC = Random.Shared.Next(-20, 55),
                Summary = Summaries[Random.Shared.Next(Summaries.Length)],
            })
            .ToList();

        var response = new WeatherForecastResponse { Forecasts = forecast };

        return Ok(response);
    }
}
