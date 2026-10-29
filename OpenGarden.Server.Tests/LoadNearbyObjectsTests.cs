using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using NodaTime;
using OpenGarden.Server.Data;
using OpenGarden.Server.Dtos;
using OpenGarden.Server.Models;
using OpenGarden.Server.Utils;

namespace OpenGarden.Server.Tests;

public class LoadNearbyObjectsTests(OpenGardenWebApplicationFactory factory)
    : IClassFixture<OpenGardenWebApplicationFactory>
{
    private async Task ResetPlantsAsync()
    {
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        await db.Plants.ExecuteDeleteAsync(TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task EmptyMap_ReturnsNoPlants()
    {
        await ResetPlantsAsync();
        await using var connection = factory.CreateHubConnection();
        await connection.StartAsync(TestContext.Current.CancellationToken);

        var result = await connection.InvokeAsync<LoadResultDto>(
            "LoadNearbyObjects", 6, 12, TestContext.Current.CancellationToken);

        Assert.Empty(result.Plants);
    }

    private async Task InsertPlantsAsync(IEnumerable<(int X, int Y)> cells)
    {
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        db.Plants.AddRange(cells.Select(c => new Plant
        {
            X = c.X,
            Y = c.Y,
            PlantTypeId = 1,
            RipeAt = Instant.FromUnixTimeSeconds(0)
        }));

        await db.SaveChangesAsync(TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task ReturnsOnlyPlantsInsideTheLoadedArea()
    {
        await ResetPlantsAsync();

        var focus = new ChunkCoord(6, 12);   // far enough from the map edges for a full area
        var area = GridMath.GetCellBoundsAroundChunk(focus);

        var inside = new[]
        {
            (area.MinX, area.MinY),
            (area.MaxX, area.MaxY),
        };
        var outside = new[]
        {
            (area.MinX - 1, area.MinY),   // just left
            (area.MaxX + 1, area.MinY),   // just right
            (area.MinX, area.MinY - 1),   // just above
            (area.MinX, area.MaxY + 1),   // just below
        };

        await InsertPlantsAsync(inside.Concat(outside));

        await using var connection = factory.CreateHubConnection();
        await connection.StartAsync(TestContext.Current.CancellationToken);

        var result = await connection.InvokeAsync<LoadResultDto>(
                "LoadNearbyObjects", focus.X, focus.Y, TestContext.Current.CancellationToken);

        var returned = result.Plants.Select(p => (p.X, p.Y)).ToHashSet();
        Assert.Equal(inside.ToHashSet(), returned);
    }
}
