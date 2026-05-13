using PhilosopherService.Services;

namespace PhilosopherService.Services;

public class PhilosopherBackgroundService : BackgroundService
{
    private readonly PhilosopherWorker _philosopherService;
    private readonly ILogger<PhilosopherBackgroundService> _logger;

    public PhilosopherBackgroundService(
        PhilosopherWorker philosopherService,
        ILogger<PhilosopherBackgroundService> logger)
    {
        _philosopherService = philosopherService;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // Wait a bit for the service to be fully ready
        await Task.Delay(2000, stoppingToken);
        
        _logger.LogInformation("Starting philosopher simulation");
        await _philosopherService.StartSimulationAsync(stoppingToken);
    }
}


