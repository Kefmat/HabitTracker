using HabitTracker.Models;
using Microsoft.EntityFrameworkCore;

namespace HabitTracker.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    // DbSet = tabeller i databasen
    public DbSet<Habit> Habits => Set<Habit>();
    public DbSet<Completion> Completions => Set<Completion>();
    public DbSet<Reward> Rewards => Set<Reward>();
    public DbSet<RewardUnlock> RewardUnlocks => Set<RewardUnlock>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Litt datarens/constraints: maks lengde på navn.
        modelBuilder.Entity<Habit>()
            .Property(h => h.Name)
            .HasMaxLength(200);

        modelBuilder.Entity<Reward>()
            .Property(r => r.Name)
            .HasMaxLength(200);

        // Viktig: Sikrer at vi ikke kan registrere samme vane som fullført
        // flere ganger på samme dato.
        modelBuilder.Entity<Completion>()
            .HasIndex(c => new { c.HabitId, c.Date })
            .IsUnique();

        // Nyttig indeks for datospørringer (7-dagers visning osv.).
        modelBuilder.Entity<Completion>()
            .HasIndex(c => c.Date);

        // Sikrer at en reward ikke kan "låses opp" flere ganger.
        modelBuilder.Entity<RewardUnlock>()
            .HasIndex(u => u.RewardId)
            .IsUnique();
    }
}
