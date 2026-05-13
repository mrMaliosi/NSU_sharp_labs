using System;
using System.Threading.Tasks;
using Lab1.DiningPhilosophers;

namespace Lab1.DiningPhilosophers
{
    public static class Program
	{
        public static async Task Main()
        {
            var configPath = @"conf/config.json";
            SimulationContext simContext = SimulationContext.makeFromJson(configPath);

            IPhilosopherStrategy strategy = simContext.StrategyName switch
            {
                _ => new NaiveStrategy()
            };

            Console.WriteLine("Запуск симуляции...");
            await simContext.RunSimulation(strategy.PerformAction);
            Console.WriteLine("Симуляция завершена.");
        }

    }
}
