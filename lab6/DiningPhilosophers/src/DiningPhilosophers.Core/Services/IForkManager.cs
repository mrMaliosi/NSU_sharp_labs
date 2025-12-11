namespace Lab1.DiningPhilosophers
{
    public interface IForkManager
    {
        Fork[] GetAllForks();
        Fork GetFork(int id);
        int TotalForks { get; }
    }
}

