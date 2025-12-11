using Microsoft.AspNetCore.Mvc;
using Lab1.DiningPhilosophers;
using TableService.Services;
using TableService.Models;

namespace TableService.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ForksController : ControllerBase
{
    private readonly IForkManager _forkManager;
    private readonly TableStateService _tableStateService;
    private readonly ILogger<ForksController> _logger;

    public ForksController(
        IForkManager forkManager,
        TableStateService tableStateService,
        ILogger<ForksController> logger)
    {
        _forkManager = forkManager;
        _tableStateService = tableStateService;
        _logger = logger;
    }

    [HttpPost("{forkId}/acquire")]
    public IActionResult AcquireFork(int forkId, [FromBody] AcquireForkRequest request)
    {
        try
        {
            var fork = _forkManager.GetFork(forkId);
            // Создаем временный объект Philosopher для работы с Fork
            var tempFork = new Lab1.DiningPhilosophers.Fork(0);
            var philosopher = new Lab1.DiningPhilosophers.Philosopher(
                request.PhilosopherName,
                tempFork,
                tempFork,
                new Lab1.DiningPhilosophers.PhilosopherContext(
                    new Lab1.DiningPhilosophers.Segment(30, 100),
                    new Lab1.DiningPhilosophers.Segment(40, 50),
                    20));
            
            if (fork.TryAcquire(philosopher))
            {
                _logger.LogInformation("Вилка {ForkId} захвачена философом {PhilosopherName}", forkId, request.PhilosopherName);
                return Ok(new AcquireForkResponse { Success = true });
            }
            
            return Ok(new AcquireForkResponse { Success = false });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при захвате вилки {ForkId}", forkId);
            return StatusCode(500, new { error = ex.Message });
        }
    }

    [HttpPost("{forkId}/release")]
    public IActionResult ReleaseFork(int forkId)
    {
        try
        {
            var fork = _forkManager.GetFork(forkId);
            fork.Release();
            _logger.LogInformation("Вилка {ForkId} освобождена", forkId);
            return Ok(new { success = true });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при освобождении вилки {ForkId}", forkId);
            return StatusCode(500, new { error = ex.Message });
        }
    }

    [HttpGet("{forkId}/state")]
    public IActionResult GetForkState(int forkId)
    {
        try
        {
            var fork = _forkManager.GetFork(forkId);
            var (state, heldBy) = fork.GetState();
            
            return Ok(new ForkStateResponse
            {
                ForkId = forkId,
                State = state.ToString(),
                HeldBy = heldBy?.Name
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при получении состояния вилки {ForkId}", forkId);
            return StatusCode(500, new { error = ex.Message });
        }
    }

    [HttpGet]
    public IActionResult GetAllForks()
    {
        try
        {
            var forks = _forkManager.GetAllForks();
            var states = forks.Select(f =>
            {
                var (state, heldBy) = f.GetState();
                return new ForkStateResponse
                {
                    ForkId = f.Id,
                    State = state.ToString(),
                    HeldBy = heldBy?.Name
                };
            }).ToArray();
            
            return Ok(states);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при получении всех вилок");
            return StatusCode(500, new { error = ex.Message });
        }
    }
}

