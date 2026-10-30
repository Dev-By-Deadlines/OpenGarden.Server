namespace OpenGarden.Server.Constants;

public static class GameConstants
{
    public const int MapSize = 256;
    public const int ChunkSize = 16;
    public const int RenderDistance = 1;
    public const int ChunksPerSide = MapSize / ChunkSize;

    public const int HarvestCost = 10;
    public const int PlantCost = 5;
    public const int GrowthTimeSeconds = 60;
}
