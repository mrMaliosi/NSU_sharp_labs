using Lab1.DiningPhilosophers;

namespace Lab1.DiningPhilosophers
{
    public interface ICoordinator
    {
        event Action<Philosopher> PhilosopherReady;
        void RequestPermission(Philosopher philosopher);
    }
}
