using System.Text;
using System.Threading.Tasks;
using Nayvid.Gemini.Speech;
using Nayvid.Gemini.Core;
using Xunit;

public class SpeechClientTests
{
    [Fact]
    public async Task TextToSpeech_ReturnsBytes()
    {
        var client = new SpeechClient(new GeminiClientOptions { ApiKey = "test" });
        var audio = await client.TextToSpeechAsync("speech-model", "hello");
        Assert.NotEmpty(audio);
    }

    [Fact]
    public async Task SpeechToText_ReturnsTranscript()
    {
        var client = new SpeechClient(new GeminiClientOptions { ApiKey = "test" });
        var bytes = Encoding.UTF8.GetBytes("fake");
        var text = await client.SpeechToTextAsync("speech-model", bytes);
        Assert.Contains("Transcript", text);
    }
}
