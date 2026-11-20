using Microsoft.AspNetCore.SignalR;
using OpenGarden.Server.Dtos;
using OpenGarden.Server.Hubs;
using OpenGarden.Server.Utils;

namespace OpenGarden.Server.Services;

public class ClientUpdater(IHubContext<GameHub> hubContext)
{
    public async Task NotifyCellsChanged(List<CellUpdateDto> cells)
    {
        var byChunk =
            cells.GroupBy(c => GridMath.GetChunk(new CellCoord(c.X, c.Y)));

        foreach (var group in byChunk)
        {
            await hubContext.Clients
                .Group(GridMath.GetGroupName(group.Key))
                .SendAsync("WorldUpdated", group.ToList());
        }
    }
}
