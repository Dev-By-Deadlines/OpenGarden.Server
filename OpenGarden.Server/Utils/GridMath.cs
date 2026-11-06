using OpenGarden.Server.Constants;
namespace OpenGarden.Server.Utils;

public readonly record struct ChunkCoord(int X, int Y);
public readonly record struct CellCoord(int X, int Y);
public readonly record struct CellBounds(int MinX, int MaxX, int MinY, int MaxY);

public static class GridMath
{
    public static ChunkCoord GetChunk(CellCoord cellCoord)
    {
        return new ChunkCoord(cellCoord.X / GameConstants.ChunkSize, cellCoord.Y / GameConstants.ChunkSize);
    }

    public static CellBounds GetCellBoundsAroundChunk(ChunkCoord chunkCoord)
    {
        var renderDistance = GameConstants.RenderDistance;
        var lastChunk = GameConstants.ChunksPerSide - 1;

        var minChunkX = Math.Max(chunkCoord.X - renderDistance, 0);
        var maxChunkX = Math.Min(chunkCoord.X + renderDistance, lastChunk);
        var minChunkY = Math.Max(chunkCoord.Y - renderDistance, 0);
        var maxChunkY = Math.Min(chunkCoord.Y + renderDistance, lastChunk);

        return new CellBounds(
            MinX: minChunkX * GameConstants.ChunkSize,
            MaxX: (maxChunkX + 1) * GameConstants.ChunkSize - 1,
            MinY: minChunkY * GameConstants.ChunkSize,
            MaxY: (maxChunkY + 1) * GameConstants.ChunkSize - 1);
    }

    public static bool IsValidChunk(int chunkX, int chunkY)
    {
        return chunkX >= 0 && chunkX < GameConstants.ChunksPerSide
            && chunkY >= 0 && chunkY < GameConstants.ChunksPerSide;
    }

    public static bool IsInWorldBounds(int cellX, int cellY)
    {
        return cellX >= 0 && cellX < GameConstants.MapSize
            && cellY >= 0 && cellY < GameConstants.MapSize;
    }

    public static CellCoord GetRandomCellInChunk(ChunkCoord chunk)
    {
        var size = GameConstants.ChunkSize;
        return new CellCoord(
            Random.Shared.Next(chunk.X * size, (chunk.X + 1) * size),
            Random.Shared.Next(chunk.Y * size, (chunk.Y + 1) * size));
    }
}
