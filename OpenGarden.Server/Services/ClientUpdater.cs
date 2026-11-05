using Microsoft.AspNetCore.SignalR;
using OpenGarden.Server.Dtos;
using OpenGarden.Server.Hubs;

namespace OpenGarden.Server.Services;

public class ClientUpdater(IHubContext<GameHub> hubContext)
{
    public Task UpdateAllClients(List<CellUpdateDto> cells)
        => hubContext.Clients.All.SendAsync("WorldUpdated", cells);
}
