namespace HabitTracker.Models;

/// Enkel modell for å vise fremdrift mot neste reward i UI.
/// Dette er ikke en DB-tabell, kun et "view-model"/resultatobjekt.

public class RewardProgress
{
    public int TotalPoints { get; set; }

    // Neste reward som ikke er låst opp ennå (null hvis alle er unlocked eller ingen finnes).
    public Reward? NextReward { get; set; }

    // Hvor mange poeng som mangler for å låse opp NextReward.
    public int PointsToNext { get; set; }

    // Prosent (0-100) for progress bar.
    public int Percent { get; set; }
}
