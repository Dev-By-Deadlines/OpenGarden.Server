using Microsoft.EntityFrameworkCore;
using NodaTime;
using Npgsql;
using OpenGarden.Server.Constants;
using OpenGarden.Server.Data;
using OpenGarden.Server.Dtos;
using OpenGarden.Server.Models;
using OpenGarden.Server.Utils;

namespace OpenGarden.Server.Services;

public class PlantingService(AppDbContext db, IClock clock, ClientUpdater clientUpdater)
{
    public async Task<ActionOutcome> TryPlantAsync(short plantTypeId, int cellX, int cellY, CancellationToken ct = default)
    {
        if (!GridMath.IsInWorldBounds(cellX, cellY)) return ActionOutcome.OutOfBounds;
        if (!await db.PlantTypes.AnyAsync(t => t.Id == plantTypeId, ct)) return ActionOutcome.UnknownPlantType;

        var ripeAt = clock.GetCurrentInstant() + Duration.FromSeconds(GameConstants.GrowthTimeSeconds);
        db.Plants.Add(new Plant { PlantTypeId = plantTypeId, X = cellX, Y = cellY, RipeAt = ripeAt });

        try { await db.SaveChangesAsync(ct); }
        catch (DbUpdateException ex) when (ex.InnerException is PostgresException { SqlState: PostgresErrorCodes.UniqueViolation })
        {
            return ActionOutcome.CellOccupied;
        }

        var plant = new PlantDto(cellX, cellY, plantTypeId, ripeAt.ToDateTimeUtc());
        await clientUpdater.UpdateAllClients([new CellUpdateDto(cellX, cellY, plant)]);
        return ActionOutcome.Success;
    }
}
