using Lab1.DiningPhilosophers;

namespace Lab1.DiningPhilosophers
{
    public interface IPhilosopherStrategy
    {
        void PerformAction(Philosopher philosopher, Philosopher[] allPhilosophers, Fork[] allForks);
    }
}
