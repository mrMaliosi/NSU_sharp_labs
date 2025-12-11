namespace Lab1.DiningPhilosophers
{
    public sealed class ForkManager : IForkManager
    {
        private readonly Fork[] _forks;

        public ForkManager(int totalForks)
        {
            _forks = new Fork[totalForks];
            for (int i = 0; i < totalForks; i++)
            {
                _forks[i] = new Fork(i);
            }
        }

        public Fork[] GetAllForks() => _forks;
        
        public Fork GetFork(int id) => _forks[id];
        
        public int TotalForks => _forks.Length;
    }
}

