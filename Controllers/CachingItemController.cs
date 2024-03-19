using Microsoft.AspNetCore.Mvc;
using StackExchange.Redis;
using SimpleApiRedis.Services;


namespace SimpleApiRedis.Controllers;

[ApiController]
[Route("[controller]")]
public class CachingItemController : ControllerBase
{
    private static readonly string[] Summaries = new[]
    {
        "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
    };

    private readonly RedisCacheService _cache;
    private readonly ILogger<CachingItemController> _logger;
    public bool IsFromCache { get; set; } = false;

    public CachingItemController(ILogger<CachingItemController> logger, RedisCacheService cache)
    {
        _logger = logger;
        _cache = cache;
    }

    [HttpGet(Name = "GetCachingItem")]
    public IActionResult Get()
    {
        var res = Enumerable.Range(1, 5).Select(index => new WeatherForecast
        {
            Date = DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
            TemperatureC = Random.Shared.Next(-20, 55),
            Summary = Summaries[Random.Shared.Next(Summaries.Length)]
        })
               .ToArray();
        
        var instanceId = GetInstanceId();
        var cacheKey = $"Games_Cache_{instanceId}";

        var weather = _cache.GetCachedData<WeatherForecast>(cacheKey);

        if (weather is null) {
            _cache.SetCachedData(cacheKey, res, TimeSpan.FromSeconds(60));
            IsFromCache = false;
        } else {
            IsFromCache = true;
        }

        return Ok( new {
            IsFromCache=IsFromCache,
            cacheKey = cacheKey,
            data = res
        });
    }

    public IActionResult RemoveCache()
    {
        var instanceId = GetInstanceId();
        var cacheKey = $"Games_Cache_{instanceId}";

        _cache.RemoveCachedData(cacheKey);
        
        return Ok (_cache.GetCachedData<WeatherForecast>(cacheKey));
    }

    private string GetInstanceId()
    {
        var instanceId = HttpContext.Session.GetString("InstanceId");

        if (string.IsNullOrEmpty(instanceId))
        {
            instanceId = Guid.NewGuid().ToString();
            HttpContext.Session.SetString("InstanceId", instanceId);
        }

        return instanceId;
    }
}
