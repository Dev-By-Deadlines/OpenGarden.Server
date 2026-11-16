namespace OpenGarden.Server.Dtos;

public record PlayerStateDto(int Energy, int MaxEnergy, int EnergyRegenSeconds, List<SeedStackDto> Seeds);
