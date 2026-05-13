using Microsoft.AspNetCore.Mvc;

namespace CoordinatorService.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CoordinatorController : ControllerBase
{
    private readonly ILogger<CoordinatorController> _logger;

    public CoordinatorController(ILogger<CoordinatorController> logger)
    {
        _logger = logger;
    }

    [HttpGet("status")]
    public IActionResult GetStatus()
    {
        return Ok(new { status = "running", service = "CoordinatorService" });
    }
}

