using Microsoft.EntityFrameworkCore;
using NodaTime;
using OpenGarden.Server.Data;

namespace OpenGarden.Server.Jobs;

public class ExpiredTokenCleanupJob(
    IServiceScopeFactory scopeFactory,
    IClock clock,
    ILogger<ExpiredTokenCleanupJob> logger) : BackgroundService
{
    private static readonly TimeSpan Interval = TimeSpan.FromHours(1);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(Interval);

        while (await timer.WaitForNextTickAsync(stoppingToken))
        {
            try
            {
                await DeleteExpiredTokensAsync(stoppingToken);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                logger.LogError(ex, "Expired token cleanup failed");
            }
        }
    }

    private async Task DeleteExpiredTokensAsync(CancellationToken ct)
    {
        using var scope = scopeFactory.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var now = clock.GetCurrentInstant();
        var deleted = await dbContext.RefreshTokens
            .Where(rt => rt.Expires < now)
            .ExecuteDeleteAsync(ct);

        if (deleted > 0)
            logger.LogInformation("Deleted {Count} expired refresh tokens", deleted);
    }
}
