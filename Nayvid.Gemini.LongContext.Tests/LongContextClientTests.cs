using System.Linq;
using Nayvid.Gemini.LongContext;
using Xunit;

public class LongContextClientTests
{
    [Fact]
    public void Chunk_SplitsText()
    {
        var client = new LongContextClient();
        var chunks = client.Chunk("abcdefghijkl", 5).ToArray();
        Assert.Equal(3, chunks.Length);
        Assert.Equal("abcde", chunks[0]);
    }
}
