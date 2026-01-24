using HabitTracker.Models;

namespace HabitTracker.Services;

/// CoachService gir råd/motivasjon basert på data i appen.
/// Den gir alltid "regelbaserte" tips, og kan i tillegg bruke ekte AI når tilgjengelig.
public class CoachService
{
    private readonly HabitService _habitService;
    private readonly ICoachClient _coachClient;

    public bool IsAiEnabled => _coachClient.IsAiEnabled;

    public CoachService(HabitService habitService, ICoachClient coachClient)
    {
        _habitService = habitService;
        _coachClient = coachClient;
    }

    public async Task<CoachResult> GetCoachAsync(string? userQuestion = null)
    {
        var result = new CoachResult();

        var habits = await _habitService.GetHabitsAsync();
        var weekly = await _habitService.GetWeeklyStatsAsync();

        // --- 1) Automatisk råd (regelbasert) ---
        if (habits.Count == 0)
        {
            result.Messages.Add(new CoachMessage
            {
                Title = "Start enkelt",
                Body = "Legg til 1 vane du faktisk klarer å gjøre hver dag i 2 minutter (f.eks. gå 5 min, lese 1 side, drikke vann).",
                Tone = CoachTone.Tip
            });

            result.Messages.Add(new CoachMessage
            {
                Title = "Første mål",
                Body = "Målet er ikke perfeksjon – det er å møte opp. Når det er lett, kan du øke gradvis.",
                Tone = CoachTone.Positive
            });
        }
        else
        {
            var totalCompletions = weekly.TotalCompletions;
            var totalPoints = weekly.TotalPoints;

            if (totalCompletions == 0)
            {
                result.Messages.Add(new CoachMessage
                {
                    Title = "Ingen fullføringer siste 7 dager",
                    Body = "Helt vanlig. Velg én vane og gjør den superenkel: '2 minutter' eller 'bare start'.",
                    Tone = CoachTone.Warning
                });

                result.Messages.Add(new CoachMessage
                {
                    Title = "Plan for i dag",
                    Body = "Sett et minimumsmål du klarer selv på en dårlig dag. Når minimum er gjort → bonus hvis du vil.",
                    Tone = CoachTone.Tip
                });
            }
            else if (totalCompletions < 5)
            {
                result.Messages.Add(new CoachMessage
                {
                    Title = "Du er i gang",
                    Body = $"Du har {totalCompletions} fullføringer siste 7 dager. Prøv én liten fullføring i dag for å bygge rytme.",
                    Tone = CoachTone.Positive
                });
            }
            else
            {
                result.Messages.Add(new CoachMessage
                {
                    Title = "Sterk uke",
                    Body = $"Du har {totalCompletions} fullføringer og {totalPoints} poeng siste 7 dager. Hold det lett nok til å være stabilt.",
                    Tone = CoachTone.Positive
                });
            }

            if (!string.IsNullOrWhiteSpace(weekly.TopHabitName))
            {
                result.Messages.Add(new CoachMessage
                {
                    Title = "Toppvane denne uka",
                    Body = $"{weekly.TopHabitName} er mest fullført. Knyt den til en trigger: 'Etter X gjør jeg Y'.",
                    Tone = CoachTone.Tip
                });
            }

            result.Messages.Add(new CoachMessage
            {
                Title = "Motivasjon",
                Body = PickMotivationLine(),
                Tone = CoachTone.Neutral
            });
        }

        // --- 2) Spørsmål/svar (AI hvis mulig, ellers fallback) ---
        if (!string.IsNullOrWhiteSpace(userQuestion))
        {
            if (_coachClient.IsAiEnabled)
            {
                var system = BuildSystemPrompt();
                var user = BuildUserPrompt(userQuestion, habits, weekly);

                try
                {
                    result.AnswerToQuestion = await _coachClient.AskAsync(system, user);
                    if (string.IsNullOrWhiteSpace(result.AnswerToQuestion))
                    {
                        // Hvis AI returnerte tomt, bruk fallback.
                        result.AnswerToQuestion = AnswerQuestionFallback(userQuestion);
                    }
                }
                catch
                {
                    // Hvis AI feiler (nett/API/limit), bruk fallback.
                    result.AnswerToQuestion = AnswerQuestionFallback(userQuestion);
                }
            }
            else
            {
                result.AnswerToQuestion = AnswerQuestionFallback(userQuestion);
            }
        }

        return result;
    }

    private static string BuildSystemPrompt()
    {
        return
            "Du er en vennlig coach for vaner. Svar kort, konkret og på norsk. " +
            "Gi 1–3 konkrete forslag. Unngå lange foredrag.";
    }

    private static string BuildUserPrompt(string question, List<Models.Habit> habits, Models.WeeklyStats weekly)
    {
        var habitNames = habits.Count == 0
            ? "Ingen vaner ennå."
            : string.Join(", ", habits.Select(h => $"{h.Name} (+{h.Points})"));

        return
            $"Brukerens spørsmål: {question}\n\n" +
            $"Vaner: {habitNames}\n" +
            $"Siste 7 dager: {weekly.TotalCompletions} fullføringer, {weekly.TotalPoints} poeng.\n" +
            (string.IsNullOrWhiteSpace(weekly.TopHabitName)
                ? ""
                : $"Toppvane: {weekly.TopHabitName} ({weekly.TopHabitCompletions} fullføringer).\n") +
            "\nSvar nå med konkrete steg.";
    }

    private static string PickMotivationLine()
    {
        var lines = new[]
        {
            "Små steg hver dag slår store skippertak.",
            "Gjør det lett nok til at du klarer det på en dårlig dag.",
            "Fokuser på å møte opp – intensitet kan komme senere.",
            "Hvis du kan gjøre det på 2 minutter, kan du alltid starte.",
            "Målet er konsistens, ikke perfeksjon."
        };

        var idx = Random.Shared.Next(0, lines.Length);
        return lines[idx];
    }

    private static string AnswerQuestionFallback(string question)
    {
        var q = question.ToLowerInvariant();

        if (q.Contains("motivasjon") || q.Contains("motivert") || q.Contains("orker"))
            return "Prøv 2-minutters regelen: gjør det i 2 minutter. Start er målet. Når du først har startet, kan du stoppe eller fortsette.";

        if (q.Contains("starte") || q.Contains("begynne") || q.Contains("komme i gang"))
            return "Velg én vane. Gjør den så liten at den er vanskelig å feile (1 side, 5 squats, 2 min rydding). Gjenta i 7 dager.";

        if (q.Contains("rutine") || q.Contains("konsistens"))
            return "Knytt vanen til en trigger: 'Etter X, gjør jeg Y'. Eksempel: Etter tannpuss → 10 squats. Etter kaffe → 1 side lesing.";

        if (q.Contains("belønning") || q.Contains("reward"))
            return "Belønning fungerer best når den er tett på handlingen: små belønninger etter fullføring og større rewards etter uke/streak.";

        return "Si hva du prøver å få til (trening/lesing/kosthold) og hva som stopper deg, så gjør vi planen enklere.";
    }
}
