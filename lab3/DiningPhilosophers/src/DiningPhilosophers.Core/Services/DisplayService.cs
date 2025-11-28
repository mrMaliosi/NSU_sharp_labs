using System.Linq;

namespace Lab1.DiningPhilosophers
{
    public sealed class DisplayService : IDisplayService
    {
        public void DisplayStats(Philosopher[] philosophers, Fork[] forks, long elapsedMs)
        {
            Console.WriteLine($"===== ВРЕМЯ: {elapsedMs} мс =====");
            Console.WriteLine("Философы:");
            foreach (Philosopher philosopher in philosophers) 
            {
                Console.WriteLine($"  {philosopher.Name}: {philosopher.State} (Action = {philosopher.LastAction}), съедено: {philosopher.MealsEaten}");
            }
            Console.WriteLine();
            Console.WriteLine("Вилки:");
            foreach (Fork fork in forks) 
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

        public void DisplayMetrics(Philosopher[] philosophers, IMetricsCalculator metrics)
        {
            Console.WriteLine($"===== МЕТРИКИ =====");
            
            double throughput = metrics.GetThroughput();
            Console.WriteLine($"1. Пропускная способность: {throughput:F6} блюд/мс");
            Console.WriteLine();
            
            Console.WriteLine("2. Среднее время ожидания по философам:");
            var avgWaitingTimes = metrics.GetAverageWaitingTime();
            foreach (var kvp in avgWaitingTimes)
            {
                Console.WriteLine($"  {kvp.Key}: {kvp.Value:F2} мс");
            }
            Console.WriteLine();
            
            Console.WriteLine("3. Коэффициент утилизации вилок:");
            var forkUtilization = metrics.GetForkUtilization();
            foreach (var kvp in forkUtilization)
            {
                Console.WriteLine($"  Fork-{kvp.Key}: {kvp.Value:F2}%");
            }
            Console.WriteLine();
            
            int totalMeals = philosophers.Sum(p => p.MealsEaten);
            Console.WriteLine($"4. Общая статистика:");
            Console.WriteLine($"  Всего съедено блюд: {totalMeals}");
            Console.WriteLine($"  Время симуляции: {metrics.TotalSimulationTimeMs} мс");
            
            CheckForDeadlock(philosophers);
        }

        private void CheckForDeadlock(Philosopher[] philosophers)
        {
            // Проверка на дедлок - если все философы голодны и держат по одной вилке
            var hungryPhilosophers = philosophers.Where(p => p.State == PhilosopherState.Hungry).ToList();
            if (hungryPhilosophers.Count == philosophers.Length)
            {
                Console.WriteLine($"⚠️  DEADLOCK DETECTED");
            }
        }
    }
}

