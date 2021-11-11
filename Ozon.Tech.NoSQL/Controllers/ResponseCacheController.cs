using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Ozon.Tech.NoSQL.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ResponseCacheController : ControllerBase
    {
        private readonly ILogger<ResponseCacheController> _logger;
        private readonly WeatherApiClient _weatherApiClient;

        public ResponseCacheController(WeatherApiClient weatherApiClient, ILogger<ResponseCacheController> logger)
        {
            _weatherApiClient = weatherApiClient;
            _logger = logger;
        }

        [HttpGet]
        [ResponseCache(Location = ResponseCacheLocation.Any, Duration = 10)]
        public async Task<IEnumerable<WeatherForecast>> Get()
        {
            var weatherForecast = await _weatherApiClient.GetWeatherForecast();
            _logger.LogInformation("Loaded {Count} items", weatherForecast.Count);
            return weatherForecast;
        }
    }
}