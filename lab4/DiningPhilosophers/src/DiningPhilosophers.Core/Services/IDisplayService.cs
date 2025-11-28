namespace Lab1.DiningPhilosophers
{
    public interface IDisplayService
    {
        void DisplayStats(Philosopher[] philosophers, Fork[] forks, long elapsedMs);
        void DisplayMetrics(Philosopher[] philosophers, IMetricsCalculator metrics);
    }
}

