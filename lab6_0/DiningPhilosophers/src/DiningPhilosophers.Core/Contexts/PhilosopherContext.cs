using System;

namespace Lab1.DiningPhilosophers
{
	public sealed class PhilosopherContext
	{
		public Segment thinkingTime { get; }
		public Segment eatingTime { get; }
        public int forkPickTime { get; }

		public PhilosopherContext(Segment thinking, Segment eating, int forkPick)
		{
			thinkingTime = thinking;
			eatingTime = eating;
			forkPickTime = forkPick;
		}

		public PhilosopherContext(PhilosopherContextConfig pConf) 
		{
			var thinkingTimeJson = new Segment(
				pConf.ThinkingTime.From,
				pConf.ThinkingTime.To);

			var eatingTimeJson = new Segment(
				pConf.EatingTime.From,
				pConf.EatingTime.To);

			thinkingTime = thinkingTimeJson;
			eatingTime = eatingTimeJson;
			forkPickTime = pConf.ForkPickTime;
		}

		public int GetThinkingDuration() => thinkingTime.Next();
        public int GetEatingDuration() => eatingTime.Next();
        public int GetForkPickDuration() => forkPickTime;
    }
}


