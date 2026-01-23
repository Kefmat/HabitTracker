using HabitTracker.Models;

namespace HabitTracker.Services;

/// CoachService gir råd/motivasjon basert på data i appen.
/// Nå er den regelbasert (ingen ekstern AI).
///
/// TODO (AI): Bytt ut regelmotoren med ekte AI (OpenAI / Azure OpenAI)
/// ved å sende en "prompt" med ukesstatistikk, vaner og mål.

public class CoachService
{
    private readonly HabitService _habitService;

    public CoachService(HabitService habitService)
    {
        _habitService = habitService;
    }

    public async Task<CoachResult> GetCoachAsync(string? userQuestion = null)
    {
        var result = new CoachResult();

        // Hent grunn-data vi kan gi råd ut fra.
        var habits = await _habitService.GetHabitsAsync();
        var weekly = await _habitService.GetWeeklyStatsAsync();

        // --- 1) Automatisk råd (basert på status) ---
        if (habits.Count == 0)
        {
            result.Messages.Add(new CoachMessage
            {
                Title = "Start enkelt",
                Body = "Legg til 1 vane du faktisk klarer å gjøre hver dag i 2 minutter (f.eks. gå 5 minutter, lese 1 side, drikke vann).",
                Tone = CoachTone.Tip
            });

            result.Messages.Add(new CoachMessage
            {
                Title = "Første mål",
                Body = "Målet er ikke perfeksjon – det er å møte opp. Når det er lett, kan du øke gradvis.",
                Tone = CoachTone.Positive
            });

            // Hvis ingen vaner, svarer vi fortsatt på spørsmål under.
        }
        else
        {
            // Enkel vurdering av “aktivitet” siste 7 dager.
            var totalCompletions = weekly.TotalCompletions;
            var totalPoints = weekly.TotalPoints;

            if (totalCompletions == 0)
            {
                result.Messages.Add(new CoachMessage
                {
                    Title = "Ingen fullføringer siste 7 dager",
                    Body = "Det er helt vanlig å starte tregt. Velg én vane og gjør den superenkel: '2 minutter' eller 'bare start'.",
                    Tone = CoachTone.Warning
                });

                result.Messages.Add(new CoachMessage
                {
                    Title = "Plan for i dag",
                    Body = "Sett et minimumsmål du kan klare selv på en dårlig dag. Når du har gjort minimum → bonus hvis du vil.",
                    Tone = CoachTone.Tip
                });
            }
            else if (totalCompletions < 5)
            {
                result.Messages.Add(new CoachMessage
                {
                    Title = "Du er i gang",
                    Body = $"Du har {totalCompletions} fullføringer siste 7 dager. Prøv å få til én liten fullføring i dag for å bygge rytme.",
                    Tone = CoachTone.Positive
                });
            }
            else
            {
                result.Messages.Add(new CoachMessage
                {
                    Title = "Sterk uke",
                    Body = $"Du har {totalCompletions} fullføringer og {totalPoints} poeng siste 7 dager. Fortsett med samme tempo – og hold det lett nok til å være stabilt.",
                    Tone = CoachTone.Positive
                });
            }

            // “Beste vane”-feedback (fra WeeklyStats).
            if (!string.IsNullOrWhiteSpace(weekly.TopHabitName))
            {
                result.Messages.Add(new CoachMessage
                {
                    Title = "Din toppvane denne uka",
                    Body = $"{weekly.TopHabitName} er mest fullført. Hvis du vil ha mer momentum: bygg en 'trigger' (f.eks. etter kaffe / etter tannpuss).",
                    Tone = CoachTone.Tip
                });
            }

            // Generell motivasjon
            result.Messages.Add(new CoachMessage
            {
                Title = "Motivasjon",
                Body = PickMotivationLine(),
                Tone = CoachTone.Neutral
            });
        }

        // --- 2) Svar på brukerens spørsmål (regelbasert Q&A) ---
        if (!string.IsNullOrWhiteSpace(userQuestion))
        {
            result.AnswerToQuestion = AnswerQuestion(userQuestion.Trim());
        }

        return result;
    }

    private static string PickMotivationLine()
    {
        // Enkle “rotation”-linjer. Senere kan dette være AI-generert.
        var lines = new[]
        {
            "Små steg hver dag slår store skippertak.",
            "Gjør det lett nok til at du klarer det på en dårlig dag.",
            "Fokuser på å møte opp – intensitet kan komme senere.",
            "Hvis du kan gjøre det på 2 minutter, kan du alltid starte.",
            "Målet er konsistens, ikke perfeksjon."
        };

        // Litt random uten å være “for fancy”.
        var idx = Random.Shared.Next(0, lines.Length);
        return lines[idx];
    }

    private static string AnswerQuestion(string question)
    {
        var q = question.ToLowerInvariant();

        // Super enkel “intent”-matching. Dette er nok for MVP.
        if (q.Contains("motivasjon") || q.Contains("motivert") || q.Contains("orker"))
        {
            return "Prøv 2-minutters regelen: gjør det i 2 minutter. Når du først har startet, kan du stoppe eller fortsette. Start er målet.";
        }

        if (q.Contains("starte") || q.Contains("begynne") || q.Contains("komme i gang"))
        {
            return "Velg én vane. Gjør den så liten at den er vanskelig å feile (f.eks. 1 side, 5 knebøy, 2 minutter rydding). Gjenta i 7 dager.";
        }

        if (q.Contains("rutine") || q.Contains("vaner") || q.Contains("konsistens"))
        {
            return "Knytt vanen til en trigger: 'Etter X, gjør jeg Y'. Eksempel: Etter tannpuss → 10 squats. Etter kaffe → skrive 1 setning.";
        }

        if (q.Contains("belønning") || q.Contains("reward"))
        {
            return "Belønning fungerer best når den er tett på handlingen. Vurder små belønninger etter fullføring, og større rewards etter en uke/streak.";
        }

        // Default-svar
        return "Fortell meg hva du prøver å få til (f.eks. trening, lesing, kosthold), og hva som stopper deg – så kan vi gjøre planen enklere.";
    }
}
