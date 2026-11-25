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
        IClock clock,
        HttpContext httpContext)
    {
        var token = httpContext.GetRefreshToken();

        if (string.IsNullOrEmpty(token))
            return Results.Unauthorized();

        var tokenHash = RefreshTokenHasher.Hash(token);

        RefreshToken? storedRefreshToken = await dbContext.RefreshTokens
            .Include(rt => rt.ApplicationUser)
            .FirstOrDefaultAsync(rt => rt.Token == tokenHash);

        if (storedRefreshToken is null ||
            storedRefreshToken.Expires < clock.GetCurrentInstant())
        {
            return Results.Unauthorized();
        }

        var access = await tokenProvider.CreateAccessToken(
            storedRefreshToken.ApplicationUser);

        var refresh = tokenProvider.RotateRefreshToken(storedRefreshToken);

        await dbContext.SaveChangesAsync();

        httpContext.AppendAuthCookies(access, refresh);

        return Results.Ok();
    }
}
