using Microsoft.EntityFrameworkCore;
using NodaTime;
using OpenGarden.Server.Constants;
using OpenGarden.Server.Data;
using OpenGarden.Server.Dtos;
using OpenGarden.Server.Models;
using OpenGarden.Server.Utils;

namespace OpenGarden.Server.Services;

public class HarvestService(AppDbContext db, IClock clock, ClientUpdater clientUpdater, EnergyService energyService)
{
    public async Task<ActionOutcome> TryHarvestAsync(int cellX, int cellY, string playerId, CancellationToken ct = default)
    {
        if (!GridMath.IsInWorldBounds(cellX, cellY))
            return ActionOutcome.OutOfBounds;

        var player = await db.Players
            .Include(p => p.User)
            .FirstOrDefaultAsync(p => p.Id == playerId, ct);
        if (player is null)
            return ActionOutcome.UnknownPlayer;

        if (!energyService.TrySpend(player, GameConstants.HarvestCost))   // in memory only
            return ActionOutcome.NotEnoughEnergy;

        await using var transaction = await db.Database.BeginTransactionAsync(ct);

        var plant = await db.Plants
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.X == cellX && p.Y == cellY, ct);
        if (plant is null)
            return ActionOutcome.CellEmpty;

        // delete exactly the plant we read: if it changed in between, nothing is deleted
        var deleted = await db.Plants
            .Where(p => p.X == cellX && p.Y == cellY && p.PlantTypeId == plant.PlantTypeId)
            .ExecuteDeleteAsync(ct);
        if (deleted == 0)
            return ActionOutcome.CellEmpty;

        var seeds = GetHarvestedSeedCount(plant);

        // again, make the SQL like "quantity = quantity + x" instead of "quantity = y" which could lead into race conditions.
        await db.SeedInventories
            .Where(s => s.PlayerId == playerId && s.PlantTypeId == plant.PlantTypeId)
            .ExecuteUpdateAsync(s =>
                    s.SetProperty(x => x.Quantity, x => x.Quantity + seeds), ct);

        await db.SaveChangesAsync(ct);   // saves the energy change
        await transaction.CommitAsync(ct);

        await clientUpdater.UpdateAllClients([new CellUpdateDto(cellX, cellY, null)]);
        return ActionOutcome.Success;
    }

    private int GetHarvestedSeedCount(Plant plant)
    {
        var isRipe = plant.RipeAt <= clock.GetCurrentInstant();

        return isRipe
            ? Random.Shared.Next(GameConstants.RipeSeedsMin, GameConstants.RipeSeedsMax + 1) : 1;
    }
}
