using System.Threading.Tasks;
using Nayvid.Gemini.Music;
using Nayvid.Gemini.Core;
using Xunit;

public class MusicClientTests
{
    [Fact]
    public async Task GenerateMusic_ReturnsPendingOperation()
    {
        var client = new MusicClient(new GeminiClientOptions { ApiKey = "test" });
        var op = await client.GenerateMusicAsync("music-model", "a melody");
        Assert.Equal("PENDING", op.Status);
    }
}
