using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using Lab1.DiningPhilosophers;

namespace Lab1.DiningPhilosophers
{
    public sealed class NaiveStrategy : IPhilosopherStrategy
    {
        private static Random _rand = new Random();

        static void tryToPickFork(Philosopher philosopher)
        {
            int forksNum = 2;
            int chooseFork = _rand.Next(forksNum);
            switch (chooseFork)
            {
                case 0:
                    philosopher.PickLeftFork();
                    break;
                case 1:
                    philosopher.PickRightFork();
                    break;
            }
        }

        static void checkFutilityOfBeing(Philosopher philosopher)
        {
            if (-philosopher.Context.eatingTime.To * 10 > philosopher.MillisecondsRemainingInState) {
                int chance = _rand.Next(100);
                if (chance <  -philosopher.MillisecondsRemainingInState - philosopher.Context.eatingTime.To * 10) 
                {
                    philosopher.RealizeFutilityOfBeing();
                }
            }
        }

        public void PerformAction(Philosopher philosopher)
        {
            switch (philosopher.State) 
            {
                case PhilosopherState.Thinking:
                    // No actions - философ думает
                    break;
                case PhilosopherState.Hungry:
                    // Философ пытается взять вилки, даже если он "занят" другими действиями
                    tryToPickFork(philosopher);
                    checkFutilityOfBeing(philosopher);
                    break;
                case PhilosopherState.Eating:
                    // No actions - философ ест
                    break;
            }
            philosopher.RealiseThePassageOfTime();
        }
    }
}
