using Microsoft.AspNetCore.Mvc;

namespace PhilosopherService.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PhilosopherController : ControllerBase
{
    private readonly ILogger<PhilosopherController> _logger;

    public PhilosopherController(ILogger<PhilosopherController> logger)
    {
        _logger = logger;
    }

    [HttpGet("status")]
    public IActionResult GetStatus()
    {
        return Ok(new { status = "running" });
    }
}


