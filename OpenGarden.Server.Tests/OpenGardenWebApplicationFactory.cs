using Testcontainers.PostgreSql;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using OpenGarden.Server.Data;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.AspNetCore.Http.Connections;
using System.Text.Json.Serialization;
using OpenGarden.Server.Models;
using NodaTime;

namespace OpenGarden.Server.Tests;


public class OpenGardenWebApplicationFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly PostgreSqlContainer _postgres = new PostgreSqlBuilder().Build();

    public async ValueTask InitializeAsync()
    {
        await _postgres.StartAsync();

        using var scope = Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        await db.Database.MigrateAsync();
    }

    public override async ValueTask DisposeAsync()
    {
        await _postgres.DisposeAsync();
        await base.DisposeAsync();
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureTestServices(services =>
        {
            services.RemoveAll<DbContextOptions<AppDbContext>>();
            services.AddDbContextPool<AppDbContext>(options =>
                options.UseNpgsql(_postgres.GetConnectionString(), npgsql => npgsql.UseNodaTime()));
        });

        builder.UseSetting("Jwt:SecretKey", "test-only-signing-key-at-least-32-chars-long!");
        builder.UseSetting("RateLimiting:PermitLimit", "100000");
        builder.UseSetting("Spawner:Enabled", "false");
    }

    public HubConnection CreateHubConnection()
    {
        return new HubConnectionBuilder()
            .WithUrl("http://localhost/hubs/game", options =>
            {
                options.HttpMessageHandlerFactory = _ => Server.CreateHandler();
                options.Transports = HttpTransportType.LongPolling;
            })
            .AddJsonProtocol(o => o.PayloadSerializerOptions.Converters.Add(new JsonStringEnumConverter()))
            .Build();
    }

    public async Task ResetPlantsAsync()
    {
        using var scope = Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        await db.Plants.ExecuteDeleteAsync();
    }

    public async Task InsertPlantsAsync(IEnumerable<(int X, int Y)> cells)
    {
        using var scope = Services.CreateScope();
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
}
