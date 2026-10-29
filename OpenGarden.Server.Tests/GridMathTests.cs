using OpenGarden.Server.Constants;
using OpenGarden.Server.Utils;

namespace OpenGarden.Server.Tests;

public class GridMathTests
{
    private const int Chunk = GameConstants.ChunkSize;
    private const int LastCell = GameConstants.MapSize - 1;
    private const int LastChunk = GameConstants.ChunksPerSide - 1;

    [Theory]
    [InlineData(0, 0, 0, 0)]                          // first cell
    [InlineData(Chunk - 1, Chunk - 1, 0, 0)]          // last cell of the first chunk
    [InlineData(Chunk, 0, 1, 0)]                      // first cell of the next chunk (x)
    [InlineData(0, Chunk, 0, 1)]                      // first cell of the next chunk (y)
    [InlineData(LastCell, LastCell, LastChunk, LastChunk)] // last cell of the map
    public void GetChunk_ReturnsExpectedChunk(int x, int y, int chunkX, int chunkY)
    {
        Assert.Equal(new ChunkCoord(chunkX, chunkY), GridMath.GetChunk(new CellCoord(x, y)));
    }

    [Theory]
    [InlineData(6, 12, 5 * Chunk, 8 * Chunk - 1, 11 * Chunk, 14 * Chunk - 1)]  // middle: full 3x3
    [InlineData(0, 0, 0, 2 * Chunk - 1, 0, 2 * Chunk - 1)]                      // top-left corner: clipped to 2x2
    [InlineData(LastChunk, LastChunk,
                (LastChunk - 1) * Chunk, LastCell,
                (LastChunk - 1) * Chunk, LastCell)]                              // bottom-right corner
    [InlineData(0, 8, 0, 2 * Chunk - 1, 7 * Chunk, 10 * Chunk - 1)]              // left edge: clipped on x only
    public void GetCellBoundsAroundChunk_ClipsAtMapEdges(
        int cx, int cy, int minX, int maxX, int minY, int maxY)
    {
        var bounds = GridMath.GetCellBoundsAroundChunk(new ChunkCoord(cx, cy));
        Assert.Equal(new CellBounds(minX, maxX, minY, maxY), bounds);
    }
}
