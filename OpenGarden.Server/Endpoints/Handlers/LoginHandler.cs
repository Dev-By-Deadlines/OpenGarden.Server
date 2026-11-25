using Microsoft.AspNetCore.Identity;
using NodaTime;
using OpenGarden.Server.Data;
using OpenGarden.Server.Dtos;
using OpenGarden.Server.Extensions;
using OpenGarden.Server.Models;
using OpenGarden.Server.Services;
using OpenGarden.Server.Utils;

namespace OpenGarden.Server.Endpoints.Handlers;

public class LoginHandler
{
    public static async Task<IResult> Handler(
        LoginDto loginDto,
        UserManager<ApplicationUser> userManager,
        AppDbContext dbContext,
        TokenProvider tokenProvider,
        IClock clock,
        HttpContext httpContext)
    {
        var user = await userManager.FindByNameAsync(loginDto.UserName);

        if (user is null || user.IsGuest)
            return Results.Unauthorized();

        if (await userManager.IsLockedOutAsync(user))
            return Results.Unauthorized();

        if (!await userManager.CheckPasswordAsync(user, loginDto.Password))
        {
            await userManager.AccessFailedAsync(user);
            return Results.Unauthorized();
        }

        await userManager.ResetAccessFailedCountAsync(user);

        var (accessToken, accessTokenExpiresAt) =
            await tokenProvider.CreateAccessToken(user);

        var refreshToken = tokenProvider.GenerateRefreshToken();

        var refreshTokenExpiresAt = clock.GetCurrentInstant()
            .Plus(Duration.FromDays(7));

        var refreshTokenEntity = new RefreshToken
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            Token = RefreshTokenHasher.Hash(refreshToken),
            Expires = refreshTokenExpiresAt
        };

        dbContext.RefreshTokens.Add(refreshTokenEntity);
        await dbContext.SaveChangesAsync();

        httpContext.AppendAccessTokenCookie(
            accessToken,
            accessTokenExpiresAt);

        httpContext.AppendRefreshTokenCookie(
            refreshToken,
            refreshTokenExpiresAt);

        return Results.Ok();
    }
}
