using System;
using System.Linq;
using System.Threading.Tasks;
using DiningPhilosophers.Core.Data;
using DiningPhilosophers.Core.Services;
using Lab1.DiningPhilosophers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Xunit;

namespace DiningPhilosophers.Tests
{
    public class SimulationStateServiceTests : IDisposable
    {
        private readonly SimulationDbContext _context;
        private readonly SimulationStateService _service;
        private readonly ILogger<SimulationStateService> _logger;

        public SimulationStateServiceTests()
        {
            var options = new DbContextOptionsBuilder<SimulationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new SimulationDbContext(options);
            _context.Database.EnsureCreated();

            var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
            _logger = loggerFactory.CreateLogger<SimulationStateService>();

            _service = new SimulationStateService(_context, _logger);
        }

        [Fact]
        public void StartSimulation_ShouldCreateSimulationRun()
        {
            // Arrange
            int totalPhilosophers = 5;
            int totalForks = 5;
            string strategy = "naive";

            // Act
            var runId = _service.StartSimulation(totalPhilosophers, totalForks, strategy);

            // Assert
            Assert.NotEqual(Guid.Empty, runId);
            var simulationRun = _context.SimulationRuns.FirstOrDefault(r => r.RunId == runId);
            Assert.NotNull(simulationRun);
            Assert.Equal(totalPhilosophers, simulationRun.TotalPhilosophers);
            Assert.Equal(totalForks, simulationRun.TotalForks);
            Assert.Equal(strategy, simulationRun.Strategy);
        }

        [Fact]
        public async Task SaveStateAsync_ShouldSavePhilosopherSnapshots()
        {
            // Arrange
            var runId = _service.StartSimulation(2, 2, "naive");
            var forks = new[] { new Fork(0), new Fork(1) };
            var philosophers = new[]
            {
                new Philosopher("Philosopher1", forks[0], forks[1], 
                    new PhilosopherContext(new Segment(10, 20), new Segment(10, 20), 5)),
                new Philosopher("Philosopher2", forks[1], forks[0], 
                    new PhilosopherContext(new Segment(10, 20), new Segment(10, 20), 5))
            };

            // Act
            await _service.SaveStateAsync(philosophers, forks, 1.5);

            // Assert
            var snapshots = _context.PhilosopherSnapshots.ToList();
            Assert.Equal(2, snapshots.Count);
            Assert.All(snapshots, s => Assert.Equal(1.5, s.ElapsedSeconds));
            Assert.Contains(snapshots, s => s.PhilosopherName == "Philosopher1");
            Assert.Contains(snapshots, s => s.PhilosopherName == "Philosopher2");
        }

        [Fact]
        public async Task SaveStateAsync_ShouldSaveForkSnapshots()
        {
            // Arrange
            var runId = _service.StartSimulation(2, 2, "naive");
            var forks = new[] { new Fork(0), new Fork(1) };
            var philosophers = new[]
            {
                new Philosopher("Philosopher1", forks[0], forks[1], 
                    new PhilosopherContext(new Segment(10, 20), new Segment(10, 20), 5))
            };

            // Act
            await _service.SaveStateAsync(philosophers, forks, 2.0);

            // Assert
            var snapshots = _context.ForkSnapshots.ToList();
            Assert.Equal(2, snapshots.Count);
            Assert.All(snapshots, s => Assert.Equal(2.0, s.ElapsedSeconds));
            Assert.Contains(snapshots, s => s.ForkId == 0);
            Assert.Contains(snapshots, s => s.ForkId == 1);
        }

        [Fact]
        public async Task SaveStateAsync_ShouldSaveForkStateCorrectly()
        {
            // Arrange
            var runId = _service.StartSimulation(1, 1, "naive");
            var fork = new Fork(0);
            var philosopher = new Philosopher("Philosopher1", fork, fork, 
                new PhilosopherContext(new Segment(10, 20), new Segment(10, 20), 5));
            
            fork.TryAcquire(philosopher);

            // Act
            await _service.SaveStateAsync(new[] { philosopher }, new[] { fork }, 1.0);

            // Assert
            var snapshot = _context.ForkSnapshots.First();
            Assert.Equal("InUse", snapshot.State);
            Assert.Equal("Philosopher1", snapshot.HeldByPhilosopherName);
        }

        [Fact]
        public async Task MultipleSaveStateAsync_ShouldCreateMultipleSnapshots()
        {
            // Arrange
            var runId = _service.StartSimulation(1, 1, "naive");
            var fork = new Fork(0);
            var philosopher = new Philosopher("Philosopher1", fork, fork, 
                new PhilosopherContext(new Segment(10, 20), new Segment(10, 20), 5));

            // Act
            await _service.SaveStateAsync(new[] { philosopher }, new[] { fork }, 1.0);
            await _service.SaveStateAsync(new[] { philosopher }, new[] { fork }, 2.0);
            await _service.SaveStateAsync(new[] { philosopher }, new[] { fork }, 3.0);

            // Assert
            var philosopherSnapshots = _context.PhilosopherSnapshots.ToList();
            var forkSnapshots = _context.ForkSnapshots.ToList();
            
            Assert.Equal(3, philosopherSnapshots.Count);
            Assert.Equal(3, forkSnapshots.Count);
            
            Assert.Contains(philosopherSnapshots, s => Math.Abs(s.ElapsedSeconds - 1.0) < 0.001);
            Assert.Contains(philosopherSnapshots, s => Math.Abs(s.ElapsedSeconds - 2.0) < 0.001);
            Assert.Contains(philosopherSnapshots, s => Math.Abs(s.ElapsedSeconds - 3.0) < 0.001);
        }

        public void Dispose()
        {
            _context?.Dispose();
        }
    }
}

