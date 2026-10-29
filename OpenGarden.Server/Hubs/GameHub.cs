using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using OpenGarden.Server.Data;
using OpenGarden.Server.Dtos;
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
}
