using Microsoft.Extensions.Caching.Distributed;
using System.Text.Json;

namespace SimpleApiRedis.Services
{
    public class RedisCacheService
    {
        private readonly IDistributedCache _cache;

        public RedisCacheService(IDistributedCache cache)
        {
            _cache = cache;
        }

        public WeatherForecast GetCachedData<WeatherForecast>(string key)
        {
            var jsonData = _cache.GetString(key);

            if (jsonData == null)
                return default(WeatherForecast);

            return JsonSerializer.Deserialize<WeatherForecast>(jsonData);
        }

        public void SetCachedData<WeatherForecast>(string key, WeatherForecast data, TimeSpan cacheDuration)
        {
            var options = new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = cacheDuration,
                SlidingExpiration = cacheDuration
            };

            var jsonData = JsonSerializer.Serialize(data);
            _cache.SetString(key, jsonData, options);
        }

        public void RemoveCachedData(string key)
        {
            _cache.Remove(key);
        }
    }
}