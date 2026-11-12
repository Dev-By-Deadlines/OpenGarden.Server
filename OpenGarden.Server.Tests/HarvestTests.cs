using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using OpenGarden.Server.Constants;
using OpenGarden.Server.Data;
using OpenGarden.Server.Dtos;

namespace OpenGarden.Server.Tests;

public class HarvestTests(OpenGardenWebApplicationFactory factory)
    : IClassFixture<OpenGardenWebApplicationFactory>
{
    [Fact]
    public async Task Harvest_ExistingPlant_ReturnsSuccessAndRemovesIt()
    {
        var ct = TestContext.Current.CancellationToken;
        await factory.ResetPlantsAsync();
        await factory.InsertPlantsAsync([(0, 0)]);

        await using var connection = factory.CreateHubConnection();
        await connection.StartAsync(ct);

        var result = await connection.InvokeAsync<HarvestResultDto>("Harvest", 0, 0, ct);

        Assert.Equal(ActionOutcome.Success, result.Outcome);

        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        Assert.False(await db.Plants.AnyAsync(p => p.X == 0 && p.Y == 0, ct));
    }

    [Fact]
    public async Task Harvest_EmptyCell_ReturnsCellEmpty()
    {
        var ct = TestContext.Current.CancellationToken;
        await factory.ResetPlantsAsync();

        await using var connection = factory.CreateHubConnection();
        await connection.StartAsync(ct);

        var result = await connection.InvokeAsync<HarvestResultDto>("Harvest", 0, 0, ct);

        Assert.Equal(ActionOutcome.CellEmpty, result.Outcome);
    }

    [Fact]
    public async Task Harvest_SimultaneousHarvestsOfSameCell_ExactlyOneSucceeds()
    {
        var ct = TestContext.Current.CancellationToken;
        await factory.ResetPlantsAsync();

        const int cells = GameConstants.GuestMaxEnergy / GameConstants.HarvestCost;
        await factory.InsertPlantsAsync(Enumerable.Range(0, cells).Select(x => (x, 0)));

        await using var a = factory.CreateHubConnection();
        await using var b = factory.CreateHubConnection();
        await a.StartAsync(ct);
        await b.StartAsync(ct);

        for (int x = 0; x < cells; x++)
        {
            var first = a.InvokeAsync<HarvestResultDto>("Harvest", x, 0, ct);
            var second = b.InvokeAsync<HarvestResultDto>("Harvest", x, 0, ct);

            var results = await Task.WhenAll(first, second);

            Assert.Single(results, r => r.Outcome == ActionOutcome.Success);
            Assert.Single(results, r => r.Outcome == ActionOutcome.CellEmpty);
        }
    }

    [Fact]
    public async Task Harvest_WhenEnergyIsInsufficient_ReturnsNotEnoughEnergy()
    {
        var ct = TestContext.Current.CancellationToken;

        await factory.ResetPlantsAsync();

        await using var client = factory.CreateHubConnection();
        await client.StartAsync(ct);

        var maxHarvestCount =
            GameConstants.GuestMaxEnergy / GameConstants.HarvestCost;

        for (int x = 0; x < maxHarvestCount; x++)
        {
            await factory.InsertPlantsAsync([(0, 0)]);

            var result = await client.InvokeAsync<HarvestResultDto>(
                "Harvest", 0, 0, ct);

            Assert.Equal(ActionOutcome.Success, result.Outcome);
        }

        await factory.InsertPlantsAsync([(0, 0)]);

        var finalResult = await client.InvokeAsync<HarvestResultDto>(
            "Harvest", 0, 0, ct);

        Assert.Equal(ActionOutcome.NotEnoughEnergy, finalResult.Outcome);
    }
}
