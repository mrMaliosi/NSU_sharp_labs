using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Lab1.DiningPhilosophers
{
    public sealed class MetricsCalculator : IMetricsCalculator
    {
        private readonly object _lock = new object();
        
        private readonly Dictionary<string, int> _mealsByPhilosopher = new();
        private readonly Dictionary<string, long> _waitingTimeByPhilosopher = new();
        private readonly Dictionary<int, long> _forkUsageTimeMs = new();
        private readonly Dictionary<int, DateTime> _forkLastUsedTime = new();
        
        private DateTime _simulationStartTime;
        private Fork[]? _forks;
        
        public long TotalSimulationTimeMs { get; private set; }

        public void StartSimulation(Fork[] forks)
        {
            _simulationStartTime = DateTime.Now;
            _forks = forks;
        }

        public void OnStep(Fork[] forks, Philosopher[] philosophers)
        {
            lock (_lock)
            {
                TotalSimulationTimeMs = (long)(DateTime.Now - _simulationStartTime).TotalMilliseconds;
                UpdateForksStatistic(forks);
                UpdatePhilosophersStatistic(philosophers);
            }
        }

        private void UpdateForksStatistic(Fork[] forks) 
        {
            foreach (var fork in forks)
            {
                if (!_forkUsageTimeMs.ContainsKey(fork.Id)) _forkUsageTimeMs[fork.Id] = 0;
                if (!_forkLastUsedTime.ContainsKey(fork.Id)) _forkLastUsedTime[fork.Id] = _simulationStartTime;
                
                var (state, heldBy) = fork.GetState();
                if (state == ForkState.InUse && heldBy != null)
                {
                    _forkUsageTimeMs[fork.Id] += (long)(DateTime.Now - _forkLastUsedTime[fork.Id]).TotalMilliseconds;
                    _forkLastUsedTime[fork.Id] = DateTime.Now;
                }
                else
                {
                    _forkLastUsedTime[fork.Id] = DateTime.Now;
                }
            }
        }

        private void UpdatePhilosophersStatistic(Philosopher[] philosophers)
        {
            foreach (var p in philosophers)
            {
                if (!_mealsByPhilosopher.ContainsKey(p.Name)) _mealsByPhilosopher[p.Name] = 0;
                if (!_waitingTimeByPhilosopher.ContainsKey(p.Name)) _waitingTimeByPhilosopher[p.Name] = 0;
                
                _waitingTimeByPhilosopher[p.Name] = p.GetTotalWaitingTimeMs();
            }
        }

        public void OnMeal(string name)
        {
            lock (_lock)
            {
                if (!_mealsByPhilosopher.ContainsKey(name)) _mealsByPhilosopher[name] = 0;
                _mealsByPhilosopher[name]++;
            }
        }

        public double GetThroughput()
        {
            lock (_lock)
            {
                if (TotalSimulationTimeMs == 0) return 0;
                int totalMeals = _mealsByPhilosopher.Values.Sum();
                return (double)totalMeals / TotalSimulationTimeMs;
            }
        }

        public Dictionary<string, double> GetAverageWaitingTime()
        {
            lock (_lock)
            {
                var result = new Dictionary<string, double>();
                foreach (var kvp in _waitingTimeByPhilosopher)
                {
                    int meals = _mealsByPhilosopher.ContainsKey(kvp.Key) ? _mealsByPhilosopher[kvp.Key] : 0;
                    result[kvp.Key] = meals > 0 ? (double)kvp.Value / meals : 0;
                }
                return result;
            }
        }

        public Dictionary<int, double> GetForkUtilization()
        {
            lock (_lock)
            {
                var result = new Dictionary<int, double>();
                foreach (var kvp in _forkUsageTimeMs)
                {
                    int forkId = kvp.Key;
                    long usageTime = kvp.Value;
                    
                    if (_forkLastUsedTime.ContainsKey(forkId) && _forks != null)
                    {
                        var (state, heldBy) = _forks.FirstOrDefault(f => f.Id == forkId)?.GetState() ?? (ForkState.Available, null);
                        if (state == ForkState.InUse && heldBy != null)
                        {
                            usageTime += (long)(DateTime.Now - _forkLastUsedTime[forkId]).TotalMilliseconds;
                        }
                    }
                    
                    result[forkId] = TotalSimulationTimeMs > 0 ? (double)usageTime / TotalSimulationTimeMs * 100 : 0;
                }
                return result;
            }
        }
    }
}

