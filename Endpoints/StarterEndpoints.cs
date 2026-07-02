using StarterKit.Api.Endpoints.Handlers;

namespace StarterKit.Api.Endpoints;

public static class StarterEndpoints
{
    public static void MapEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/app");

        group.MapPost("/login", LoginHandler.Handler);
    }
}
