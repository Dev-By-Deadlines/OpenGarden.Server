using Microsoft.EntityFrameworkCore;
using OpenGarden.Server.Data;
using OpenGarden.Server.Extensions;

namespace OpenGarden.Server.Endpoints.Handlers;

public class LogoutHandler
{
    public static async Task<IResult> Handler(HttpContext httpContext, AppDbContext dbContext)
    {
        var refreshToken = httpContext.GetRefreshToken();

        if (refreshToken is null)
        {
            httpContext.DeleteAuthCookies();
            return Results.Ok();
        }

        var storedRefreshToken = await dbContext.RefreshTokens
            .FirstOrDefaultAsync(rt => rt.Token == refreshToken);

        if (storedRefreshToken is not null)
        {
            dbContext.RefreshTokens.Remove(storedRefreshToken);
            await dbContext.SaveChangesAsync();
        }

        httpContext.DeleteAuthCookies();
        return Results.Ok();
    }
}
