namespace Lab1.DiningPhilosophers
{
	public enum ForkState { Available, InUse }

	public sealed class Fork
	{
		public int Id { get; }
		public ForkState State { get; private set; } = ForkState.Available;
		public Philosopher? HeldBy { get; private set; }

        public Fork(int id) { Id = id; HeldBy = null; }

        public bool TryAcquire(Philosopher philosopher)
		{
			if (State == ForkState.Available)
			{
				State = ForkState.InUse;
				HeldBy = philosopher;
				return true;
			}
			return false;
		}

        public string ?GetOwnerName() => HeldBy != null ? HeldBy.Name : null;

        public void Release()
		{
			State = ForkState.Available;
			HeldBy = null;
		}
	}
}


