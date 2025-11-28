using System;
using System.Collections.Generic;

namespace DiningPhilosophers.Core.Data
{
    public class SimulationRun
    {
        public int Id { get; set; }
        public Guid RunId { get; set; }
        public DateTime StartTime { get; set; }
        public int TotalPhilosophers { get; set; }
        public int TotalForks { get; set; }
        public string Strategy { get; set; } = string.Empty;
        
        public ICollection<PhilosopherStateSnapshot> PhilosopherSnapshots { get; set; } = new List<PhilosopherStateSnapshot>();
        public ICollection<ForkStateSnapshot> ForkSnapshots { get; set; } = new List<ForkStateSnapshot>();
    }
}

