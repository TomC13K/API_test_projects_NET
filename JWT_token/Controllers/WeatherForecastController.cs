using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace WebApi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    //[Authorize]                              //can use Authorize here or on specific endpoint inside controller
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

        //Authentication is just validation of JWT

        // if controller has [Authorized], this will let this endpoint be without Auth
        //[HttpGet(Name = "GetWeatherForecast"), AllowAnonymous] 

        // Authorize endpoint without roles 
        //[HttpGet(Name = "GetWeatherForecast"), Authorize]

        [HttpGet(Name = "GetWeatherForecast"), Authorize(Roles ="Admin")] 
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