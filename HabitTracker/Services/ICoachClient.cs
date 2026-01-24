namespace HabitTracker.Services;

/// Abstraksjon slik at UI/CoachService ikke bryr seg om vi bruker ekte AI eller fallback.
public interface ICoachClient
{
    /// Returnerer true hvis ekte AI er aktiv.
    bool IsAiEnabled { get; }

    /// Sender et spørsmål til modellen og får tekst tilbake.
    Task<string> AskAsync(string systemPrompt, string userPrompt, CancellationToken ct = default);
}