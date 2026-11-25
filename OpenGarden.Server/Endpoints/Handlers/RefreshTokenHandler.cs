using Microsoft.EntityFrameworkCore;
using NodaTime;
using OpenGarden.Server.Data;
using OpenGarden.Server.Extensions;
using OpenGarden.Server.Models;
using OpenGarden.Server.Services;
using OpenGarden.Server.Utils;

namespace OpenGarden.Server.Endpoints.Handlers;

public class RefreshTokenHandler
{
    public static async Task<IResult> Handler(
        AppDbContext dbContext,
        TokenProvider tokenProvider,
        HttpContext httpContext)
    {
        var token = httpContext.Request.Cookies["refreshToken"];

        if (string.IsNullOrEmpty(token))
        {
            return Results.Unauthorized();
        }

        var tokenHash = RefreshTokenHasher.Hash(token);

        RefreshToken? storedRefreshToken = await dbContext.RefreshTokens
            .Include(rt => rt.ApplicationUser)
            .FirstOrDefaultAsync(rt => rt.Token == tokenHash);

        if (storedRefreshToken is null ||
            storedRefreshToken.Expires < SystemClock.Instance.GetCurrentInstant())
        {
            return Results.Unauthorized();
        }

        var accessToken = await tokenProvider.CreateAccessToken(
            storedRefreshToken.ApplicationUser);

        var newToken = tokenProvider.GenerateRefreshToken();

        storedRefreshToken.Token = RefreshTokenHasher.Hash(newToken);
        storedRefreshToken.Expires = SystemClock.Instance.GetCurrentInstant()
            + Duration.FromDays(7);

        httpContext.AppendAccessTokenCookie(accessToken);
        httpContext.AppendRefreshTokenCookie(newToken);

        await dbContext.SaveChangesAsync();

        return Results.Ok();
    }
}
