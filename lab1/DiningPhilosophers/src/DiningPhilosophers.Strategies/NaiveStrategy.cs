using System.Security.Cryptography.X509Certificates;
using Lab1.DiningPhilosophers;

namespace Lab1.DiningPhilosophers
{
    public sealed class NaiveStrategy : IPhilosopherStrategy
    {
        private static Random? _rand = new Random();

        static void tryToPickFork(Philosopher philosopher)
        {
            int forksNum = philosopher.GetForksNum();
            var forkIndices = Enumerable.Range(0, forksNum).ToList();

            if (_rand == null)
            {
                _rand = new Random();
            }

            // Перемешиваем индексы
            for (int i = forkIndices.Count - 1; i > 0; i--)
            {
                int j = _rand.Next(i + 1);
                (forkIndices[i], forkIndices[j]) = (forkIndices[j], forkIndices[i]);
            }

            // Пробуем взять по порядку
            foreach (var forkNum in forkIndices)
            {
                if (philosopher.PickFork(forkNum))
                {
                    return; // Успешно взяли – выходим
                }
            }
        }

        static void checkFutilityOfBeing(Philosopher philosopher)
        {
            if (-philosopher.Context.eatingTime.To * 10 > philosopher.StepsRemainingInState) {
                int chance = _rand.Next(100);
                if (chance <  -philosopher.StepsRemainingInState - philosopher.Context.eatingTime.To * 10) 
                {
                    philosopher.RealizeFutilityOfBeing();
                }
            }
        }

        public void PerformAction(Philosopher philosopher, SimulationContext context)
        {
            if (!philosopher.IsBusy()) 
            {
                switch (philosopher.State) 
                {
                    case PhilosopherState.Thinking:
                        // No actions
                        break;
                    case PhilosopherState.Hungry:
                        tryToPickFork(philosopher);
                        checkFutilityOfBeing(philosopher);
                        break;
                    case PhilosopherState.Eating:
                        // No actions
                        break;
                }
            }
            philosopher.RealiseThePassageOfTime();
        }
    }
}
