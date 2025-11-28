using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualBasic;

namespace Lab1.DiningPhilosophers
{
    public sealed class SimulationContext
    {
        private int _simulationDurationMs;
        private int _displayIntervalMs;
        private int _totalPhilosophers;
        private int _totalForks;
        private static int ForksPerPhilosopher { get; } = 2;

        private Stopwatch _simulationTimer = new Stopwatch();
        private readonly CancellationTokenSource _cancellationTokenSource = new();

		private Philosopher[] _philosophers;
		private Fork[] _forks;
		private Metrics _metrics;
        public string? StrategyName;

		public SimulationContext(int simulationDurationMs, int displayIntervalMs, int totalPhilosophers, int totalForks)
		{
            _simulationDurationMs = simulationDurationMs;
            _displayIntervalMs = displayIntervalMs;
            _totalPhilosophers = totalPhilosophers;
            _totalForks = totalForks;
            _philosophers = new Philosopher[totalPhilosophers];
            _forks = new Fork[totalForks];
            _metrics = new Metrics();
		}

        public static SimulationContext makeFromJson(string filePath)
        {
            string json = File.ReadAllText(filePath);
            var config = JsonSerializer.Deserialize<SimulationConfig>(json) ?? throw new InvalidOperationException("Не удалось десериализовать конфигурацию");

            var context = new SimulationContext(config.SimulationDurationMs, config.DisplayIntervalMs, config.TotalPhilosophers, config.TotalForks);
            context.StrategyName = config.Strategy?.ToLowerInvariant();

            context.CreateForks();
            context.CreatePhilosophers(config);

			return context;
        }

		public async Task RunSimulation(Action<Philosopher, SimulationContext> performAction) {
			var cancellationToken = _cancellationTokenSource.Token;
			
			_simulationTimer.Start();
			_metrics.StartSimulation(_forks);
			
			List<Thread> _philosopherThreads = new();
			for (int i = 0; i < _philosophers.Length; i++)
			{
				var thread = new Thread(() =>
                {
                    PhilosopherWorker(_philosophers[i], performAction, _cancellationTokenSource.Token);
                });
                thread.IsBackground = false;
                thread.Start();
                _philosopherThreads.Add(thread);
			}

			var displayTask = Task.Run(() => DisplayWorker(cancellationToken), cancellationToken);
			var timerTask = Task.Delay(_simulationDurationMs, cancellationToken);

			try
			{
				await timerTask;
			}
			catch (OperationCanceledException)
			{
				Console.WriteLine("Симуляция была отменена.");
			}

			// Отменяем все задачи
			_cancellationTokenSource.Cancel();

			// Ждём, пока каждый поток завершится
            foreach (var thread in _philosopherThreads)
            {
                thread.Join(); // здесь основной поток ждёт каждого философа
            }
			await displayTask;

			_simulationTimer.Stop();
			Console.WriteLine($"Симуляция завершена за {_simulationTimer.ElapsedMilliseconds} мс");
			CoutMetrics();
        }

		private void PhilosopherWorker(Philosopher philosopher, Action<Philosopher, SimulationContext> performAction, CancellationToken cancellationToken)
		{
			while (!cancellationToken.IsCancellationRequested)
			{
				try
				{
					performAction(philosopher, this);
					_metrics.OnStep(_forks, _philosophers);
				}
				catch (OperationCanceledException)
				{
					break;
				}
			}
		}

		private async Task DisplayWorker(CancellationToken cancellationToken)
		{
			while (!cancellationToken.IsCancellationRequested)
			{
				try
				{
					await Task.Delay(_displayIntervalMs, cancellationToken);
					CoutStats();
				}
				catch (OperationCanceledException)
				{
					break;
				}
			}
		}

        private void CreateForks(){
            for (int i = 0; i < _totalForks; i++)
            {
                _forks[i] = new Fork(i);
            }
        }

        private void CreatePhilosophers(SimulationConfig config) {
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

                Fork leftFork = _forks[i];
                Fork rightFork = _forks[(i + 1) % config.TotalForks];

                _philosophers[i] = new Philosopher(name, leftFork, rightFork, pCtx);
                _philosophers[i].OnMealEaten += (philosopherName) => _metrics.OnMeal(philosopherName);
            }
        }

        private void CoutMetrics() 
        {
            Console.WriteLine($"===== МЕТРИКИ =====");
            
            double throughput = _metrics.GetThroughput();
            Console.WriteLine($"1. Пропускная способность: {throughput:F6} блюд/мс");
            Console.WriteLine();
            
            Console.WriteLine("2. Среднее время ожидания по философам:");
            var avgWaitingTimes = _metrics.GetAverageWaitingTime();
            foreach (var kvp in avgWaitingTimes)
            {
                Console.WriteLine($"  {kvp.Key}: {kvp.Value:F2} мс");
            }
            Console.WriteLine();
            
            Console.WriteLine("3. Коэффициент утилизации вилок:");
            var forkUtilization = _metrics.GetForkUtilization();
            foreach (var kvp in forkUtilization)
            {
                Console.WriteLine($"  Fork-{kvp.Key}: {kvp.Value:F2}%");
            }
            Console.WriteLine();
            
            int totalMeals = _philosophers.Sum(p => p.MealsEaten);
            Console.WriteLine($"4. Общая статистика:");
            Console.WriteLine($"  Всего съедено блюд: {totalMeals}");
            Console.WriteLine($"  Время симуляции: {_simulationTimer.ElapsedMilliseconds} мс");
            checkForDeadlock();
        }

        private void checkForDeadlock()
        {
            // Получаем состояние всех вилок
            var forksInUse = _forks.Where(f => f.State == ForkState.InUse).ToList();

            // Если не все вилки заняты — дедлока нет
            if (forksInUse.Count < _forks.Length)
                return ;

            // Получаем философов, которые держат вилки
            var holders = forksInUse
                .Select(f => f.HeldBy)
                .Where(p => p != null)
                .Distinct()
                .ToList();

            // Если все вилки заняты, и каждая вилка у своего философа — дедлок
            if (holders.Count == forksInUse.Count)
            {
                Console.WriteLine($"⚠️  DEADLOCK DETECTED");
            }
        }

        private void CoutStats() 
        {
            Console.WriteLine($"===== ВРЕМЯ: {_simulationTimer.ElapsedMilliseconds} мс =====");
			Console.WriteLine("Философы:");
			foreach (Philosopher philosopher in _philosophers) 
            {
                Console.WriteLine($"  {philosopher.Name}: {philosopher.State} (Action = {philosopher.LastAction}), съедено: {philosopher.MealsEaten}");
            }
            Console.WriteLine();
            Console.WriteLine("Вилки:");
            foreach (Fork fork in _forks) 
            {
                var (state, heldBy) = fork.GetState();
                if (heldBy != null) 
                {
                    Console.WriteLine($"  Fork-{fork.Id}: {state} (используется {heldBy.Name})");
                } 
                else 
                {
                    Console.WriteLine($"  Fork-{fork.Id}: {state}");
                }
            }
            Console.WriteLine();
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
