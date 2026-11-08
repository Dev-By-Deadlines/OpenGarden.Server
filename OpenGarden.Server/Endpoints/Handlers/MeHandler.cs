using System.Security.Claims;

namespace OpenGarden.Server.Endpoints.Handlers;

public class MeHandler
{
    public static async Task<IResult> Handler(ClaimsPrincipal user)
    {
        var result = new
        {
            Id = user.FindFirstValue(ClaimTypes.NameIdentifier),
            UserName = user.Identity?.Name,
            Role = user.FindFirstValue(ClaimTypes.Role)
        };

        return Results.Ok(result);
    }
}
