using Microsoft.AspNetCore.Mvc;
using Pro219.API.DTOs;
using Pro219.API.Utilities;
using System.Text;

namespace Pro219.BUS.Controllers
{
    [ApiController]
    [Route("common")]
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

        [HttpPost("send-email")]
        public async Task<bool> SendEmailToAddress([FromBody] EmailDTO emailDTO)
        {
            UtilityFunc utilityFunc = new UtilityFunc();
            bool sentResult = await utilityFunc.SendEmailToAddress(emailDTO.Email, emailDTO.Name, emailDTO.Subject, emailDTO.Body, emailDTO.BodyHTML);
            return sentResult;
        }
    }
}
