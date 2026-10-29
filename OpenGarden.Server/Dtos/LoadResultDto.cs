namespace OpenGarden.Server.Dtos;

// We use a wrapper so we can add additional stuff later.
public record LoadResultDto(List<PlantDto> Plants);
