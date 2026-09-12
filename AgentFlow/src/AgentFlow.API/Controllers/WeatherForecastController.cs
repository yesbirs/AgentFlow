using Microsoft.AspNetCore.Mvc;

namespace AgentFlow.API.Controllers
{
    [ApiController]
    [Route("[controller]")]
    /// <summary>
    /// Simple controller used by the sample application to return weather forecasts.
    /// </summary>
    public class WeatherForecastController : ControllerBase
    {
        /// <summary>
        /// Static set of human-friendly weather summaries used to populate sample data.
        /// </summary>
        private static readonly string[] Summaries = new[]
        {
            "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
        };

        private readonly ILogger<WeatherForecastController> _logger;

        /// <summary>
        /// Creates a new instance of the controller.
        /// </summary>
        public WeatherForecastController(ILogger<WeatherForecastController> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Returns a small collection of sample <see cref="WeatherForecast"/> items.
        /// </summary>
        [HttpGet(Name = "GetWeatherForecast")]
        public IEnumerable<WeatherForecast> Get()
        {
            return Enumerable.Range(1, 5).Select(index => new WeatherForecast
            {
                Date = DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
                TemperatureC = Random.Shared.Next(-20, 55),
                Summary = Summaries[Random.Shared.Next(Summaries.Length)]
            })
            .ToArray();
        }
    }
}
