namespace Lab1.DiningPhilosophers
{
    public sealed class PhilosophersOptions
    {
        public const string SectionName = "Philosophers";
        
        public string[]? Names { get; set; }
        public PhilosopherContextConfig? DefaultContext { get; set; }
        public PhilosopherConfig[]? Philosophers { get; set; }
    }
}

