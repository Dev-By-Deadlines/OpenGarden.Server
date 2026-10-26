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
    public void GetChunk_ReturnsExpectedChunk(int x, int y, int cx, int cy)
    {
        Assert.Equal(new ChunkCoord(cx, cy), GridMath.GetChunk(x, y));
    }
}
