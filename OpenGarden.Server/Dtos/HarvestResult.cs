namespace OpenGarden.Server.Dtos;

public record HarvestResult(
    ActionOutcome Outcome,
    short? PlantTypeId = null,
    int SeedsGained = 0);
