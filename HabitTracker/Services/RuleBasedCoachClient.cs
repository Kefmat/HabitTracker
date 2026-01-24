namespace HabitTracker.Services;

/// Fallback hvis API-key mangler eller AI feiler.
public class RuleBasedCoachClient : ICoachClient
{
    public bool IsAiEnabled => false;

    public Task<string> AskAsync(string systemPrompt, string userPrompt, CancellationToken ct = default)
    {
        // Returnerer tom streng her; CoachService håndterer fallback-svar.
        return Task.FromResult("");
    }
}
