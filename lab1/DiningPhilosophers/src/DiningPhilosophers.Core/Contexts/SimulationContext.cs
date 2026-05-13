using System;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text.Json;

namespace Lab1.DiningPhilosophers
{
    public sealed class SimulationContext
    {
        private int _simulationSteps;
        private int _totalPhilosophers;
        private int _totalForks;
        private static int ForksPerPhilosopher { get; } = 2;

        private int step;

		private Philosopher[] _philosophers;
		private Fork[] _forks;
		private Metrics _metrics;
        public string? StrategyName;

		public SimulationContext(int simSteps, int totalPhilosophers, int totalForks)
		{
            _simulationSteps = simSteps;
            _totalPhilosophers = totalPhilosophers;
            _totalForks = totalForks;
            step = 0;
            _philosophers = new Philosopher[totalPhilosophers];
            _forks = new Fork[totalForks];
            _metrics = new Metrics();
		}

		public static SimulationContext makeFromJson(string filePath)
        {
            string json = File.ReadAllText(filePath);
            var config = JsonSerializer.Deserialize<SimulationConfig>(json) ?? throw new InvalidOperationException("Не удалось десериализовать конфигурацию");

            var context = new SimulationContext(config.SimulationSteps, config.TotalPhilosophers, config.TotalForks);
            context.StrategyName = config.Strategy?.ToLowerInvariant();

            // создаём вилки
            for (int i = 0; i < config.TotalForks; i++)
            {
                context._forks[i] = new Fork(i);
            }

            PhilosopherContext ?defaultCtx = null;
            if (config.DefaultContext != null)
            {
                defaultCtx = new PhilosopherContext(config.DefaultContext);
            }

            for (int i = 0; i < config.TotalPhilosophers; i++)
            {
                string name = config.PhilosopherNames?[i] 
                            ?? config.Philosophers?[i].Name 
                            ?? $"Philosopher{i+1}";

                PhilosopherContext pCtx;
                if (defaultCtx != null) {
                    pCtx = defaultCtx;
                } else if (config.Philosophers != null && i < config.Philosophers.Length) {
                    pCtx = new PhilosopherContext(config.Philosophers[i].Context);
                } else {
                    throw new InvalidOperationException(
                        "Ни Philosopher[].Context, ни DefaultContext не заданы"
                    );
                }

                // TODO: refactor
                Fork[] forks = new Fork[ForksPerPhilosopher];
                forks[0] = context._forks[i];
                forks[1] = context._forks[(i + 1) % config.TotalForks];

                context._philosophers[i] = new Philosopher(name, forks, pCtx);
            }

			return context;
        }

		public void runSimulation(Action<Philosopher, SimulationContext> performAction) {
			while (step < _simulationSteps) {
				foreach (var philosopher in _philosophers)
				{
					performAction(philosopher, this);
				}
				_metrics.OnStep(_forks, _philosophers);
				++step;

                CoutStats();
                //WriteStatsToFile();
            }

            CoutMetrics();
        }

        private void CoutMetrics() 
        {
            Console.WriteLine($"===== МЕТРИКИ =====");
			Console.WriteLine("1. Средняя поедательная способность:");
            double eatSpeed = 0.0;
            int mealsTotal = 0;
            foreach (Philosopher philosopher in _philosophers) 
            {
                mealsTotal += philosopher.MealsEaten;
                double philosopherEatSpeed = (double)philosopher.MealsEaten / _simulationSteps;
                Console.WriteLine($"  {philosopher.Name}: {philosopherEatSpeed} блюд/шаг.");
            }
            eatSpeed = (double) mealsTotal / _simulationSteps;
            Console.WriteLine($"Общее среднее: {eatSpeed:F2} блюд/шаг");
            Console.WriteLine();
            Console.WriteLine("2. Среднее время голодания по философам:");
            int maxHungry = -1;
            string maxHungryPhilosopherName = "";
            foreach (Philosopher philosopher in _philosophers) 
            {
                int hungryTime = _metrics.HungryTimeByPhilosopher[philosopher.Name];
                double avgHungryTime = (double)hungryTime / _simulationSteps * 100;
                Console.WriteLine($"  {philosopher.Name}: {avgHungryTime:F2} %.");
                if (maxHungry < _metrics.MaxHungryTimeByPhilosopher[philosopher.Name])
                {
                    maxHungry = _metrics.MaxHungryTimeByPhilosopher[philosopher.Name];
                    maxHungryPhilosopherName = philosopher.Name;
                }
            }
            Console.WriteLine($"Максимальное время голодания у {maxHungryPhilosopherName}: {maxHungry}");
            Console.WriteLine();

            Console.WriteLine("Вилки:");
            foreach (Fork fork in _forks) 
            {
                double freePercents = (double)_metrics.ForkFreeSteps[fork.Id] / _simulationSteps * 100;
                double usingPercents = (double)_metrics.ForkUsingSteps[fork.Id] / _simulationSteps * 100;
                double busyPercents = (double)_metrics.ForkBusySteps[fork.Id] / _simulationSteps * 100;
                Console.WriteLine($"  Fork-{fork.Id}. Свободна: {freePercents:F2}%. Используется: {usingPercents:F2}%. Заблокирована: {busyPercents:F2}%.");
            }
            Console.WriteLine();
            Console.WriteLine($"Score: {mealsTotal}");
        }

        private void CoutStats() 
        {
            Console.WriteLine($"===== ШАГ {step} =====");
			Console.WriteLine("Философы:");
			foreach (Philosopher philosopher in _philosophers) 
            {
                Console.WriteLine($"  {philosopher.Name}: {philosopher.State} (Action = {philosopher.LastAction}), съедено: {philosopher.MealsEaten}");
            }
            Console.WriteLine();
            Console.WriteLine("Вилки:");
            foreach (Fork fork in _forks) 
            {
                if (fork.HeldBy != null) 
                {
                    Console.WriteLine($"  Fork-{fork.Id}: {fork.State} (используется {fork.GetOwnerName()})");
                } 
                else 
                {
                    Console.WriteLine($"  Fork-{fork.Id}: {fork.State}");
                }
            }
        }

        // Временный костыль. Исправим в будущих лабах
        // private void WriteStatsToFile()
        // {
        //     string filePath = "C:/Users/Владимир/Desktop/Универ/4 курс/CS/1/log.txt";
        //     //File.WriteAllText(filePath, string.Empty);
        //     using (var writer = new StreamWriter(filePath, append: true)) // append = true, чтобы добавлять, а не перезаписывать
        //     {
        //         writer.WriteLine($"===== ШАГ {step} =====");
        //         writer.WriteLine("Философы:");
        //         foreach (Philosopher philosopher in _philosophers)
        //         {
        //             writer.WriteLine($"  {philosopher.Name}: {philosopher.State} (Action = {philosopher.LastAction}), съедено: {philosopher.MealsEaten}");
        //         }

        //         writer.WriteLine();
        //         writer.WriteLine("Вилки:");
        //         foreach (Fork fork in _forks)
        //         {
        //             if (fork.HeldBy != null)
        //             {
        //                 writer.WriteLine($"  Fork-{fork.Id}: {fork.State} (используется {fork.GetOwnerName()})");
        //             }
        //             else
        //             {
        //                 writer.WriteLine($"  Fork-{fork.Id}: {fork.State}");
        //             }
        //         }

        //         writer.WriteLine(); // пустая строка между шагами
        //     }
        // }
    }
}
