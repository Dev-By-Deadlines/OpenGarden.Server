namespace OpenGarden.Server.Models;

using NodaTime;

public class Player
{
    public required string Id { get; set; } = null!;
    public required string DisplayName { get; set; }
    public int Energy { get; set; }
    public Instant EnergyUpdatedAt { get; set; }
}
