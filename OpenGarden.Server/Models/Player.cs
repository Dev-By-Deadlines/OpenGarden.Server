namespace OpenGarden.Server.Models;

using NodaTime;
using OpenGarden.Server.Data;

public class Player
{
    public required string Id { get; set; }
    public int Energy { get; set; }
    public Instant EnergyUpdatedAt { get; set; }
    public ApplicationUser User { get; set; } = null!;
}
