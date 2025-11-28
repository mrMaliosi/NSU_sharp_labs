using System;
using System.Threading.Tasks;
using Lab1.DiningPhilosophers;

namespace DiningPhilosophers.Core.Services
{
    public interface ISimulationStateService
    {
        Guid StartSimulation(int totalPhilosophers, int totalForks, string strategy);
        Task SaveStateAsync(Philosopher[] philosophers, Fork[] forks, double elapsedSeconds);
        Task CompleteSimulationAsync();
    }
}

