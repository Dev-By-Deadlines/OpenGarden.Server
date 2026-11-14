using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using OpenGarden.Server.Constants;
using OpenGarden.Server.Data;
using OpenGarden.Server.Dtos;

namespace OpenGarden.Server.Tests;

public class PlantTests(OpenGardenWebApplicationFactory factory)
    : IClassFixture<OpenGardenWebApplicationFactory>
{
    [Fact]
    public async Task Plant_WithUnknownPlantType_ReturnsUnknownPlantType()
    {
        await factory.ResetPlantsAsync();
        await using var connection = factory.CreateHubConnection();
        await connection.StartAsync(TestContext.Current.CancellationToken);
        var result = await connection
            .InvokeAsync<PlantResultDto>
            ("Plant", 9900, 0, 0, TestContext.Current.CancellationToken);
        Assert.Equal(ActionOutcome.UnknownPlantType, result.Outcome);
    }

    [Fact]
    public async Task Plant_SimultaneousPlantsOnSameCell_ExactlyOneSucceeds()
    {
        var ct = TestContext.Current.CancellationToken;
        await factory.ResetPlantsAsync();

        await using var a = factory.CreateHubConnection();
        await using var b = factory.CreateHubConnection();
        await a.StartAsync(ct);
        await b.StartAsync(ct);

        const int cells = GameConstants.GuestMaxEnergy / GameConstants.PlantCost;
        for (int x = 0; x < cells; x++)
        {
            // start both calls before awaiting either one
            var first = a.InvokeAsync<PlantResultDto>("Plant", (short)(x + 1), x, 0, ct);
            var second = b.InvokeAsync<PlantResultDto>("Plant", (short)(x + 1), x, 0, ct);

            var results = await Task.WhenAll(first, second);

            Assert.Single(results, r => r.Outcome == ActionOutcome.Success);
            Assert.Single(results, r => r.Outcome == ActionOutcome.CellOccupied);
        }

        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        Assert.Equal(cells, await db.Plants.CountAsync(ct));
    }
}
