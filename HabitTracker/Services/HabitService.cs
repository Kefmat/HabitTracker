using HabitTracker.Data;
using HabitTracker.Models;
using Microsoft.EntityFrameworkCore;

namespace HabitTracker.Services;

/// <summary>
/// HabitService er "forretningslogikken" for appen.
/// UI (Razor Pages) skal i hovedsak kalle denne, og ikke skrive DB-spørringer direkte.
/// Dette gjør appen enklere å teste/utvide (f.eks. integrasjoner/AI senere).
/// </summary>
public class HabitService
{
    private readonly AppDbContext _db;

    public HabitService(AppDbContext db)
    {
        _db = db;
    }

    // -----------------------
    // HABITS
    // -----------------------

    public Task<List<Habit>> GetHabitsAsync()
        => _db.Habits.OrderBy(h => h.Name).ToListAsync();

    public async Task AddHabitAsync(string name, int points = 10)
    {
        if (string.IsNullOrWhiteSpace(name)) return;

        _db.Habits.Add(new Habit
        {
            Name = name.Trim(),
            Points = Math.Max(0, points)
        });

        await _db.SaveChangesAsync();
    }

    public async Task DeleteHabitAsync(Guid habitId)
    {
        // Sletter completions først for å unngå "dangling" data.
        _db.Completions.RemoveRange(_db.Completions.Where(c => c.HabitId == habitId));

        var habit = await _db.Habits.FirstOrDefaultAsync(h => h.Id == habitId);
        if (habit is not null)
            _db.Habits.Remove(habit);

        await _db.SaveChangesAsync();
    }

    // -----------------------
    // COMPLETIONS
    // -----------------------

    public async Task<HashSet<Guid>> GetCompletedHabitIdsOnDateAsync(DateOnly date)
    {
        var ids = await _db.Completions
            .Where(c => c.Date == date)
            .Select(c => c.HabitId)
            .Distinct()
            .ToListAsync();

        return ids.ToHashSet();
    }

    public Task<bool> IsCompletedOnDateAsync(Guid habitId, DateOnly date)
        => _db.Completions.AnyAsync(c => c.HabitId == habitId && c.Date == date);

    public async Task ToggleCompleteOnDateAsync(Guid habitId, DateOnly date)
    {
        // Toggle: hvis den finnes -> fjern, ellers -> legg til.
        var existing = await _db.Completions
            .FirstOrDefaultAsync(c => c.HabitId == habitId && c.Date == date);

        if (existing is null)
            _db.Completions.Add(new Completion { HabitId = habitId, Date = date });
        else
            _db.Completions.Remove(existing);

        await _db.SaveChangesAsync();

        // Etter endring i completions sjekker vi om nye rewards skal låses opp.
        await EnsureRewardsUnlockedAsync();
    }

    // -----------------------
    // POINTS
    // -----------------------

    public async Task<int> TotalPointsOnDateAsync(DateOnly date)
    {
        var completedHabitIds = await _db.Completions
            .Where(c => c.Date == date)
            .Select(c => c.HabitId)
            .Distinct()
            .ToListAsync();

        return await _db.Habits
            .Where(h => completedHabitIds.Contains(h.Id))
            .SumAsync(h => h.Points);
    }

    public async Task<int> TotalPointsAllTimeAsync()
    {
        // All-time points = sum(poeng for habit) for hver completion.
        // Siden (HabitId, Date) er unikt, får vi ikke dobbelttelling samme dag.
        return await _db.Completions
            .Join(_db.Habits,
                c => c.HabitId,
                h => h.Id,
                (c, h) => h.Points)
            .SumAsync();
    }

    // -----------------------
    // STREAKS
    // -----------------------

    public async Task<int> GetCurrentStreakAsync(Guid habitId, DateOnly? upTo = null)
    {
        // "Streak" betyr her: sammenhengende dager med completion fram til i dag.
        // NB: Dette antar daglige vaner. Hvis du senere får ukentlig schedule,
        // må streak-logikken endres til å se på "forventede dager".
        var day = upTo ?? DateOnly.FromDateTime(DateTime.Now);

        var dates = await _db.Completions
            .Where(c => c.HabitId == habitId)
            .Select(c => c.Date)
            .OrderByDescending(d => d)
            .ToListAsync();

        if (dates.Count == 0) return 0;

        var set = dates.ToHashSet();
        var streak = 0;
        var cur = day;

        while (set.Contains(cur))
        {
            streak++;
            cur = cur.AddDays(-1);
        }

        return streak;
    }

