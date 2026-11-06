using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using NodaTime;
using OpenGarden.Server.Constants;
using OpenGarden.Server.Services;
using OpenGarden.Server.Data;
using OpenGarden.Server.Dtos;
using OpenGarden.Server.Utils;

namespace OpenGarden.Server.Hubs;

public class GameHub : Hub
{
    private readonly AppDbContext _dbContext;
    private readonly ClientUpdater _clientUpdater;
    private readonly PlantingService _plantingService;
    private readonly IClock _clock;

    public GameHub(AppDbContext dbContext, IClock clock, ClientUpdater clientUpdater, PlantingService plantingService)
    {
        _dbContext = dbContext;
        _clock = clock;
        _clientUpdater = clientUpdater;
        _plantingService = plantingService;
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
        return new PlantResultDto(await _plantingService.TryPlantAsync(plantTypeId, cellX, cellY));
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
}
