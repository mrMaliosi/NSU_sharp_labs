namespace Lab1.DiningPhilosophers
{
    public sealed class RngProvider
    {
        private static Random? _instance;
        private static readonly object _lock = new object();

        private RngProvider() { }

        public static Random Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (_lock)
                    {
                        if (_instance == null)
                        {
                            _instance = new Random(); // без seed — случайный запуск
                        }
                    }
                }
                return _instance;
            }
        }

        /// <summary>
        /// Устанавливает seed. Вызывать можно только один раз
        /// до первого обращения к Instance.
        /// </summary>
        public static void Init(int seed)
        {
            lock (_lock)
            {
                if (_instance != null)
                    throw new InvalidOperationException("RngProvider уже инициализирован");

                _instance = new Random(seed);
            }
        }
    }
}