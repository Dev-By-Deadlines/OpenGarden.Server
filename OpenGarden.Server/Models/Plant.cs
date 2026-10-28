using NodaTime;

namespace OpenGarden.Server.Models;

public class Plant
{
    public int X { get; set; }
    public int Y { get; set; }

    public short PlantTypeId { get; set; }
    public PlantType PlantType { get; set; } = null!;

    public Instant RipeAt { get; set; }
}
