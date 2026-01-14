namespace HabitTracker.Models;

public class Completion
{
    public Guid Id { get; set; } = Guid.NewGuid();

    // FK til Habit
    public Guid HabitId { get; set; }

    // Navigasjon (valgfri, men nyttig når man vil include/joine senere)
    public Habit? Habit { get; set; }

    // DateOnly er fin for "dag"-logikk (streaks, historikk).
    public DateOnly Date { get; set; } = DateOnly.FromDateTime(DateTime.Now);

    // TODO (senere): Kilde (Manual/Garmin/Goodreads/Lifesum), notat, verdi (antall/minutter).
}
