using System.Collections.Generic;
using Lab1.DiningPhilosophers;

namespace Lab1.DiningPhilosophers
{
    public sealed class SimpleCoordinator : ICoordinator
    {
        private readonly Queue<Philosopher> _waitingQueue = new();
        private readonly HashSet<Philosopher> _eatingPhilosophers = new();
        private readonly object _lock = new object();

        public event Action<Philosopher>? PhilosopherReady;

        public void RequestPermission(Philosopher philosopher)
        {
            lock (_lock)
            {
                // Если философ уже ест, не добавляем в очередь
                if (_eatingPhilosophers.Contains(philosopher))
                    return;

                // Если философ не голоден, не обрабатываем запрос
                if (philosopher.State != PhilosopherState.Hungry)
                    return;

                // Добавляем в очередь ожидания
                if (!_waitingQueue.Contains(philosopher))
                {
                    _waitingQueue.Enqueue(philosopher);
                }

                // Обрабатываем очередь
                ProcessQueue();
            }
        }

        private void ProcessQueue()
        {
            var processed = new List<Philosopher>();

            while (_waitingQueue.Count > 0)
            {
                var philosopher = _waitingQueue.Dequeue();
                
                // Проверяем, может ли философ взять вилки
                if (CanPhilosopherEat(philosopher))
                {
                    _eatingPhilosophers.Add(philosopher);
                    PhilosopherReady?.Invoke(philosopher);
                }
                else
                {
                    // Возвращаем в очередь, если не может взять вилки
                    processed.Add(philosopher);
                }
            }

            // Возвращаем необработанных философов в очередь
            foreach (var philosopher in processed)
            {
                _waitingQueue.Enqueue(philosopher);
            }
        }

        private bool CanPhilosopherEat(Philosopher philosopher)
        {
            // Простая проверка: философ может есть, если его вилки свободны
            // и нет соседних философов, которые уже едят
            for (int i = 0; i < philosopher.GetForksNum(); i++)
            {
                if (!philosopher.PickFork(i))
                {
                    // Если не удалось взять вилку, отпускаем уже взятые
                    philosopher.RealizeFutilityOfBeing();
                    return false;
                }
            }
            return true;
        }

        public void OnPhilosopherFinishedEating(Philosopher philosopher)
        {
            lock (_lock)
            {
                _eatingPhilosophers.Remove(philosopher);
                // Обрабатываем очередь снова, так как освободились вилки
                ProcessQueue();
            }
        }
    }
}
