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
    PlantingService _plantingService,
    HarvestService _harvestService,
    PlayerStateService _playerStateService
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

    public async Task<PlayerStateDto?> GetPlayerState()
    {
        return await _playerStateService.GetAsync(GetPlayerId(), Context.ConnectionAborted);
    }

    public async Task<LoadResultDto> LoadNearbyObjects(int chunkX, int chunkY)
    {
        if (!GridMath.IsValidChunk(chunkX, chunkY))
            throw new HubException("Chunk is outside the map.");

        var focus = new ChunkCoord(chunkX, chunkY);
        await SubscribeToChunksAsync(focus);

        var cellBounds = GridMath.GetCellBoundsAroundChunk(focus);

        var plants = await _dbContext.Plants
            .AsNoTracking()
            .Where(p =>
                p.X >= cellBounds.MinX &&
                p.X <= cellBounds.MaxX &&
                p.Y >= cellBounds.MinY &&
                p.Y <= cellBounds.MaxY)
            .Select(p => new PlantDto(p.X, p.Y, p.PlantTypeId, p.RipeAt.ToDateTimeUtc(), p.PlantedBy!.UserName))
            .ToListAsync();

        return new LoadResultDto(plants);
    }

    public async Task<PlantResultDto> Plant(short plantTypeId, int cellX, int cellY)
    {
        var outcome = await _plantingService.TryPlantAsync(plantTypeId, cellX, cellY, GetPlayerId(), Context.ConnectionAborted);
        var state = await _playerStateService.GetAsync(GetPlayerId(), Context.ConnectionAborted);
        return new PlantResultDto(outcome, state);
    }

    public async Task<HarvestResultDto> Harvest(int cellX, int cellY)
    {
        var result = await _harvestService.TryHarvestAsync(
            cellX, cellY, GetPlayerId(), Context.ConnectionAborted);

        var state = await _playerStateService.GetAsync(
            GetPlayerId(), Context.ConnectionAborted);

        return new HarvestResultDto(
            result.Outcome,
            result.PlantTypeId,
            result.SeedsGained,
            state);
    }

    private async Task SubscribeToChunksAsync(ChunkCoord focus)
    {
        var wanted = GridMath.GetChunksAround(focus).ToHashSet();
        var current = Context.Items.TryGetValue("chunks", out var stored)
            ? (HashSet<ChunkCoord>)stored!
            : new HashSet<ChunkCoord>();

        foreach (var chunk in current.Except(wanted))
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, GridMath.GetGroupName(chunk));

        foreach (var chunk in wanted.Except(current))
            await Groups.AddToGroupAsync(Context.ConnectionId, GridMath.GetGroupName(chunk));

        Context.Items["chunks"] = wanted;
    }

    private string GetPlayerId() => Context.UserIdentifier!;
}
