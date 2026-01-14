namespace HabitTracker.Models;

public class RewardUnlock
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid RewardId { get; set; }
    public Reward? Reward { get; set; }

    // Når belønningen ble låst opp (audit/historikk).
    public DateTime UnlockedAt { get; set; } = DateTime.UtcNow;

    // TODO (senere): "claimed" / "redeemed" hvis du vil markere at bruker faktisk tok belønningen.
}
