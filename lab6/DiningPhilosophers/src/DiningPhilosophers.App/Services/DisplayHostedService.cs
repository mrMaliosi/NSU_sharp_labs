using System;
using System.Threading;
using System.Threading.Tasks;
using Lab1.DiningPhilosophers;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace DiningPhilosophers.App.Services
{
    public sealed class DisplayHostedService : BackgroundService
    {
        private readonly IDisplayService _displayService;
        private readonly IMetricsCalculator _metricsCalculator;
        private readonly IForkManager _forkManager;
        private readonly Philosopher[] _philosophers;
        private readonly SimulationOptions _options;
        private readonly ILogger<DisplayHostedService> _logger;

        public DisplayHostedService(
            IDisplayService displayService,
            IMetricsCalculator metricsCalculator,
            IForkManager forkManager,
            Philosopher[] philosophers,
            IOptions<SimulationOptions> options,
            ILogger<DisplayHostedService> logger)
        {
            _displayService = displayService;
            _metricsCalculator = metricsCalculator;
            _forkManager = forkManager;
            _philosophers = philosophers;
            _options = options.Value;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Сервис отображения запущен");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await Task.Delay(_options.DisplayIntervalMs, stoppingToken);
                    
                    long elapsedMs = _metricsCalculator.TotalSimulationTimeMs;
                    _displayService.DisplayStats(_philosophers, _forkManager.GetAllForks(), elapsedMs);
                }
                catch (OperationCanceledException)
                {
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Ошибка в сервисе отображения");
                }
            }

            _logger.LogInformation("Сервис отображения завершил работу");
        }
    }
}

