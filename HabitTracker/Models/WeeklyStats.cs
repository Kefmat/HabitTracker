namespace HabitTracker.Models;

/// Statistikk for en periode (typisk siste 7 dager).
/// Brukes kun i UI (ikke DB-tabell).

public class WeeklyStats
{
    public List<DailyStat> Days { get; set; } = new();

    public int TotalPoints => Days.Sum(d => d.Points);
    public int TotalCompletions => Days.Sum(d => d.Completions);

    // Mest fullførte vane i perioden (kan være null hvis ingen data)
    public string? TopHabitName { get; set; }
    public int TopHabitCompletions { get; set; }
}


/// Statistikk per dag.

public class DailyStat
{
    public DateOnly Date { get; set; }
    public int Points { get; set; }
    public int Completions { get; set; }
}
