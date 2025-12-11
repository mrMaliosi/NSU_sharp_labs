using System;
using System.Threading;
using System.Threading.Tasks;
using Lab1.DiningPhilosophers;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace DiningPhilosophers.App.Services
{
    public sealed class PhilosopherHostedService : BackgroundService
    {
        private readonly Philosopher _philosopher;
        private readonly IPhilosopherStrategy _strategy;
        private readonly IMetricsCalculator _metricsCalculator;
        private readonly Philosopher[] _allPhilosophers;
        private readonly Fork[] _allForks;
        private readonly ILogger<PhilosopherHostedService> _logger;

        public PhilosopherHostedService(
            Philosopher philosopher,
            IPhilosopherStrategy strategy,
            IMetricsCalculator metricsCalculator,
            IForkManager forkManager,
            Philosopher[] allPhilosophers,
            ILogger<PhilosopherHostedService> logger)
        {
            _philosopher = philosopher;
            _strategy = strategy;
            _metricsCalculator = metricsCalculator;
            _allPhilosophers = allPhilosophers;
            _allForks = forkManager.GetAllForks();
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Философ {Name} начал работу", _philosopher.Name);

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    _strategy.PerformAction(_philosopher, _allPhilosophers, _allForks);
                    _metricsCalculator.OnStep(_allForks, _allPhilosophers);
                    
                    await Task.Delay(1, stoppingToken);
                }
                catch (OperationCanceledException)
                {
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Ошибка в работе философа {Name}", _philosopher.Name);
                }
            }

            _logger.LogInformation("Философ {Name} завершил работу", _philosopher.Name);
        }
    }
}

