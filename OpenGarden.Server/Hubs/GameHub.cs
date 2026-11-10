using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using NodaTime;
using OpenGarden.Server.Constants;
using OpenGarden.Server.Services;
using OpenGarden.Server.Data;
using OpenGarden.Server.Dtos;
using OpenGarden.Server.Utils;
using Microsoft.AspNetCore.Authorization;

namespace OpenGarden.Server.Hubs;

[Authorize]
public class GameHub(
    AppDbContext _dbContext,
    IClock _clock,
    ClientUpdater _clientUpdater,
    PlantingService _plantingService,
    EnergyService _energyService
) : Hub
{
    public override async Task OnConnectedAsync()
    {
        var userId = Context.UserIdentifier;

        Console.WriteLine($"Player connected: {userId}");

        await base.OnConnectedAsync();
    }

    public Task<GameConfigDto> GetGameConfig()
    {
        return Task.FromResult(new GameConfigDto(
            GameConstants.MapSize,
            GameConstants.ChunkSize,
            GameConstants.HarvestCost,
            GameConstants.PlantCost,
            GameConstants.GrowthTimeSeconds,
            _clock.GetCurrentInstant().ToUnixTimeMilliseconds()));
    }

    public async Task<LoadResultDto> LoadNearbyObjects(int chunkX, int chunkY)
    {
        if (!GridMath.IsValidChunk(chunkX, chunkY))
            throw new HubException("Chunk is outside the map.");

        var cellBounds = GridMath.GetCellBoundsAroundChunk(new ChunkCoord(chunkX, chunkY));

        var plants = await _dbContext.Plants
            .AsNoTracking()
            .Where(p =>
                p.X >= cellBounds.MinX &&
                p.X <= cellBounds.MaxX &&
                p.Y >= cellBounds.MinY &&
                p.Y <= cellBounds.MaxY)
            .Select(p => new PlantDto(p.X, p.Y, p.PlantTypeId, p.RipeAt.ToDateTimeUtc()))
            .ToListAsync();

        return new LoadResultDto(plants);
    }

    public async Task<PlantResultDto> Plant(short plantTypeId, int cellX, int cellY)
    {
        return new PlantResultDto(await _plantingService
                .TryPlantAsync(plantTypeId, cellX, cellY, GetPlayerId(), Context.ConnectionAborted));
    }

    public async Task<HarvestResultDto> Harvest(int cellX, int cellY)
    {
        if (!GridMath.IsInWorldBounds(cellX, cellY))
            return new HarvestResultDto(ActionOutcome.OutOfBounds);

        var deleted = await _dbContext.Plants
            .Where(p => p.X == cellX && p.Y == cellY)
            .ExecuteDeleteAsync();

        if (deleted == 0)
            return new HarvestResultDto(ActionOutcome.CellEmpty);

        await _clientUpdater.UpdateAllClients([new CellUpdateDto(cellX, cellY, null)]);

        return new HarvestResultDto(ActionOutcome.Success);
    }

    private string GetPlayerId() => Context.UserIdentifier!;
}
