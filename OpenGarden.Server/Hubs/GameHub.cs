using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using OpenGarden.Server.Data;
using OpenGarden.Server.Dtos;
using OpenGarden.Server.Models;
using OpenGarden.Server.Utils;

namespace OpenGarden.Server.Hubs;

public class GameHub : Hub
{
    private readonly AppDbContext _dbContext;

    public GameHub(AppDbContext dbContext)
    {
        _dbContext = dbContext;
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
        if (!await _dbContext.PlantTypes.Select(p => p.Id).ContainsAsync(plantTypeId))
            return new PlantResultDto(ActionOutcome.UnknownPlantType);

        await _dbContext.Plants.AddAsync(new Plant()
        {
            PlantTypeId = plantTypeId,
            X = cellX,
            Y = cellY
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
