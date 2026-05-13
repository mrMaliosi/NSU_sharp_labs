namespace Lab1.DiningPhilosophers
{
	public enum ForkState { Available, InUse }

	public sealed class Fork
	{
		private readonly object _lock = new object();
		
		public int Id { get; }
		public ForkState State { get; private set; } = ForkState.Available;
		public Philosopher? HeldBy { get; private set; }
		
		// Статистика использования вилки
		private DateTime _lastStateChangeTime;
		private long _totalUsageTimeMs = 0;

        public Fork(int id) 
		{ 
			Id = id; 
			HeldBy = null; 
			_lastStateChangeTime = DateTime.Now;
		}

        public bool TryAcquire(Philosopher philosopher)
		{
			lock (_lock)
			{
				if (State == ForkState.Available)
				{
					// Обновляем статистику перед изменением состояния
					UpdateUsageStatistics();
					
					State = ForkState.InUse;
					HeldBy = philosopher;
					_lastStateChangeTime = DateTime.Now;
					return true;
				}
				return false;
			}
		}

        public string ?GetOwnerName() 
		{
			lock (_lock)
			{
				return HeldBy != null ? HeldBy.Name : null;
			}
		}

        public (ForkState state, Philosopher? heldBy) GetState()
		{
			lock (_lock)
			{
				return (State, HeldBy);
			}
		}

        public void Release()
		{
			lock (_lock)
			{
				UpdateUsageStatistics();
				
				State = ForkState.Available;
				HeldBy = null;
				_lastStateChangeTime = DateTime.Now;
			}
		}
		
		/// <summary>
		/// Обновляет статистику использования вилки
		/// </summary>
		private void UpdateUsageStatistics()
		{
			if (State == ForkState.InUse)
			{
				var now = DateTime.Now;
				var elapsed = (now - _lastStateChangeTime).TotalMilliseconds;
				_totalUsageTimeMs += (long)elapsed;
			}
		}
		
		/// <summary>
		/// Получает общее время использования вилки в миллисекундах
		/// </summary>
		public long GetTotalUsageTimeMs()
		{
			lock (_lock)
			{
				UpdateUsageStatistics();
				_lastStateChangeTime = DateTime.Now;
				
				return _totalUsageTimeMs;
			}
		}
		
		/// <summary>
		/// Получает коэффициент утилизации вилки в процентах
		/// </summary>
		public double GetUtilizationPercentage(long totalSimulationTimeMs)
		{
			lock (_lock)
			{
				if (totalSimulationTimeMs == 0) return 0;
				
				UpdateUsageStatistics();
				_lastStateChangeTime = DateTime.Now;
				
				return (double)_totalUsageTimeMs / totalSimulationTimeMs * 100;
			}
		}
	}
}


