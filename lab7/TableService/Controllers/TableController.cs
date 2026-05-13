using Microsoft.AspNetCore.Mvc;
using TableService.Models;
using TableService.Services;

namespace TableService.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TableController : ControllerBase
{
    private readonly TableManager _tableService;
    private readonly ILogger<TableController> _logger;

    public TableController(TableManager tableService, ILogger<TableController> logger)
    {
        _tableService = tableService;
        _logger = logger;
    }

    [HttpPost("take-fork")]
    public IActionResult TakeFork([FromBody] ForkRequest request)
    {
        _logger.LogInformation($"Philosopher {request.PhilosopherId} attempting to take fork {request.ForkId}");
        var response = _tableService.TryTakeFork(request.ForkId, request.PhilosopherId);
        
        if (response.Success)
        {
            return Ok(response);
        }
        
        return Conflict(response);
    }

    [HttpPost("release-fork")]
    public IActionResult ReleaseFork([FromBody] ForkRequest request)
    {
        _logger.LogInformation($"Philosopher {request.PhilosopherId} releasing fork {request.ForkId}");
        var response = _tableService.ReleaseFork(request.ForkId, request.PhilosopherId);
        
        if (response.Success)
        {
            return Ok(response);
        }
        
        return BadRequest(response);
    }

    [HttpPost("register")]
    public IActionResult RegisterPhilosopher([FromBody] RegisterRequest request)
    {
        _logger.LogInformation($"Registering philosopher {request.PhilosopherName} ({request.PhilosopherId})");
        _tableService.RegisterPhilosopher(request.PhilosopherId, request.PhilosopherName);
        return Ok(new { message = "Philosopher registered successfully" });
    }

    [HttpPost("update-stats")]
    public IActionResult UpdateStats([FromBody] StatsUpdateRequest request)
    {
        _tableService.UpdateStats(
            request.PhilosopherId,
            request.MealsEaten,
            request.TotalThinkingTime,
            request.TotalEatingTime,
            request.TotalHungryTime
        );
        return Ok(new { message = "Stats updated successfully" });
    }

    [HttpPost("exit")]
    public IActionResult PhilosopherExit([FromBody] ExitRequest request)
    {
        _logger.LogInformation($"Philosopher {request.PhilosopherId} is exiting");
        _tableService.PhilosopherExited(request.PhilosopherId);
        return Ok(new { message = "Philosopher exited successfully" });
    }
}

public class RegisterRequest
{
    public string PhilosopherId { get; set; } = string.Empty;
    public string PhilosopherName { get; set; } = string.Empty;
}

public class StatsUpdateRequest
{
    public string PhilosopherId { get; set; } = string.Empty;
    public int MealsEaten { get; set; }
    public int TotalThinkingTime { get; set; }
    public int TotalEatingTime { get; set; }
    public int TotalHungryTime { get; set; }
}

public class ExitRequest
{
    public string PhilosopherId { get; set; } = string.Empty;
}

