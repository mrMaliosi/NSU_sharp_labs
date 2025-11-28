using Microsoft.EntityFrameworkCore;

namespace DiningPhilosophers.Core.Data
{
    public class SimulationDbContext : DbContext
    {
        public SimulationDbContext(DbContextOptions<SimulationDbContext> options)
            : base(options)
        {
        }

        public DbSet<SimulationRun> SimulationRuns { get; set; } = null!;
        public DbSet<PhilosopherStateSnapshot> PhilosopherSnapshots { get; set; } = null!;
        public DbSet<ForkStateSnapshot> ForkSnapshots { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<SimulationRun>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.RunId).IsUnique();
                entity.Property(e => e.RunId).IsRequired();
                entity.Property(e => e.StartTime).IsRequired();
                entity.Property(e => e.Strategy).HasMaxLength(100);
            });

            modelBuilder.Entity<PhilosopherStateSnapshot>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => new { e.SimulationRunId, e.Timestamp });
                entity.HasOne(e => e.SimulationRun)
                    .WithMany(r => r.PhilosopherSnapshots)
                    .HasForeignKey(e => e.SimulationRunId)
                    .OnDelete(DeleteBehavior.Cascade);
                entity.Property(e => e.PhilosopherName).HasMaxLength(200).IsRequired();
                entity.Property(e => e.State).HasMaxLength(50).IsRequired();
                entity.Property(e => e.LastAction).HasMaxLength(50).IsRequired();
            });

            modelBuilder.Entity<ForkStateSnapshot>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => new { e.SimulationRunId, e.Timestamp });
                entity.HasOne(e => e.SimulationRun)
                    .WithMany(r => r.ForkSnapshots)
                    .HasForeignKey(e => e.SimulationRunId)
                    .OnDelete(DeleteBehavior.Cascade);
                entity.Property(e => e.State).HasMaxLength(50).IsRequired();
                entity.Property(e => e.HeldByPhilosopherName).HasMaxLength(200);
            });
        }
    }
}

