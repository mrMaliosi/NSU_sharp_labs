using System.Collections.Concurrent;
using TableService.Models;

namespace TableService.Services;

public class TableManager
{
    private readonly ConcurrentDictionary<int, string?> _forks;
    private readonly ConcurrentDictionary<string, PhilosopherStats> _philosopherStats;
    private readonly int _philosophersCount;
    private int _activePhilosophers;

    public TableManager(IConfiguration configuration)
    {
        _philosophersCount = int.Parse(configuration["PHILOSOPHERS_COUNT"] ?? "5");
        _forks = new ConcurrentDictionary<int, string?>();
        _philosopherStats = new ConcurrentDictionary<string, PhilosopherStats>();
        _activePhilosophers = _philosophersCount;

        // Initialize forks (1 to philosophersCount)
        for (int i = 1; i <= _philosophersCount; i++)
        {
            _forks[i] = null;
        }
    }

    public ForkResponse TryTakeFork(int forkId, string philosopherId)
    {
        if (!_forks.ContainsKey(forkId))
        {
            return new ForkResponse { Success = false, Message = $"Fork {forkId} does not exist" };
        }

        if (_forks.TryGetValue(forkId, out var currentOwner) && currentOwner != null)
        {
            return new ForkResponse { Success = false, Message = $"Fork {forkId} is already taken by {currentOwner}" };
        }

        if (_forks.TryUpdate(forkId, philosopherId, null))
        {
            return new ForkResponse { Success = true, Message = $"Fork {forkId} taken by {philosopherId}" };
        }

        return new ForkResponse { Success = false, Message = $"Failed to take fork {forkId}" };
    }

    public ForkResponse ReleaseFork(int forkId, string philosopherId)
    {
        if (!_forks.ContainsKey(forkId))
        {
            return new ForkResponse { Success = false, Message = $"Fork {forkId} does not exist" };
        }

        if (_forks.TryGetValue(forkId, out var currentOwner) && currentOwner != philosopherId)
        {
            return new ForkResponse { Success = false, Message = $"Fork {forkId} is not owned by {philosopherId}" };
        }

        if (_forks.TryUpdate(forkId, null, philosopherId))
        {
            return new ForkResponse { Success = true, Message = $"Fork {forkId} released by {philosopherId}" };
        }

        return new ForkResponse { Success = false, Message = $"Failed to release fork {forkId}" };
    }

    public void RegisterPhilosopher(string philosopherId, string philosopherName)
    {
        _philosopherStats.TryAdd(philosopherId, new PhilosopherStats
        {
            PhilosopherId = philosopherId,
            PhilosopherName = philosopherName,
            StartTime = DateTime.UtcNow
        });
    }

    public void UpdateStats(string philosopherId, int mealsEaten, int thinkingTime, int eatingTime, int hungryTime)
    {
        if (_philosopherStats.TryGetValue(philosopherId, out var stats))
        {
            stats.MealsEaten = mealsEaten;
            stats.TotalThinkingTime = thinkingTime;
            stats.TotalEatingTime = eatingTime;
            stats.TotalHungryTime = hungryTime;
        }
    }

    public void PhilosopherExited(string philosopherId)
    {
        if (_philosopherStats.TryGetValue(philosopherId, out var stats))
        {
            stats.EndTime = DateTime.UtcNow;
        }

        Interlocked.Decrement(ref _activePhilosophers);

        if (_activePhilosophers == 0)
        {
            PrintFinalStats();
        }
    }

    private void PrintFinalStats()
    {
        Console.WriteLine("\n========== FINAL STATISTICS ==========");
        Console.WriteLine($"Total Philosophers: {_philosophersCount}");
        Console.WriteLine("----------------------------------------");

        foreach (var stats in _philosopherStats.Values.OrderBy(s => s.PhilosopherId))
        {
            var duration = stats.EndTime.HasValue 
                ? (stats.EndTime.Value - stats.StartTime).TotalSeconds 
                : 0;

            Console.WriteLine($"Philosopher: {stats.PhilosopherName} ({stats.PhilosopherId})");
            Console.WriteLine($"  Meals Eaten: {stats.MealsEaten}");
            Console.WriteLine($"  Total Thinking Time: {stats.TotalThinkingTime}ms");
            Console.WriteLine($"  Total Eating Time: {stats.TotalEatingTime}ms");
            Console.WriteLine($"  Total Hungry Time: {stats.TotalHungryTime}ms");
            Console.WriteLine($"  Duration: {duration:F2} seconds");
            Console.WriteLine("----------------------------------------");
        }

        Console.WriteLine("========================================\n");
    }
}

