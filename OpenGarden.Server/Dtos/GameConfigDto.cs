namespace OpenGarden.Server.Dtos;

public record GameConfigDto(
        int MapSize,
        int ChunkSize,
        int HarvestCost,
        int PlantCost,
        int GrowthTimeSeconds,
        long ServerTimeUnixMilliseconds)
{ }
