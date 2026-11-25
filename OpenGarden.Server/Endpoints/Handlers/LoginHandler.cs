using Microsoft.AspNetCore.Identity;
using OpenGarden.Server.Data;
using OpenGarden.Server.Dtos;
using OpenGarden.Server.Extensions;
using OpenGarden.Server.Services;

namespace OpenGarden.Server.Endpoints.Handlers;

public class LoginHandler
{
    public static async Task<IResult> Handler(
        LoginDto loginDto,
        UserManager<ApplicationUser> userManager,
        AppDbContext dbContext,
        TokenProvider tokenProvider,
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

        var access = await tokenProvider.CreateAccessToken(user);
        var refresh = tokenProvider.CreateRefreshToken(user.Id);

        await dbContext.SaveChangesAsync();

        httpContext.AppendAuthCookies(access, refresh);

        return Results.Ok();
    }
}
