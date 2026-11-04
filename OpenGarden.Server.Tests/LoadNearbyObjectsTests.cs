using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.EntityFrameworkCore;
using OpenGarden.Server.Dtos;
using OpenGarden.Server.Utils;

namespace OpenGarden.Server.Tests;

public class LoadNearbyObjectsTests(OpenGardenWebApplicationFactory factory)
    : IClassFixture<OpenGardenWebApplicationFactory>
{
    [Fact]
    public async Task EmptyMap_ReturnsNoPlants()
    {
        await factory.ResetPlantsAsync();
        await using var connection = factory.CreateHubConnection();
        await connection.StartAsync(TestContext.Current.CancellationToken);

        var result = await connection.InvokeAsync<LoadResultDto>(
            "LoadNearbyObjects", 6, 12, TestContext.Current.CancellationToken);

        Assert.Empty(result.Plants);
    }

    [Fact]
    public async Task ReturnsOnlyPlantsInsideTheLoadedArea()
    {
        await factory.ResetPlantsAsync();

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

        await factory.InsertPlantsAsync(inside.Concat(outside));

        await using var connection = factory.CreateHubConnection();
        await connection.StartAsync(TestContext.Current.CancellationToken);

        var result = await connection.InvokeAsync<LoadResultDto>(
                "LoadNearbyObjects", focus.X, focus.Y, TestContext.Current.CancellationToken);

        var returned = result.Plants.Select(p => (p.X, p.Y)).ToHashSet();
        Assert.Equal(inside.ToHashSet(), returned);
    }
}
