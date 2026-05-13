using System;
using System.Security.Cryptography.X509Certificates;
using System.Diagnostics;
using System.Threading.Tasks;

namespace Lab1.DiningPhilosophers
{
	public enum PhilosopherState { 
		Thinking, 
		Hungry, 
		Eating 
	}

	public enum ActionType
	{
		None,
		TakeLeftFork,
        TakeRightFork,
		ReleaseLeftFork,
        ReleaseRightFork
	}

    /// <summary>
    /// Модель философа в задаче об обедающих философах.
    /// </summary>
    public sealed class Philosopher
    {
        //базовые поля
        public string Name { get; }
        public PhilosopherState State { get; private set; } = PhilosopherState.Thinking;
        public PhilosopherContext Context { get; }

        //вилки
        private Fork _left_fork;
        private Fork _right_fork;
        private bool _left_fork_picked = false;
        private bool _right_fork_picked = false;

        //поля состояния
        public ActionType LastAction { get; private set; } = ActionType.None;
        public int MillisecondsRemainingInState { get; private set; } = 0;
        
        private DateTime _stateStartTime;
        private DateTime _hungryStartTime;
        private long _totalWaitingTimeMs = 0;

        //статистика
        public int MealsEaten { get; private set; } = 0;
        public event Action<string>? OnMealEaten;

        public Philosopher(string name, Fork leftFork, Fork rightFork, PhilosopherContext context)
        {
            Name = name;
            _left_fork = leftFork;
            _right_fork = rightFork;
            Context = context;
            _stateStartTime = DateTime.Now;
            MillisecondsRemainingInState = Context.GetThinkingDuration();
        }

        public bool PickLeftFork() 
        {
            if (_left_fork.TryAcquire(this))
            {
                _left_fork_picked = true;
                LastAction = ActionType.TakeLeftFork;
                Thread.Sleep(Context.GetForkPickDuration());
                return true;
            }
            return false;
        }

        public bool PickRightFork() 
        {
            if (_right_fork.TryAcquire(this))
            {
                _right_fork_picked = true;
                LastAction = ActionType.TakeRightFork;
                Thread.Sleep(Context.GetForkPickDuration());
                return true;
            }
            return false;
        }

        // Осознать тщетность бытия - отпустить вилки
        public void RealizeFutilityOfBeing() {
            _left_fork.Release();
            _right_fork.Release();
            _left_fork_picked = false;
            _right_fork_picked = false;
        }

        private void ChangeState() {
            if (MillisecondsRemainingInState <= 0) {
                switch (State)
                {
                    case PhilosopherState.Thinking:
                        State = PhilosopherState.Hungry;
                        _hungryStartTime = DateTime.Now;
                        _stateStartTime = DateTime.Now;
                        break;
                    case PhilosopherState.Hungry:
                        LastAction = ActionType.None;
                        if (_left_fork_picked && _right_fork_picked) 
                        {
                            State = PhilosopherState.Eating;
                            _stateStartTime = DateTime.Now;
                            MillisecondsRemainingInState = Context.GetEatingDuration();
                        }
                        break;
                    case PhilosopherState.Eating:
                        RealizeFutilityOfBeing();
                        _stateStartTime = DateTime.Now;
                        MillisecondsRemainingInState = Context.GetThinkingDuration();
                        State = PhilosopherState.Thinking;
                        LastAction = ActionType.ReleaseLeftFork;
                        ++MealsEaten;
                        OnMealEaten?.Invoke(Name);
                        break;
                }
            } else {
                if (State == PhilosopherState.Thinking) 
                {
                    LastAction = ActionType.None;
                }
            }
        }

        // Осознать течение времени
        public void RealiseThePassageOfTime() 
        {
            var elapsed = (DateTime.Now - _stateStartTime).TotalMilliseconds;
            MillisecondsRemainingInState = Math.Max(0, MillisecondsRemainingInState - (int)elapsed);
            
            if (State == PhilosopherState.Hungry)
            {
                _totalWaitingTimeMs += (long)elapsed;
            }
            
            _stateStartTime = DateTime.Now;
            ChangeState();
        }

        public bool IsBusy() 
        {
            return MillisecondsRemainingInState > 0;
        }

        public long GetTotalWaitingTimeMs() => _totalWaitingTimeMs;
    }
}


