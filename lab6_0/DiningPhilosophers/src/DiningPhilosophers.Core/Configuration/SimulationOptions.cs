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

        public void Validate()
        {
            if (SimulationDurationMs <= 0)
                throw new InvalidOperationException($"'{nameof(SimulationDurationMs)}' должен быть > 0");
            if (DisplayIntervalMs <= 0)
                throw new InvalidOperationException($"'{nameof(DisplayIntervalMs)}' должен быть > 0");
            if (TotalPhilosophers <= 0)
                throw new InvalidOperationException($"'{nameof(TotalPhilosophers)}' должен быть > 0");
            if (TotalForks <= 0)
                throw new InvalidOperationException($"'{nameof(TotalForks)}' должен быть > 0");
            if (string.IsNullOrWhiteSpace(Strategy))
                throw new InvalidOperationException($"'{nameof(Strategy)}' не может быть пустым");
        }
    }
}

