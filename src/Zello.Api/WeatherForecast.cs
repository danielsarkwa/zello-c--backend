using System;

namespace Zello.Api {
    // Split into separate files to fix SA1649 (File name should match first type)
    public class WeatherForecast {
        public DateOnly Date { get; set; }
        public int TemperatureC { get; set; }
        public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
        public string Summary { get; set; } = string.Empty;
    }
}