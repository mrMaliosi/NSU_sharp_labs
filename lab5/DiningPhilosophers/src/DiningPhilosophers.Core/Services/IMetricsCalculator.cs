namespace Lab1.DiningPhilosophers
{
    public interface IMetricsCalculator
    {
        void StartSimulation(Fork[] forks);
        void OnStep(Fork[] forks, Philosopher[] philosophers);
        void OnMeal(string philosopherName);
        double GetThroughput();
        Dictionary<string, double> GetAverageWaitingTime();
        Dictionary<int, double> GetForkUtilization();
        long TotalSimulationTimeMs { get; }
    }
}

