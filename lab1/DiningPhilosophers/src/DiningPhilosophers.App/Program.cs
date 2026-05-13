using System;
using Lab1.DiningPhilosophers;

namespace Lab1.DiningPhilosophers
{
    public static class Program
	{
        public static void Main()
        {
            var configPath = @"conf/config.json";
            SimulationContext simContext = SimulationContext.makeFromJson(configPath);

            IPhilosopherStrategy strategy = simContext.StrategyName switch
            {
                "cord" => new CoordinatedStrategy(new SimpleCoordinator()),
                _ => new NaiveStrategy()
            };

            simContext.runSimulation(strategy.PerformAction);
            Console.WriteLine("Симуляция завершена.");
        }

    }
}
