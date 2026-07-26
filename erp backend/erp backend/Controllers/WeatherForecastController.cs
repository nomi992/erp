using erp_backend.Models;
using erp_backend.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace erp_backend.Controllers
{
    [ApiController]
    [Route("[controller]")]
    [Authorize]
    public class WeatherForecastController : ControllerBase
    {
        private readonly IRepository<WeatherForecast> _repository;

        public WeatherForecastController(IRepository<WeatherForecast> repository)
        {
            _repository = repository;
        }

        [HttpGet(Name = "GetWeatherForecast")]
        public async Task<ActionResult<ApiResponse<IReadOnlyList<WeatherForecast>>>> GetAll()
        {
            var forecasts = await _repository.GetAllAsync();
            return Ok(ApiResponse<IReadOnlyList<WeatherForecast>>.Ok(forecasts));
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<ApiResponse<WeatherForecast>>> GetById(int id)
        {
            var forecast = await _repository.GetByIdAsync(id);
            if (forecast is null)
            {
                return NotFound(ApiResponse<WeatherForecast>.Fail(
                    $"Weather forecast with id {id} was not found.", StatusCodes.Status404NotFound));
            }

            return Ok(ApiResponse<WeatherForecast>.Ok(forecast));
        }

        [HttpPost]
        public async Task<ActionResult<ApiResponse<WeatherForecast>>> Create(WeatherForecast forecast)
        {
            await _repository.AddAsync(forecast);
            await _repository.SaveChangesAsync();

            return CreatedAtAction(nameof(GetById), new { id = forecast.Id },
                ApiResponse<WeatherForecast>.Ok(forecast, "Weather forecast created.", StatusCodes.Status201Created));
        }

        [HttpPut("{id:int}")]
        public async Task<ActionResult<ApiResponse<WeatherForecast>>> Update(int id, WeatherForecast forecast)
        {
            var existing = await _repository.GetByIdAsync(id);
            if (existing is null)
            {
                return NotFound(ApiResponse<WeatherForecast>.Fail(
                    $"Weather forecast with id {id} was not found.", StatusCodes.Status404NotFound));
            }

            existing.Date = forecast.Date;
            existing.TemperatureC = forecast.TemperatureC;
            existing.Summary = forecast.Summary;

            _repository.Update(existing);
            await _repository.SaveChangesAsync();

            return Ok(ApiResponse<WeatherForecast>.Ok(existing, "Weather forecast updated."));
        }

        [HttpDelete("{id:int}")]
        public async Task<ActionResult<ApiResponse<object>>> Delete(int id)
        {
            var existing = await _repository.GetByIdAsync(id);
            if (existing is null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    $"Weather forecast with id {id} was not found.", StatusCodes.Status404NotFound));
            }

            _repository.Remove(existing);
            await _repository.SaveChangesAsync();

            return Ok(ApiResponse<object>.Ok(null, "Weather forecast deleted."));
        }
    }
}
