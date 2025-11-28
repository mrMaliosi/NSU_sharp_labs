using Lab1.DiningPhilosophers;

namespace Lab1.DiningPhilosophers
{
    /// <summary>
    /// Стратегия "Иерархия ресурсов" - философы всегда берут вилки в определенном порядке
    /// (сначала вилку с меньшим ID, затем с большим ID)
    /// </summary>
    public sealed class ResourceHierarchyStrategy : IPhilosopherStrategy
    {
        public void PerformAction(Philosopher philosopher, Philosopher[] allPhilosophers, Fork[] allForks)
        {
            if (!philosopher.IsBusy())
            {
                switch (philosopher.State)
                {
                    case PhilosopherState.Thinking:
                        // No actions
                        break;
                    case PhilosopherState.Hungry:
                        // Всегда берем сначала вилку с меньшим ID, затем с большим
                        TryPickForksInOrder(philosopher);
                        break;
                    case PhilosopherState.Eating:
                        // No actions
                        break;
                }
            }
            philosopher.RealiseThePassageOfTime();
        }

        private void TryPickForksInOrder(Philosopher philosopher)
        {
            // Стратегия "Иерархия ресурсов": всегда берем вилки в определенном порядке
            // Для упрощения: всегда сначала левую, затем правую
            // Это гарантирует, что все философы используют одинаковый порядок взятия вилок
            if (!philosopher.PickLeftFork())
                return;
            philosopher.PickRightFork();
        }
    }
}

