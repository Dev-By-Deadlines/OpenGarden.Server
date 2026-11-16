using NodaTime;
using OpenGarden.Server.Constants;
using OpenGarden.Server.Models;

namespace OpenGarden.Server.Services;

public class EnergyService(IClock clock)
{
    public bool TrySpend(Player player, int cost)
    {
        var now = clock.GetCurrentInstant();
        var current = GetCurrent(player);

        if (current < cost) return false;

        player.Energy = current - cost;
        player.EnergyUpdatedAt = now;
        return true;
    }

    public int GetCurrent(Player player)
    {
        var max = player.User.IsGuest ? GameConstants.GuestMaxEnergy : GameConstants.MaxEnergy;
        var regenSeconds = player.User.IsGuest ? GameConstants.GuestEnergyRegenSeconds : GameConstants.EnergyRegenSeconds;

        var now = clock.GetCurrentInstant();
        var secondsPassed = (now - player.EnergyUpdatedAt).TotalSeconds;
        var regenerated = (int)(secondsPassed / regenSeconds);
        return Math.Min(max, player.Energy + regenerated);
    }
}
