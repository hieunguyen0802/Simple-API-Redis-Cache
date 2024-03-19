using Microsoft.AspNetCore.Mvc;
using StackExchange.Redis;
using NRedisStack;
using NRedisStack.RedisStackCommands;

namespace SimpleApiRedis.Controllers;

[ApiController]
[Route("[controller]")]
public class WeatherForecastController : ControllerBase
{
    private static readonly string[] Summaries = new[]
    {
        "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
    };

    private readonly ILogger<WeatherForecastController> _logger;

    public WeatherForecastController(ILogger<WeatherForecastController> logger)
    {
        _logger = logger;
    }

    [HttpGet(Name = "GetWeatherForecast")]
    public IActionResult Get()
    {
        ConnectionMultiplexer redis = ConnectionMultiplexer.Connect("localhost:90");
        IDatabase db = redis.GetDatabase();
        db.StringSet("foo", "bar");
        Console.WriteLine(db.StringGet("foo"));
        var res = Enumerable.Range(1, 5).Select(index => new WeatherForecast
        {
            Date = DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
            TemperatureC = Random.Shared.Next(-20, 55),
            Summary = Summaries[Random.Shared.Next(Summaries.Length)]
        })
        .ToArray();
        db.StringSet("CachedArray", res.ToString());
        Console.WriteLine(db.StringGet("CachedArray"));

        return Ok(res);
    }
}
