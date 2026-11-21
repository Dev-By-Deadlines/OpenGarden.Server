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
        await using var scope = scopeFactory.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        await using var transaction =
            await dbContext.Database.BeginTransactionAsync(ct);

        var now = clock.GetCurrentInstant();

        var deletedTokens = await dbContext.RefreshTokens
            .Where(rt => rt.Expires <= now)
            .ExecuteDeleteAsync(ct);

        // Delete guest users without any remaining refresh tokens.
        var deletedUsers = await dbContext.Users
            .Where(u => u.IsGuest &&
                        !dbContext.RefreshTokens.Any(rt => rt.UserId == u.Id))
            .ExecuteDeleteAsync(ct);

        await transaction.CommitAsync(ct);

        if (deletedTokens > 0 || deletedUsers > 0)
        {
            logger.LogInformation(
                "Expired token cleanup: {Tokens} tokens and {Users} guest accounts deleted",
                deletedTokens,
                deletedUsers);
        }
    }
}
