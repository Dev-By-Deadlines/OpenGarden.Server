using NodaTime;
using OpenGarden.Server.Constants;
using OpenGarden.Server.Models;

namespace OpenGarden.Server.Services;

public class EnergyService(IClock clock)
{
    public bool TrySpend(Player player, int cost, bool isGuest)
    {
        var max = isGuest ? GameConstants.GuestMaxEnergy : GameConstants.MaxEnergy;
        var regenSeconds = isGuest ? GameConstants.GuestEnergyRegenSeconds : GameConstants.EnergyRegenSeconds;

        var now = clock.GetCurrentInstant();
        var secondsPassed = (now - player.EnergyUpdatedAt).TotalSeconds;
        var regenerated = (int)(secondsPassed / regenSeconds);
        var current = Math.Min(max, player.Energy + regenerated);

        if (current < cost) return false;

        player.Energy = current - cost;
        player.EnergyUpdatedAt = now;
        return true;
    }
}
