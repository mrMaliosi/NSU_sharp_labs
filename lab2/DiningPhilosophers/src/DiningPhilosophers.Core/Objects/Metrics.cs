using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Lab1.DiningPhilosophers
{
	public sealed class Metrics
	{
		private readonly object _lock = new object();
		
		public long TotalSimulationTimeMs { get; private set; }
		public readonly Dictionary<string, int> MealsByPhilosopher = new();
		public readonly Dictionary<string, long> WaitingTimeByPhilosopher = new();
		public readonly Dictionary<int, long> ForkUsageTimeMs = new();
		public readonly Dictionary<int, DateTime> ForkLastUsedTime = new();
		private DateTime _simulationStartTime;
		private Fork[]? _forks;

		public void StartSimulation(Fork[] forks)
		{
			_simulationStartTime = DateTime.Now;
			_forks = forks;
		}

		public void OnStep(IEnumerable<Fork> forks, IEnumerable<Philosopher> philosophers)
		{
			lock (_lock)
			{
				TotalSimulationTimeMs = (long)(DateTime.Now - _simulationStartTime).TotalMilliseconds;
                UpdateForksStatistic(forks);
                UpdatePhilosophersStatistic(philosophers);
            }
		}

		private void UpdateForksStatistic(IEnumerable<Fork> forks) 
		{
			foreach (var fork in forks)
			{
				if (!ForkLastUsedTime.ContainsKey(fork.Id)) 
					ForkLastUsedTime[fork.Id] = _simulationStartTime;
				else
					ForkLastUsedTime[fork.Id] = DateTime.Now;
			}
		}

        private void UpdatePhilosophersStatistic(IEnumerable<Philosopher> philosophers)
        {
			foreach (var p in philosophers)
			{
				if (!MealsByPhilosopher.ContainsKey(p.Name)) MealsByPhilosopher[p.Name] = 0;
				if (!WaitingTimeByPhilosopher.ContainsKey(p.Name)) WaitingTimeByPhilosopher[p.Name] = 0;
				
				WaitingTimeByPhilosopher[p.Name] = p.GetTotalWaitingTimeMs();
			}
        }

        public void OnMeal(string name)
		{
			lock (_lock)
			{
				if (!MealsByPhilosopher.ContainsKey(name)) MealsByPhilosopher[name] = 0;
				MealsByPhilosopher[name]++;
			}
		}

		// Пропускная способность (блюд/миллисекунду)
		public double GetThroughput()
		{
			lock (_lock)
			{
				if (TotalSimulationTimeMs == 0) return 0;
				int totalMeals = MealsByPhilosopher.Values.Sum();
				return (double)totalMeals / TotalSimulationTimeMs;
			}
		}

		// Среднее время ожидания для каждого философа (в миллисекундах)
		public Dictionary<string, double> GetAverageWaitingTime()
		{
			lock (_lock)
			{
				var result = new Dictionary<string, double>();
				foreach (var kvp in WaitingTimeByPhilosopher)
				{
					int meals = MealsByPhilosopher.ContainsKey(kvp.Key) ? MealsByPhilosopher[kvp.Key] : 0;
					result[kvp.Key] = meals > 0 ? (double)kvp.Value / meals : 0;
				}
				return result;
			}
		}

		// Коэффициент утилизации для каждой вилки в % по времени
		public Dictionary<int, double> GetForkUtilization()
		{
			lock (_lock)
			{
				var result = new Dictionary<int, double>();
				
				if (_forks != null)
				{
					foreach (var fork in _forks)
					{
						result[fork.Id] = fork.GetUtilizationPercentage(TotalSimulationTimeMs);
					}
				}
				
				return result;
			}
		}
	}
}


