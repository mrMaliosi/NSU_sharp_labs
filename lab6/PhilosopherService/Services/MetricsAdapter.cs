using Lab1.DiningPhilosophers;
using System.Collections.Generic;

namespace TableService.Services;

public class MetricsAdapter
{
    private readonly IMetricsCalculator _metricsCalculator;
    private readonly Dictionary<string, int> _mealsByPhilosopher = new();
    private readonly Dictionary<string, long> _waitingTimeByPhilosopher = new();

    public MetricsAdapter(IMetricsCalculator metricsCalculator)
    {
        _metricsCalculator = metricsCalculator;
    }

    public void RecordMeal(string philosopherName)
    {
        if (!_mealsByPhilosopher.ContainsKey(philosopherName))
            _mealsByPhilosopher[philosopherName] = 0;
        _mealsByPhilosopher[philosopherName]++;
        _metricsCalculator.OnMeal(philosopherName);
    }

    public void UpdateWaitingTime(string philosopherName, long waitingTimeMs)
    {
        _waitingTimeByPhilosopher[philosopherName] = waitingTimeMs;
    }

    public Dictionary<string, int> GetMealsByPhilosopher() => _mealsByPhilosopher;
    public Dictionary<string, long> GetWaitingTimeByPhilosopher() => _waitingTimeByPhilosopher;
}

