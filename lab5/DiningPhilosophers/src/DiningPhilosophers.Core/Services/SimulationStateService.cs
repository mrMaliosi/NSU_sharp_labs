using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using DiningPhilosophers.Core.Data;
using Lab1.DiningPhilosophers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace DiningPhilosophers.Core.Services
{
    public class SimulationStateService : ISimulationStateService
    {
        private readonly SimulationDbContext _context;
        private readonly ILogger<SimulationStateService> _logger;
        private Guid _currentRunId;
        private DateTime _simulationStartTime;
        private int _currentSimulationRunId;

        public SimulationStateService(
            SimulationDbContext context,
            ILogger<SimulationStateService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public Guid StartSimulation(int totalPhilosophers, int totalForks, string strategy)
        {
            _currentRunId = Guid.NewGuid();
            _simulationStartTime = DateTime.UtcNow;

            var simulationRun = new SimulationRun
            {
                RunId = _currentRunId,
                StartTime = _simulationStartTime,
                TotalPhilosophers = totalPhilosophers,
                TotalForks = totalForks,
                Strategy = strategy
            };

            _context.SimulationRuns.Add(simulationRun);
            _context.SaveChanges();

            _currentSimulationRunId = simulationRun.Id;

            _logger.LogInformation("Симуляция запущена с RunId: {RunId}", _currentRunId);
            Console.WriteLine($"RunId: {_currentRunId}");

            return _currentRunId;
        }

        public async Task SaveStateAsync(Philosopher[] philosophers, Fork[] forks, double elapsedSeconds)
        {
            try
            {
                var timestamp = _simulationStartTime.AddSeconds(elapsedSeconds);

                // Сохраняем состояние философов
                foreach (var philosopher in philosophers)
                {
                    var snapshot = new PhilosopherStateSnapshot
                    {
                        SimulationRunId = _currentSimulationRunId,
                        PhilosopherName = philosopher.Name,
                        State = philosopher.State.ToString(),
                        LastAction = philosopher.LastAction.ToString(),
                        MealsEaten = philosopher.MealsEaten,
                        ElapsedSeconds = elapsedSeconds,
                        Timestamp = timestamp
                    };
                    _context.PhilosopherSnapshots.Add(snapshot);
                }

                // Сохраняем состояние вилок
                foreach (var fork in forks)
                {
                    var (state, heldBy) = fork.GetState();
                    var snapshot = new ForkStateSnapshot
                    {
                        SimulationRunId = _currentSimulationRunId,
                        ForkId = fork.Id,
                        State = state.ToString(),
                        HeldByPhilosopherName = heldBy?.Name,
                        ElapsedSeconds = elapsedSeconds,
                        Timestamp = timestamp
                    };
                    _context.ForkSnapshots.Add(snapshot);
                }

                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при сохранении состояния симуляции");
                throw;
            }
        }

        public async Task CompleteSimulationAsync()
        {
            try
            {
                await _context.SaveChangesAsync();
                _logger.LogInformation("Симуляция завершена. RunId: {RunId}", _currentRunId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при завершении симуляции");
                throw;
            }
        }
    }
}

