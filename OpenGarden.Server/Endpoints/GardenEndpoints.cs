using System.Security.Claims;
using OpenGarden.Server.Constants;
using OpenGarden.Server.Endpoints.Handlers;

namespace OpenGarden.Server.Endpoints;

public static class GardenEndpoints
{
    public static void MapEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api");

        group.MapPost("/guest", GuestHandler.Handler).RequireRateLimiting("GuestCreation");
        group.MapPost("/refresh-token", RefreshTokenHandler.Handler);
        group.MapPost("/register", RegisterHandler.Handler).RequireAuthorization();
        group.MapPost("/login", LoginHandler.Handler);
        group.MapPost("/logout", LogoutHandler.Handler);
        group.MapGet("/me", MeHandler.Handler).RequireAuthorization();
    }
}
