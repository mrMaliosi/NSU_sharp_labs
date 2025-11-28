using System;

namespace DiningPhilosophers.Core.Data
{
    public class ForkStateSnapshot
    {
        public int Id { get; set; }
        public int SimulationRunId { get; set; }
        public SimulationRun SimulationRun { get; set; } = null!;
        public int ForkId { get; set; }
        public string State { get; set; } = string.Empty; // Available, InUse
        public string? HeldByPhilosopherName { get; set; }
        public double ElapsedSeconds { get; set; }
        public DateTime Timestamp { get; set; }
    }
}

