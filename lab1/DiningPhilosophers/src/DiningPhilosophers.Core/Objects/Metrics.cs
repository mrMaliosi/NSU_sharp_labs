using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Lab1.DiningPhilosophers
{
	public sealed class Metrics
	{
		public int TotalSteps { get; private set; }
		public readonly Dictionary<string, int> MealsByPhilosopher = new();
		public readonly Dictionary<string, int> HungryTimeByPhilosopher = new();
		public readonly Dictionary<string, int> MaxHungryTimeByPhilosopher = new();
		private readonly Dictionary<string, int> StartHungryTimeByPhilosopher = new();
		public readonly Dictionary<int, int> ForkBusySteps = new();
		public readonly Dictionary<int, int> ForkFreeSteps = new();
		public readonly Dictionary<int, int> ForkUsingSteps = new();

		public void OnStep(IEnumerable<Fork> forks, IEnumerable<Philosopher> philosophers)
		{
			TotalSteps++;
			foreach (var fork in forks)
			{
				if (!ForkBusySteps.ContainsKey(fork.Id)) ForkBusySteps[fork.Id] = 0;
				if (!ForkFreeSteps.ContainsKey(fork.Id)) ForkFreeSteps[fork.Id] = 0;
				if (!ForkUsingSteps.ContainsKey(fork.Id)) ForkUsingSteps[fork.Id] = 0;
                if (fork.State == ForkState.InUse)
                {
                    Debug.Assert(fork.HeldBy != null);
                    if (fork.HeldBy.State == PhilosopherState.Eating)
					{
                        ForkUsingSteps[fork.Id]++;
                    } 
					else 
					{
						ForkBusySteps[fork.Id]++;
					}
                }
                else ForkFreeSteps[fork.Id]++;
            }
			foreach (var p in philosophers)
			{
				if (!MealsByPhilosopher.ContainsKey(p.Name)) MealsByPhilosopher[p.Name] = 0;
				if (!HungryTimeByPhilosopher.ContainsKey(p.Name)) HungryTimeByPhilosopher[p.Name] = 0;
				if (!MaxHungryTimeByPhilosopher.ContainsKey(p.Name)) MaxHungryTimeByPhilosopher[p.Name] = 0;
				if (!StartHungryTimeByPhilosopher.ContainsKey(p.Name)) StartHungryTimeByPhilosopher[p.Name] = -1;
                if (p.State == PhilosopherState.Hungry)
                {
                    HungryTimeByPhilosopher[p.Name]++;
					if (StartHungryTimeByPhilosopher[p.Name] == -1)
					{
                        StartHungryTimeByPhilosopher[p.Name] = TotalSteps - 1;
                    } else 
					{
                        int hungerness = TotalSteps - StartHungryTimeByPhilosopher[p.Name];
						MaxHungryTimeByPhilosopher[p.Name] = MaxHungryTimeByPhilosopher[p.Name] < hungerness ? hungerness : MaxHungryTimeByPhilosopher[p.Name];
                    }
                }
                if (p.State == PhilosopherState.Eating)
                {
					if (StartHungryTimeByPhilosopher[p.Name] != -1) 
					{
						int hungerness = TotalSteps - StartHungryTimeByPhilosopher[p.Name];
						MaxHungryTimeByPhilosopher[p.Name] = MaxHungryTimeByPhilosopher[p.Name] < hungerness ? hungerness : MaxHungryTimeByPhilosopher[p.Name];
						StartHungryTimeByPhilosopher[p.Name] = -1;
					}
                }
            }
		}

		public void OnMeal(string name)
		{
			if (!MealsByPhilosopher.ContainsKey(name)) MealsByPhilosopher[name] =  0;
			MealsByPhilosopher[name]++;
		}
	}
}


