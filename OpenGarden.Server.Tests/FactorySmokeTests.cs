using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using OpenGarden.Server.Data;

namespace OpenGarden.Server.Tests;

public class FactorySmokeTests(OpenGardenWebApplicationFactory factory)
    : IClassFixture<OpenGardenWebApplicationFactory>
{
    [Fact]
    public async Task Database_HasThePalette()
    {
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var count = await db.PlantTypes.CountAsync();

        Assert.Equal(16, count);
    }
}
