namespace HabitTracker.Models;

public class Habit
{
    // Guid er praktisk som unik ID (lett å generere lokalt).
    public Guid Id { get; set; } = Guid.NewGuid();

    public string Name { get; set; } = "";

    // Poeng per gjennomføring (brukes til rewards og motivasjon).
    public int Points { get; set; } = 10;

    // TODO (senere): Frekvens/schedule (daglig/ukentlig/custom),
    // måltype (checkbox/count/time), tags osv.
}
