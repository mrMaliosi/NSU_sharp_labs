namespace Lab1.DiningPhilosophers
{
    public sealed class SimulationOptions
    {
        public const string SectionName = "Simulation";
        
        public int SimulationDurationMs { get; set; }
        public int DisplayIntervalMs { get; set; }
        public int TotalPhilosophers { get; set; }
        public int TotalForks { get; set; }
        public string Strategy { get; set; } = "naive";
    }
}

