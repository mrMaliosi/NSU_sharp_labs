using System.Collections.Concurrent;

namespace TableService.Services;

public class TableStateService
{
    private readonly ConcurrentDictionary<string, PhilosopherProxy> _philosophers = new();
    private readonly ConcurrentDictionary<string, bool> _exitedPhilosophers = new();
    private readonly object _lock = new object();
    private int _expectedPhilosophersCount = 5;

    public void RegisterPhilosopher(PhilosopherProxy philosopher)
    {
        _philosophers.TryAdd(philosopher.Id, philosopher);
    }

    public void PhilosopherExit(string philosopherId)
    {
        _exitedPhilosophers.TryAdd(philosopherId, true);
    }

    public bool AllPhilosophersExited()
    {
        return _exitedPhilosophers.Count >= _philosophers.Count && _philosophers.Count > 0;
    }

    public PhilosopherProxy[] GetRegisteredPhilosophers()
    {
        return _philosophers.Values.ToArray();
    }

    public void SetExpectedPhilosophersCount(int count)
    {
        _expectedPhilosophersCount = count;
    }
}

