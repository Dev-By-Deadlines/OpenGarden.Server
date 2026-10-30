using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using NodaTime;
using Npgsql;
using OpenGarden.Server.Constants;
using OpenGarden.Server.Data;
using OpenGarden.Server.Dtos;
using OpenGarden.Server.Models;
using OpenGarden.Server.Utils;

namespace OpenGarden.Server.Hubs;

public class GameHub : Hub
{
    private readonly AppDbContext _dbContext;
    private readonly IClock _clock;

    public GameHub(AppDbContext dbContext, IClock clock)
    {
        _dbContext = dbContext;
        _clock = clock;
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
        if (!GridMath.IsInWorldBounds(cellX, cellY))
            return new PlantResultDto(ActionOutcome.OutOfBounds);

        if (!await _dbContext.PlantTypes.AnyAsync(t => t.Id == plantTypeId))
            return new PlantResultDto(ActionOutcome.UnknownPlantType);

        _dbContext.Plants.Add(new Plant()
        {
            PlantTypeId = plantTypeId,
            X = cellX,
            Y = cellY,
            RipeAt = _clock.GetCurrentInstant() + Duration.FromSeconds(GameConstants.GrowthTimeSeconds)
        });

        try
        {
            await _dbContext.SaveChangesAsync();
        }
        catch (DbUpdateException ex) when (ex.InnerException is PostgresException { SqlState: PostgresErrorCodes.UniqueViolation })
        {
            return new PlantResultDto(ActionOutcome.CellOccupied);
        }

        return new PlantResultDto(ActionOutcome.Success);
    }
}
