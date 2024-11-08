namespace Zello.Api.Controllers;

public class WeatherForecastResponse {
    public IList<WeatherForecastData> Forecasts { get; set; } = new List<WeatherForecastData>();
}
