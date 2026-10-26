using OpenGarden.Server.Constants;
namespace OpenGarden.Server.Utils;

public readonly record struct ChunkCoord(int X, int Y);

public static class GridMath
{
    public static ChunkCoord GetChunk(int cellX, int cellY)
    {
        return new ChunkCoord(cellX / GameConstants.ChunkSize, cellY / GameConstants.ChunkSize);
    }
}
