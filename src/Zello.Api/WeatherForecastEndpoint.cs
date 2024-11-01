using FastEndpoints;

public class WeatherForecastEndpoint : EndpointWithoutRequest<WeatherForecastResponse> {
    public override void Configure() {
        Get("/weatherforecast");
        AllowAnonymous();
    }

    public override async Task HandleAsync(CancellationToken ct) {
        var summaries = new[]
            { "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching" };
        var forecast = Enumerable.Range(1, 5).Select(index =>
                new WeatherForecast
                (
                    DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
                    Random.Shared.Next(-20, 55),
                    summaries[Random.Shared.Next(summaries.Length)]
                ))
            .ToArray();

        await SendAsync(new WeatherForecastResponse { Forecasts = forecast });
    }
}

public record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary) {
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}

public class WeatherForecastResponse {
    public WeatherForecast[] Forecasts { get; set; }
}