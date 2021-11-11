using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace Ozon.Tech.NoSQL.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class MemoryCacheController : ControllerBase
    {
        private readonly IMemoryCache _cache;
        private readonly ILogger<MemoryCacheController> _logger;
        private readonly SemaphoreSlim _semaphore = new(1);
        private readonly WeatherApiClient _weatherApiClient;

        public MemoryCacheController(
            IMemoryCache memoryCache,
            WeatherApiClient weatherApiClient,
            ILogger<MemoryCacheController> logger)
        {
            _cache = memoryCache;
            _weatherApiClient = weatherApiClient;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IEnumerable<WeatherForecast>> Get()
        {
            // Используем Microsoft.Extensions.Caching.Memory

            const string cacheKey = "getWeatherForecast";

            if (_cache.TryGetValue(cacheKey, out List<WeatherForecast> weatherForecast))
            {
                _logger.LogInformation("Return result from cache");
                return weatherForecast;
            }

            await _semaphore.WaitAsync();
            try
            {
                // double check locking
                if (_cache.TryGetValue(cacheKey, out weatherForecast))
                {
                    _logger.LogInformation("Return result from cache (inside the lock)");
                    return weatherForecast;
                }

                weatherForecast = await _weatherApiClient.GetWeatherForecast();

                var cacheEntryOptions = new MemoryCacheEntryOptions
                {
                    AbsoluteExpiration = DateTime.Now.AddSeconds(10),
                    Priority = CacheItemPriority.Normal
                };

                _cache.Set(cacheKey, weatherForecast, cacheEntryOptions);

                _logger.LogInformation("Loaded {Count} items", weatherForecast.Count);
                return weatherForecast;
            }
            finally
            {
                _semaphore.Release();
            }
        }
    }
}