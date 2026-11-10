using Microsoft.EntityFrameworkCore;
using OpenGarden.Server.Constants;
using OpenGarden.Server.Data;
using OpenGarden.Server.Dtos;
using OpenGarden.Server.Utils;

namespace OpenGarden.Server.Services;

public class HarvestService(AppDbContext db, ClientUpdater clientUpdater, EnergyService energyService)
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

        if (!energyService.TrySpend(player, GameConstants.HarvestCost)) //only in memory for now
            return ActionOutcome.NotEnoughEnergy;

        await using var transaction = await db.Database.BeginTransactionAsync(ct);

        var deleted = await db.Plants
            .Where(p => p.X == cellX && p.Y == cellY)
            .ExecuteDeleteAsync(ct);

        if (deleted == 0)
            return ActionOutcome.CellEmpty;

        await db.SaveChangesAsync(ct);
        await transaction.CommitAsync(ct);

        await clientUpdater.UpdateAllClients([new CellUpdateDto(cellX, cellY, null)]);
        return ActionOutcome.Success;
    }
}
