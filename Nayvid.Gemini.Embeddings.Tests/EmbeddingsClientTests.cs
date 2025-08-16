using System.Threading.Tasks;
using Nayvid.Gemini.Embeddings;
using Nayvid.Gemini.Core;
using Xunit;

public class EmbeddingsClientTests
{
    [Fact]
    public async Task Embed_ReturnsVector()
    {
        var client = new EmbeddingsClient(new GeminiClientOptions { ApiKey = "test" });
        var vec = await client.EmbedAsync("embed-model", "hello world");
        Assert.NotNull(vec);
        Assert.True(vec.Length == 8);
        Assert.InRange(vec[0], 0, 1);
    }
}
