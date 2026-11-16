namespace OpenGarden.Server.Dtos;

public record PlantResultDto(ActionOutcome Outcome, PlayerStateDto? PlayerState) { }

public enum ActionOutcome
{
    Success,
    CellOccupied,
    NotEnoughEnergy,
    OutOfBounds,
    UnknownPlantType,
    CellEmpty,
    UnknownPlayer,
    NoSeeds
}
