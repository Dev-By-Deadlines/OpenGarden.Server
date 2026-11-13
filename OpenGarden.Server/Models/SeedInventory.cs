namespace OpenGarden.Server.Models;

public class SeedInventory
{
    public required string PlayerId { get; set; }
    public short PlantTypeId { get; set; }
    public int Quantity { get; set; }
}
