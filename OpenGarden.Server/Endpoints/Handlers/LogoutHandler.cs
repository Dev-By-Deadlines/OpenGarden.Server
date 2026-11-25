using Microsoft.EntityFrameworkCore;
using OpenGarden.Server.Data;
using OpenGarden.Server.Extensions;
using OpenGarden.Server.Utils;

namespace OpenGarden.Server.Endpoints.Handlers;

public class LogoutHandler
{
    public static async Task<IResult> Handler(HttpContext httpContext, AppDbContext dbContext)
    {
        var refreshToken = httpContext.GetRefreshToken();

        if (!string.IsNullOrEmpty(refreshToken))
        {
            var tokenHash = RefreshTokenHasher.Hash(refreshToken);

            var storedRefreshToken = await dbContext.RefreshTokens
                .FirstOrDefaultAsync(rt => rt.Token == tokenHash);

            if (storedRefreshToken is not null)
            {
                dbContext.RefreshTokens.Remove(storedRefreshToken);
                await dbContext.SaveChangesAsync();
            }
        }

        httpContext.DeleteAuthCookies();
        return Results.Ok();
    }
}
