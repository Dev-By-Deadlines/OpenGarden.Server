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

    public const int MaxEnergy = 100;
    public const int GuestMaxEnergy = 50;
    public const int EnergyRegenSeconds = 6;
    public const int GuestEnergyRegenSeconds = 6;
    public const int StarterSeedsPerType = 5;

    public const int SpawnIntervalSeconds = 60;
}
