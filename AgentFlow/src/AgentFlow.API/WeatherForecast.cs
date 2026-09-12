namespace AgentFlow.API
{
    /// <summary>
    /// Simple DTO used by the sample weather endpoint.
    /// </summary>
    public class WeatherForecast
    {
        /// <summary>
        /// Forecast date in UTC.
        /// </summary>
        public DateOnly Date { get; set; }

        /// <summary>
        /// Temperature in degrees Celsius.
        /// </summary>
        public int TemperatureC { get; set; }

        /// <summary>
        /// Temperature in degrees Fahrenheit (computed).
        /// </summary>
        public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);

        /// <summary>
        /// Short description of the weather.
        /// </summary>
        public string? Summary { get; set; }
    }
}
