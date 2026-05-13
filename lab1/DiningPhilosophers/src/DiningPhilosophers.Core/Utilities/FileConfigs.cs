namespace Lab1.DiningPhilosophers
{
    public sealed class SimulationConfig
    {
        public int SimulationSteps { get; set; }
        public int TotalPhilosophers { get; set; }
        public int TotalForks { get; set; }
        public string? Strategy { get; set; }
        public string[] ?PhilosopherNames { get; set; }
        public PhilosopherConfig[] ?Philosophers { get; set; }
        public PhilosopherContextConfig ?DefaultContext { get; set; }
    }

    public sealed class PhilosopherConfig
    {
        public required string Name { get; set; }
        public required PhilosopherContextConfig Context { get; set; }
    }

    public sealed class PhilosopherContextConfig
    {
        public RangeConfig ThinkingTime { get; set; } = new RangeConfig();
        public RangeConfig EatingTime { get; set; } = new RangeConfig();
        public int ForkPickTime { get; set; }
    }

    public sealed class RangeConfig
    {
        public int From { get; set; }
        public int To { get; set; }
    }
}