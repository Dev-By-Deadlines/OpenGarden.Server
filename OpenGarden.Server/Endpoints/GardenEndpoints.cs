using System.Security.Claims;
using OpenGarden.Server.Constants;
using OpenGarden.Server.Endpoints.Handlers;

namespace OpenGarden.Server.Endpoints;

public static class GardenEndpoints
{
    public static void MapEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/app");

        group.MapPost("/register", RegisterHandler.Handler)
            .RequireAuthorization();

        group.MapPost("/login", LoginHandler.Handler);
        group.MapPost("/refresh-token", RefreshTokenHandler.Handler);
        group.MapPost("/logout", LogoutHandler.Handler);

        group.MapGet("/me", (ClaimsPrincipal user) => Results.Ok(new
        {
            Id = user.FindFirstValue(ClaimTypes.NameIdentifier),
            UserName = user.Identity?.Name,
            Role = user.FindFirstValue(ClaimTypes.Role)
        })).RequireAuthorization();

        group.MapPost("/guest", GuestHandler.Handler);
    }
}
