using System;
using System.Diagnostics;

namespace Lab1.DiningPhilosophers
{
	public sealed class Segment
    {
        public int From { get; set; }
        public int To { get; set; }

        public Segment(int from, int to)
        {
            From = from;
            To = to;

            Debug.Assert(from <= to);
        }

        public int Next() => RngProvider.Instance.Next(From, To + 1);
    }
}
