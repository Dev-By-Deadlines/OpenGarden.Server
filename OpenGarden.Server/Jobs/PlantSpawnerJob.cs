using OpenGarden.Server.Constants;
using Microsoft.EntityFrameworkCore;
using OpenGarden.Server.Data;
using OpenGarden.Server.Utils;
using OpenGarden.Server.Services;
using OpenGarden.Server.Dtos;
namespace OpenGarden.Server.Jobs;

public class PlantSpawnerJob(IServiceScopeFactory scopeFactory, ILogger<PlantSpawnerJob> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(TimeSpan.FromSeconds(GameConstants.SpawnIntervalSeconds));

        while (await timer.WaitForNextTickAsync(stoppingToken))
        {
            try
            {
                await SpawnOnePlantAsync(stoppingToken);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                logger.LogError(ex, "Plant spawn tick failed");
            }
        }
    }

    private async Task SpawnOnePlantAsync(CancellationToken ct)
    {
        using var scope = scopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var plantingService = scope.ServiceProvider.GetRequiredService<PlantingService>();

        var chunk = await GetRandomEmptyChunk(db, ct);
        if (chunk is null) return;

        var cell = GridMath.GetRandomCellInChunk(chunk.Value);
        var typeId = await GetRandomPlantType(db, ct);

        var outcome = await plantingService.TryPlantAsync(typeId, cell.X, cell.Y, ct);
        if (outcome != ActionOutcome.Success)
            logger.LogInformation("Spawn at {Cell} skipped: {Outcome}", cell, outcome);
    }

    private async Task<short> GetRandomPlantType(AppDbContext dbContext, CancellationToken ct)
    {
        var allTypes = await dbContext.PlantTypes.Select(pt => pt.Id).ToListAsync(ct);
        return allTypes[Random.Shared.Next(0, allTypes.Count)];
    }

    private async Task<ChunkCoord?> GetRandomEmptyChunk(AppDbContext dbContext, CancellationToken ct)
    {
        var chunkSize = GameConstants.ChunkSize;

        var occupied = (await dbContext.Plants
                .Select(p => new ChunkCoord(p.X / chunkSize, p.Y / chunkSize))
                .Distinct()
                .ToListAsync(ct))
            .ToHashSet();

        var empty = new List<ChunkCoord>();
        for (int cx = 0; cx < GameConstants.ChunksPerSide; cx++)
            for (int cy = 0; cy < GameConstants.ChunksPerSide; cy++)
            {
                var chunk = new ChunkCoord(cx, cy);
                if (!occupied.Contains(chunk))
                    empty.Add(chunk);
            }

        return empty.Count == 0 ? null : empty[Random.Shared.Next(empty.Count)];
    }
}
