using System.Threading.Tasks;
using Nayvid.Gemini.StructuredOutput;
using Nayvid.Gemini.Core;
using Xunit;

public class StructuredOutputClientTests
{
    private class EchoModel { public string model { get; set; } = string.Empty; public string prompt { get; set; } = string.Empty; }

    [Fact]
    public async Task GenerateStructured_EchoesFields()
    {
        var client = new StructuredOutputClient(new GeminiClientOptions { ApiKey = "test" });
        var result = await client.GenerateStructuredAsync<EchoModel>("struct-model", "prompt text");
        Assert.NotNull(result);
        Assert.Equal("struct-model", result!.model);
        Assert.Equal("prompt text", result.prompt);
    }
}
