using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using NodaTime;
using OpenGarden.Server.Constants;
using OpenGarden.Server.Data;
using OpenGarden.Server.Extensions;
using OpenGarden.Server.Models;
using OpenGarden.Server.Services;

namespace OpenGarden.Server.Endpoints.Handlers;

public class GuestHandler
{
    public static async Task<IResult> Handler(
        AppDbContext dbContext,
        UserManager<ApplicationUser> userManager,
        TokenProvider tokenProvider,
        HttpContext httpContext)
    {
        await using var transaction =
            await dbContext.Database.BeginTransactionAsync();

        var userId = Guid.NewGuid().ToString();

        var user = new ApplicationUser
        {
            Id = userId,
            UserName = GetRandomGuestName(),
            IsGuest = true
        };

        var player = new Player
        {
            Id = userId,
            Energy = GameConstants.MaxEnergy
        };

        var createUserResult = await userManager.CreateAsync(user);

        if (!createUserResult.Succeeded)
        {
            var errors = createUserResult.Errors.ToArray();

            if (errors.Any(e => e.Code == "DuplicateUserName"))
                return Results.Conflict(errors);

            return Results.BadRequest(errors);
        }

        var addToRoleResult = await userManager.AddToRoleAsync(user, Roles.Guest);

        if (!addToRoleResult.Succeeded)
        {
            return Results.BadRequest(addToRoleResult.Errors);
        }

        dbContext.Players.Add(player);

        var refreshToken = new RefreshToken
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            Token = tokenProvider.GenerateRefreshToken(),
            Expires = SystemClock.Instance.GetCurrentInstant() + Duration.FromDays(7)
        };

        dbContext.RefreshTokens.Add(refreshToken);

        var plantTypeIds = await dbContext.PlantTypes.Select(t => t.Id).ToListAsync();

        foreach (var typeId in plantTypeIds)
        {
            dbContext.SeedInventories.Add(new SeedInventory
            {
                PlayerId = userId,
                PlantTypeId = typeId,
                Quantity = GameConstants.StarterSeedsPerType
            });
        }

        await dbContext.SaveChangesAsync();

        string accessToken = await tokenProvider.CreateAccessToken(user);

        await transaction.CommitAsync();

        httpContext.AppendAccessTokenCookie(accessToken);
        httpContext.AppendRefreshTokenCookie(refreshToken.Token);

        return Results.Ok();
    }

    private static string GetRandomGuestName()
    {
        var guid = Guid.NewGuid().ToString("N");
        var randomPart = guid.Substring(0, 9);

        return $"Guest_{randomPart}";
    }
}
