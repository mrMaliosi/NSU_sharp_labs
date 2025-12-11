using Microsoft.AspNetCore.Mvc;
using Lab1.DiningPhilosophers;
using TableService.Services;
using TableService.Models;

namespace TableService.Controllers;

[ApiController]
[Route("api/[controller]")]
public class MetricsController : ControllerBase
{
    private readonly IMetricsCalculator _metricsCalculator;
    private readonly IForkManager _forkManager;
    private readonly ILogger<MetricsController> _logger;

    public MetricsController(
        IMetricsCalculator metricsCalculator,
        IForkManager forkManager,
        ILogger<MetricsController> logger)
    {
        _metricsCalculator = metricsCalculator;
        _forkManager = forkManager;
        _logger = logger;
    }

    [HttpPost("meal")]
    public IActionResult RecordMeal([FromBody] RecordMealRequest request)
    {
        try
        {
            var philosophers = _tableStateService.GetRegisteredPhilosophers();
            var philosopher = philosophers.FirstOrDefault(p => p.Name == request.PhilosopherName);
            if (philosopher != null)
            {
                philosopher.MealsEaten++;
            }
            _metricsCalculator.OnMeal(request.PhilosopherName);
            _logger.LogInformation("Записан прием пищи для философа {PhilosopherName}", request.PhilosopherName);
            return Ok(new { success = true });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при записи приема пищи");
            return StatusCode(500, new { error = ex.Message });
        }
    }

    [HttpPost("waiting-time")]
    public IActionResult UpdateWaitingTime([FromBody] UpdateWaitingTimeRequest request)
    {
        try
        {
            var philosophers = _tableStateService.GetRegisteredPhilosophers();
            var philosopher = philosophers.FirstOrDefault(p => p.Name == request.PhilosopherName);
            if (philosopher != null)
            {
                philosopher.TotalWaitingTimeMs = request.WaitingTimeMs;
            }
            return Ok(new { success = true });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при обновлении времени ожидания");
            return StatusCode(500, new { error = ex.Message });
        }
    }

    [HttpGet]
    public IActionResult GetMetrics()
    {
        try
        {
            var forks = _forkManager.GetAllForks();
            var philosophers = _tableStateService.GetRegisteredPhilosophers();
            
            // Вычисляем метрики
            var totalMeals = philosophers.Sum(p => p.MealsEaten);
            var throughput = _metricsCalculator.TotalSimulationTimeMs > 0 
                ? (double)totalMeals / _metricsCalculator.TotalSimulationTimeMs 
                : 0;
            
            var avgWaitingTimes = philosophers.ToDictionary(
                p => p.Name,
                p => p.MealsEaten > 0 ? (double)p.TotalWaitingTimeMs / p.MealsEaten : 0
            );
            
            var forkUtilization = _metricsCalculator.GetForkUtilization();
            
            return Ok(new MetricsResponse
            {
                TotalSimulationTimeMs = _metricsCalculator.TotalSimulationTimeMs,
                Throughput = throughput,
                AverageWaitingTimes = avgWaitingTimes,
                ForkUtilization = forkUtilization,
                MealsByPhilosopher = philosophers.ToDictionary(
                    p => p.Name,
                    p => p.MealsEaten
                )
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при получении метрик");
            return StatusCode(500, new { error = ex.Message });
        }
    }
}

