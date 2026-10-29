using Microsoft.AspNetCore.SignalR.Client;
using OpenGarden.Server.Dtos;

namespace OpenGarden.Server.Tests;

public class LoadNearbyObjectsTests(OpenGardenWebApplicationFactory factory)
    : IClassFixture<OpenGardenWebApplicationFactory>
{
    [Fact]
    public async Task EmptyMap_ReturnsNoPlants()
    {
        await using var connection = factory.CreateHubConnection();
        await connection.StartAsync(TestContext.Current.CancellationToken);

        var result = await connection.InvokeAsync<LoadResultDto>(
            "LoadNearbyObjects", 6, 12, TestContext.Current.CancellationToken);

        Assert.Empty(result.Plants);
    }
}
