using System;
using System.Diagnostics;
using System.Linq;

namespace Lab1.DiningPhilosophers
{
    /// <summary>
    /// Класс для мониторинга и отображения статистики симуляции философов
    /// </summary>
    public sealed class StatsMonitor
    {
        private readonly Metrics _metrics;
        private readonly Philosopher[] _philosophers;
        private readonly Fork[] _forks;
        private readonly Stopwatch _simulationTimer;

        public StatsMonitor(Metrics metrics, Philosopher[] philosophers, Fork[] forks, Stopwatch simulationTimer)
        {
            _metrics = metrics;
            _philosophers = philosophers;
            _forks = forks;
            _simulationTimer = simulationTimer;
        }

        /// <summary>
        /// Отображает текущую статистику симуляции
        /// </summary>
        public void DisplayCurrentStats()
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

        /// <summary>
        /// Отображает итоговые метрики симуляции
        /// </summary>
        public void DisplayFinalMetrics()
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
            CheckForDeadlock();
        }

        /// <summary>
        /// Проверяет наличие дедлока в системе
        /// </summary>
        private void CheckForDeadlock()
        {
            // Получаем состояние всех вилок
            var forksInUse = _forks.Where(f => f.State == ForkState.InUse).ToList();

            // Если не все вилки заняты — дедлока нет
            if (forksInUse.Count < _forks.Length)
                return;

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
    }
}
