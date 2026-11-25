using System.Security.Cryptography;
using System.Text;
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
        HttpContext httpContext)
    {
        var user = await userManager.FindByNameAsync(loginDto.UserName);

        if (user is null || user.IsGuest)
        {
            return Results.Unauthorized();
        }

        if (await userManager.IsLockedOutAsync(user))
        {
            return Results.Unauthorized();
        }

        if (!await userManager.CheckPasswordAsync(user, loginDto.Password))
        {
            await userManager.AccessFailedAsync(user);

            return Results.Unauthorized();
        }

        await userManager.ResetAccessFailedCountAsync(user);

        string accessToken = await tokenProvider.CreateAccessToken(user);

        var token = tokenProvider.GenerateRefreshToken();

        var refreshToken = new RefreshToken
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            Token = RefreshTokenHasher.Hash(token),
            Expires = SystemClock.Instance.GetCurrentInstant() + Duration.FromDays(7)
        };

        httpContext.AppendAccessTokenCookie(accessToken);
        httpContext.AppendRefreshTokenCookie(token);

        dbContext.RefreshTokens.Add(refreshToken);
        await dbContext.SaveChangesAsync();

        return Results.Ok();
    }
}
