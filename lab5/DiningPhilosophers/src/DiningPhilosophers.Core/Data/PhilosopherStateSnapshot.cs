using System;

namespace DiningPhilosophers.Core.Data
{
    public class PhilosopherStateSnapshot
    {
        public int Id { get; set; }
        public int SimulationRunId { get; set; }
        public SimulationRun SimulationRun { get; set; } = null!;
        public string PhilosopherName { get; set; } = string.Empty;
        public string State { get; set; } = string.Empty; // Thinking, Hungry, Eating
        public string LastAction { get; set; } = string.Empty; // None, TakeLeftFork, TakeRightFork, ReleaseLeftFork, ReleaseRightFork
        public int MealsEaten { get; set; }
        public double ElapsedSeconds { get; set; }
        public DateTime Timestamp { get; set; }
    }
}

