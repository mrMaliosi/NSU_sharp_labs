namespace Lab1.DiningPhilosophers
{
	public enum ForkState { Available, InUse }

	public sealed class Fork
	{
		private readonly object _lock = new object();
		
		public int Id { get; }
		public ForkState State { get; private set; } = ForkState.Available;
		public Philosopher? HeldBy { get; private set; }

        public Fork(int id) { Id = id; HeldBy = null; }

        public bool TryAcquire(Philosopher philosopher)
		{
			lock (_lock)
			{
				if (State == ForkState.Available)
				{
					State = ForkState.InUse;
					HeldBy = philosopher;
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
				State = ForkState.Available;
				HeldBy = null;
			}
		}
	}
}


