using FastEndpoints;

namespace Zello.Api;

public sealed class GetWeatherForecast : EndpointWithoutRequest<WeatherForecastResponse> {
    private static readonly string[] Summaries = {
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

    public override void Configure() {
        Get("/weatherforecast");
        AllowAnonymous();
        Description(d => d
            .WithTags("Weather")
            .WithSummary("Get weather forecast"));
    }

    public override async Task HandleAsync(CancellationToken ct) {
        var forecast = Enumerable.Range(1, 5)
            .Select(index => new WeatherForecast {
                Date = DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
                TemperatureC = Random.Shared.Next(-20, 55),
                Summary = Summaries[Random.Shared.Next(Summaries.Length)],
            })
            .ToList();

        await SendAsync(
            new WeatherForecastResponse {
                Forecasts = forecast,
            },
            cancellation: ct);
    }
}