    // -----------------------
    // REWARDS
    // -----------------------

    public Task<List<Reward>> GetRewardsAsync()
        => _db.Rewards.OrderBy(r => r.CostPoints).ToListAsync();

    public Task<HashSet<Guid>> GetUnlockedRewardIdsAsync()
        => _db.RewardUnlocks.Select(u => u.RewardId).ToHashSetAsync();

    public async Task AddRewardAsync(string name, int costPoints)
    {
        if (string.IsNullOrWhiteSpace(name)) return;

        _db.Rewards.Add(new Reward
        {
            Name = name.Trim(),
            CostPoints = Math.Max(0, costPoints)
        });

        await _db.SaveChangesAsync();

        // Hvis brukeren allerede har nok poeng, lås opp med en gang.
        await EnsureRewardsUnlockedAsync();
    }

    public async Task DeleteRewardAsync(Guid rewardId)
    {
        // Sletter unlock først (hvis den finnes) for å holde data ryddig.
        var unlock = await _db.RewardUnlocks.FirstOrDefaultAsync(u => u.RewardId == rewardId);
        if (unlock is not null) _db.RewardUnlocks.Remove(unlock);

        var reward = await _db.Rewards.FirstOrDefaultAsync(r => r.Id == rewardId);
        if (reward is not null) _db.Rewards.Remove(reward);

        await _db.SaveChangesAsync();
    }

    private async Task EnsureRewardsUnlockedAsync()
    {
        // Låser opp alle rewards der CostPoints <= all-time points.
        // Dette er "idempotent": vi legger bare til unlocks som ikke finnes.
        var total = await TotalPointsAllTimeAsync();
        var unlocked = await _db.RewardUnlocks.Select(u => u.RewardId).ToHashSetAsync();

        var newlyUnlocked = await _db.Rewards
            .Where(r => r.CostPoints <= total && !unlocked.Contains(r.Id))
            .ToListAsync();

        if (newlyUnlocked.Count == 0) return;

        foreach (var r in newlyUnlocked)
        {
            _db.RewardUnlocks.Add(new RewardUnlock
            {
                RewardId = r.Id,
                UnlockedAt = DateTime.UtcNow
            });
        }

        await _db.SaveChangesAsync();

        // TODO (senere): push-notifikasjon / toast i UI når ny reward låses opp.
    }

    public async Task<RewardProgress> GetRewardProgressAsync()
    {
    // Henter totalpoeng (all-time)
    var total = await TotalPointsAllTimeAsync();

    // Henter alle rewards sortert på kostnad
    var rewards = await _db.Rewards
        .OrderBy(r => r.CostPoints)
        .ToListAsync();

    // Henter hvilke rewards som allerede er låst opp
    var unlockedIds = await _db.RewardUnlocks
        .Select(u => u.RewardId)
        .ToHashSetAsync();

    // Finn neste reward som ikke er låst opp
    var next = rewards.FirstOrDefault(r => !unlockedIds.Contains(r.Id));

    if (next is null)
    {
        // Ingen neste reward (enten ingen rewards eller alle er unlocked)
        return new RewardProgress
        {
            TotalPoints = total,
            NextReward = null,
            PointsToNext = 0,
            Percent = 100
        };
    }

    var missing = Math.Max(0, next.CostPoints - total);

    // Prosent: total / cost * 100, men clamp til 0-100.
    var percent = next.CostPoints <= 0
        ? 100
        : (int)Math.Clamp((double)total / next.CostPoints * 100.0, 0, 100);

    return new RewardProgress
    {
        TotalPoints = total,
        NextReward = next,
        PointsToNext = missing,
        Percent = percent
    };

    // TODO (senere): Hvis du får "claim"-mekanikk, kan progress baseres på "available points".
    }
}
