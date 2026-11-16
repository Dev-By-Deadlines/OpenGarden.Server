using NodaTime;
using OpenGarden.Server.Constants;
using OpenGarden.Server.Models;

namespace OpenGarden.Server.Services;

public class EnergyService(IClock clock)
{
    public bool TrySpend(Player player, int cost)
    {
        var status = GetCurrent(player);
        if (status.Current < cost) return false;

        player.Energy = status.Current - cost;
        player.EnergyUpdatedAt = clock.GetCurrentInstant();
        return true;
    }

    public record EnergyStatus(int Current, int Max, int RegenSeconds);
    public EnergyStatus GetCurrent(Player player)
    {
        var max = player.User.IsGuest ? GameConstants.GuestMaxEnergy : GameConstants.MaxEnergy;
        var regenSeconds = player.User.IsGuest ? GameConstants.GuestEnergyRegenSeconds : GameConstants.EnergyRegenSeconds;

        var secondsPassed = (clock.GetCurrentInstant() - player.EnergyUpdatedAt).TotalSeconds;
        var regenerated = (int)(secondsPassed / regenSeconds);

        return new EnergyStatus(Math.Min(max, player.Energy + regenerated), max, regenSeconds);
    }
}
