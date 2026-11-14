using Microsoft.EntityFrameworkCore;
using NodaTime;
using Npgsql;
using OpenGarden.Server.Constants;
using OpenGarden.Server.Data;
using OpenGarden.Server.Dtos;
using OpenGarden.Server.Models;
using OpenGarden.Server.Utils;

namespace OpenGarden.Server.Services;

public class PlantingService(AppDbContext db, IClock clock, ClientUpdater clientUpdater, EnergyService energyService)
{
    public async Task<ActionOutcome> TryPlantAsync(short plantTypeId, int cellX, int cellY, string? playerId = null, CancellationToken ct = default)
    {
        if (!GridMath.IsInWorldBounds(cellX, cellY)) return ActionOutcome.OutOfBounds;
        if (!await db.PlantTypes.AnyAsync(t => t.Id == plantTypeId, ct)) return ActionOutcome.UnknownPlantType;

        if (playerId is not null)
        {
            var player = await db.Players
                .Include(p => p.User)
                .FirstOrDefaultAsync(p => p.Id == playerId, ct);
            if (player is null) return ActionOutcome.UnknownPlayer;

            // change the in-memory player only, nothing is saved yet
            if (!energyService.TrySpend(player, GameConstants.PlantCost))
                return ActionOutcome.NotEnoughEnergy;
        }

        await using var transaction = await db.Database.BeginTransactionAsync(ct);

        if (playerId is not null)
        {
            // classic stock = 1 race condition
            var spent = await db.SeedInventories
                .Where(s => s.PlayerId == playerId && s.PlantTypeId == plantTypeId && s.Quantity >= 1)
                .ExecuteUpdateAsync(s =>
                        s.SetProperty(x => x.Quantity, x => x.Quantity - 1), ct);

            if (spent == 0) return ActionOutcome.NoSeeds;
        }

        var ripeAt = clock.GetCurrentInstant() + Duration.FromSeconds(GameConstants.GrowthTimeSeconds);
        db.Plants.Add(new Plant { PlantTypeId = plantTypeId, X = cellX, Y = cellY, RipeAt = ripeAt });

        try { await db.SaveChangesAsync(ct); }
        catch (DbUpdateException ex) when (ex.InnerException is PostgresException { SqlState: PostgresErrorCodes.UniqueViolation })
        {
            return ActionOutcome.CellOccupied;
        }

        await transaction.CommitAsync(ct);

        var plant = new PlantDto(cellX, cellY, plantTypeId, ripeAt.ToDateTimeUtc());
        await clientUpdater.UpdateAllClients([new CellUpdateDto(cellX, cellY, plant)]);
        return ActionOutcome.Success;
    }
}
