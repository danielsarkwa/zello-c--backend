namespace Zello.Api.Controllers;

public class WeatherForecastResponse {
    public List<WeatherForecastData> Forecasts { get; set; } = new List<WeatherForecastData>();
}
