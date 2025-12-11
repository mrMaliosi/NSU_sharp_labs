using Microsoft.AspNetCore.Mvc;
using Lab1.DiningPhilosophers;
using TableService.Services;
using TableService.Models;

namespace TableService.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PhilosophersController : ControllerBase
{
    private readonly TableStateService _tableStateService;
    private readonly ILogger<PhilosophersController> _logger;

    public PhilosophersController(
        TableStateService tableStateService,
        ILogger<PhilosophersController> logger)
    {
        _tableStateService = tableStateService;
        _logger = logger;
    }

    [HttpPost("register")]
    public IActionResult RegisterPhilosopher([FromBody] RegisterPhilosopherRequest request)
    {
        try
        {
            var philosopher = new PhilosopherProxy(request.PhilosopherId, request.PhilosopherName);
            _tableStateService.RegisterPhilosopher(philosopher);
            _logger.LogInformation("Философ {PhilosopherName} ({PhilosopherId}) зарегистрирован", 
                request.PhilosopherName, request.PhilosopherId);
            return Ok(new { success = true });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при регистрации философа");
            return StatusCode(500, new { error = ex.Message });
        }
    }

    [HttpPost("{philosopherId}/exit")]
    public IActionResult PhilosopherExit(string philosopherId)
    {
        try
        {
            _tableStateService.PhilosopherExit(philosopherId);
            _logger.LogInformation("Философ {PhilosopherId} вышел из симуляции", philosopherId);
            
            // Проверяем, все ли философы вышли
            if (_tableStateService.AllPhilosophersExited())
            {
                _logger.LogInformation("Все философы вышли. Печатаем итоговые метрики...");
                PrintFinalMetrics();
            }
            
            return Ok(new { success = true });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при выходе философа");
            return StatusCode(500, new { error = ex.Message });
        }
    }

    private void PrintFinalMetrics()
    {
        var metricsCalculator = HttpContext.RequestServices.GetRequiredService<IMetricsCalculator>();
        var forkManager = HttpContext.RequestServices.GetRequiredService<IForkManager>();
        var philosophers = _tableStateService.GetRegisteredPhilosophers();
        var forks = forkManager.GetAllForks();
        
        var totalMeals = philosophers.Sum(p => p.MealsEaten);
        var throughput = metricsCalculator.TotalSimulationTimeMs > 0 
            ? (double)totalMeals / metricsCalculator.TotalSimulationTimeMs 
            : 0;
        
        Console.WriteLine("===== ИТОГОВЫЕ МЕТРИКИ =====");
        Console.WriteLine($"1. Пропускная способность: {throughput:F6} блюд/мс");
        Console.WriteLine();
        
        Console.WriteLine("2. Среднее время ожидания по философам:");
        foreach (var philosopher in philosophers)
        {
            var avgWaiting = philosopher.MealsEaten > 0 
                ? (double)philosopher.TotalWaitingTimeMs / philosopher.MealsEaten 
                : 0;
            Console.WriteLine($"  {philosopher.Name}: {avgWaiting:F2} мс");
        }
        Console.WriteLine();
        
        Console.WriteLine("3. Коэффициент утилизации вилок:");
        var forkUtilization = metricsCalculator.GetForkUtilization();
        foreach (var kvp in forkUtilization)
        {
            Console.WriteLine($"  Fork-{kvp.Key}: {kvp.Value:F2}%");
        }
        Console.WriteLine();
        
        Console.WriteLine($"4. Общая статистика:");
        Console.WriteLine($"  Всего съедено блюд: {totalMeals}");
        Console.WriteLine($"  Время симуляции: {metricsCalculator.TotalSimulationTimeMs} мс");
    }
}

