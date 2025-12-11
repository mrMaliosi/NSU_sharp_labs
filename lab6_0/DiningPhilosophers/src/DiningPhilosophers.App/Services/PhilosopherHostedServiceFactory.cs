using System;
using Lab1.DiningPhilosophers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace DiningPhilosophers.App.Services
{
    public static class PhilosopherHostedServiceFactory
    {
        public static IHostedService Create(IServiceProvider services, Philosopher philosopher)
        {
            var logger = services.GetRequiredService<ILogger<PhilosopherHostedService>>();
            var strategy = services.GetRequiredService<IPhilosopherStrategy>();
            var metrics = services.GetRequiredService<IMetricsCalculator>();
            var forkMgr = services.GetRequiredService<IForkManager>();
            var allPhilosophers = services.GetRequiredService<Philosopher[]>();
            
            return new PhilosopherHostedService(
                philosopher, strategy, metrics, forkMgr, allPhilosophers, logger);
        }
    }
}

