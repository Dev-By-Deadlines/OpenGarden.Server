using Microsoft.EntityFrameworkCore;
using OpenGarden.Server.Data;
using OpenGarden.Server.Dtos;

namespace OpenGarden.Server.Services;

public class PlayerStateService(AppDbContext db, EnergyService energyService)
{
    public async Task<PlayerStateDto?> GetAsync(string playerId, CancellationToken ct = default)
    {
        var player = await db.Players
            .AsNoTracking()
            .Include(p => p.User)
            .FirstOrDefaultAsync(p => p.Id == playerId, ct);
        if (player is null) return null;

        var energy = energyService.GetCurrent(player);

        var seeds = await db.SeedInventories
            .AsNoTracking()
            .Where(s => s.PlayerId == playerId)
            .OrderBy(s => s.PlantTypeId)
            .Select(s => new SeedStackDto(s.PlantTypeId, s.Quantity))
            .ToListAsync(ct);

        return new PlayerStateDto(energy.Current, energy.Max, energy.RegenSeconds, seeds);
    }
}
