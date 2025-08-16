using System.Threading.Tasks;
using Nayvid.Gemini.Image;
using Nayvid.Gemini.Core;
using Xunit;

public class ImageClientTests
{
    [Fact]
    public async Task GenerateImage_ReturnsPendingOperation()
    {
        var client = new ImageClient(new GeminiClientOptions { ApiKey = "test" });
        var op = await client.GenerateImageAsync("image-model", "a cat");
        Assert.NotNull(op);
        Assert.Equal("PENDING", op.Status);
        Assert.False(op.IsDone);
    }
}
