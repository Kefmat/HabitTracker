namespace HabitTracker.Models;

/// En coach-melding som kan vises i UI.
public class CoachMessage
{
    public string Title { get; set; } = "";
    public string Body { get; set; } = "";
    public CoachTone Tone { get; set; } = CoachTone.Neutral;
}

/// Enkel "tone" for å style meldinger senere (f.eks. badges/farger).
public enum CoachTone
{
    Neutral,
    Positive,
    Warning,
    Tip
}


/// Resultat fra coach: både automatisk råd og svar på spørsmål.
public class CoachResult
{
    public List<CoachMessage> Messages { get; set; } = new();
    public string? AnswerToQuestion { get; set; }
}
