using Lab1.DiningPhilosophers;

namespace Lab1.DiningPhilosophers
{
    public sealed class CoordinatedStrategy : IPhilosopherStrategy
    {
        private readonly ICoordinator _coordinator;

        public CoordinatedStrategy(ICoordinator coordinator)
        {
            _coordinator = coordinator;
            
            // Подписываемся на событие готовности философа
            if (coordinator is SimpleCoordinator simpleCoordinator)
            {
                simpleCoordinator.PhilosopherReady += OnPhilosopherReady;
            }
        }

        public void PerformAction(Philosopher philosopher, SimulationContext context)
        {
            if (!philosopher.IsBusy())
            {
                switch (philosopher.State)
                {
                    case PhilosopherState.Thinking:
                        // Ничего не делаем, философ думает
                        break;
                    case PhilosopherState.Hungry:
                        // Запрашиваем разрешение на еду
                        _coordinator.RequestPermission(philosopher);
                        break;
                    case PhilosopherState.Eating:
                        // Ничего не делаем, философ ест
                        break;
                }
            }
            
            // Обновляем состояние философа
            philosopher.RealiseThePassageOfTime();
            
            // Если философ закончил есть, уведомляем координатора
            if (philosopher.State == PhilosopherState.Thinking && philosopher.LastAction == ActionType.ReleaseFork)
            {
                if (_coordinator is SimpleCoordinator simpleCoordinator)
                {
                    simpleCoordinator.OnPhilosopherFinishedEating(philosopher);
                }
            }
        }

        private void OnPhilosopherReady(Philosopher philosopher)
        {
            // Философ получил разрешение и уже взял вилки
            // Дополнительных действий не требуется
        }
    }
}
