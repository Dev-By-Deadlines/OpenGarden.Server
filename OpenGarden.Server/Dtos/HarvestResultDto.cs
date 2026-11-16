namespace OpenGarden.Server.Dtos;

public record HarvestResultDto(
    ActionOutcome Outcome,
    short? PlantTypeId,
    int SeedsGained,
    PlayerStateDto? PlayerState);
