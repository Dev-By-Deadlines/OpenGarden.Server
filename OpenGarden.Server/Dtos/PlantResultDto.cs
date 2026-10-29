namespace OpenGarden.Server.Dtos;

public record PlantResultDto(ActionOutcome Outcome) { }

public enum ActionOutcome
{
    Success,
    CellOccupied,
    NotEnoughEnergy,
    OutOfBounds,
    UnknownPlantType,
}
