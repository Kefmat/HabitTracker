namespace HabitTracker.Models;

public class Reward
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public string Name { get; set; } = "";

    // Hvor mange all-time poeng som kreves for å låse opp.
    public int CostPoints { get; set; } = 100;

    // TODO (senere): kategori (digital/IRL), beskrivelse, bilde/ikon, "claim"-status.
}
