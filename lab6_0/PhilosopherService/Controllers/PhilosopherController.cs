using Microsoft.AspNetCore.Mvc;
using PhilosopherService.Services;

namespace PhilosopherService.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PhilosopherController : ControllerBase
{
    private readonly PhilosopherStateService _stateService;
    private readonly PhilosopherConfig _config;
    private readonly ILogger<PhilosopherController> _logger;

    public PhilosopherController(
        PhilosopherStateService stateService,
        PhilosopherConfig config,
        ILogger<PhilosopherController> logger)
    {
        _stateService = stateService;
        _config = config;
        _logger = logger;
    }

    [HttpGet("state")]
    public IActionResult GetState()
    {
        return Ok(new
        {
            Name = _config.Name,
            Id = _config.Id,
            State = _stateService.State.ToString(),
            LastAction = _stateService.LastAction.ToString(),
            MealsEaten = _stateService.MealsEaten,
            TotalWaitingTimeMs = _stateService.TotalWaitingTimeMs,
            LeftForkAcquired = _stateService.LeftForkAcquired,
            RightForkAcquired = _stateService.RightForkAcquired
        });
    }
}

