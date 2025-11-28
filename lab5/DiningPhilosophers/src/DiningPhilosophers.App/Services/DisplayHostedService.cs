using System;
using System.Threading;
using System.Threading.Tasks;
using Lab1.DiningPhilosophers;
using DiningPhilosophers.Core.Services;
using Microsoft.Extensions.DependencyInjection;
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
        private readonly IServiceProvider _serviceProvider;

        public DisplayHostedService(
            IDisplayService displayService,
            IMetricsCalculator metricsCalculator,
            IForkManager forkManager,
            Philosopher[] philosophers,
            IOptions<SimulationOptions> options,
            ILogger<DisplayHostedService> logger,
            IServiceProvider serviceProvider)
        {
            _displayService = displayService;
            _metricsCalculator = metricsCalculator;
            _forkManager = forkManager;
            _philosophers = philosophers;
            _options = options.Value;
            _logger = logger;
            _serviceProvider = serviceProvider;
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
                    double elapsedSeconds = elapsedMs / 1000.0;
                    _displayService.DisplayStats(_philosophers, _forkManager.GetAllForks(), elapsedMs);
                    
                    // Сохранение состояния в базу данных
                    using (var scope = _serviceProvider.CreateScope())
                    {
                        var stateService = scope.ServiceProvider.GetRequiredService<ISimulationStateService>();
                        await stateService.SaveStateAsync(_philosophers, _forkManager.GetAllForks(), elapsedSeconds);
                    }
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

