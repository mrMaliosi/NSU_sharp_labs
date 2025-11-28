using System;
using System.Threading;
using System.Threading.Tasks;
using Lab1.DiningPhilosophers;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace DiningPhilosophers.App.Services
{
    public sealed class SimulationLifecycleService : IHostedService
    {
        private readonly IHostApplicationLifetime _lifetime;
        private readonly SimulationOptions _options;
        private readonly IMetricsCalculator _metricsCalculator;
        private readonly IDisplayService _displayService;
        private readonly Philosopher[] _philosophers;
        private readonly ILogger<SimulationLifecycleService> _logger;
        private Timer? _timer;

        public SimulationLifecycleService(
            IHostApplicationLifetime lifetime,
            IOptions<SimulationOptions> options,
            IMetricsCalculator metricsCalculator,
            IDisplayService displayService,
            Philosopher[] philosophers,
            ILogger<SimulationLifecycleService> logger)
        {
            _lifetime = lifetime;
            _options = options.Value;
            _metricsCalculator = metricsCalculator;
            _displayService = displayService;
            _philosophers = philosophers;
            _logger = logger;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Сервис управления жизненным циклом запущен");
            _logger.LogInformation("Симуляция будет работать {Duration} мс", _options.SimulationDurationMs);

            _timer = new Timer(OnTimerElapsed, null, _options.SimulationDurationMs, Timeout.Infinite);
            
            return Task.CompletedTask;
        }

        private void OnTimerElapsed(object? state)
        {
            _logger.LogInformation("Время симуляции истекло, завершаем работу...");
            
            _displayService.DisplayMetrics(_philosophers, _metricsCalculator);
            
            _lifetime.StopApplication();
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _timer?.Dispose();
            _logger.LogInformation("Сервис управления жизненным циклом остановлен");
            return Task.CompletedTask;
        }
    }
}

