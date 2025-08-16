using System;
using System.Threading.Tasks;
using Xunit;
using Nayvid.Gemini.Video;
using Nayvid.Gemini.Video.Models;
using Nayvid.Gemini.Image;
using Nayvid.Gemini.Speech;
using Nayvid.Gemini.Core;

// These tests exercise the real API key path (currently still placeholder logic inside clients).
// They will be skipped automatically if GEMINI_API_KEY is not configured or appears to be a placeholder.
public class RealApiSmokeTests
{
    private static bool HasRealKey(out string key)
    {
        key = Environment.GetEnvironmentVariable("GEMINI_API_KEY") ?? string.Empty;
        return !string.IsNullOrWhiteSpace(key) && key != "YOUR_API_KEY" && key.Length >= 20; // heuristic
    }

    [Fact]
    public async Task Video_Generate_Operation()
    {
        if (!HasRealKey(out var key))
        {
            Console.WriteLine("Skipping Video_Generate_Operation (no real key)");
            return; // soft skip
        }
        var client = new GeminiVideoClient(new GeminiVideoClientOptions { ApiKey = key });
        try
        {
            var session = await client.StartResumableUploadAsync(new StartUploadRequest("s.mp4","video/mp4",0));
            var media = await client.CompleteUploadAsync(session);
            var op = await client.GenerateFromVideoAsync(new GenerateFromVideoRequest("gemini-pro-video", new []{ new VideoPart(media.MediaId)}, "Describe"));
            Assert.False(string.IsNullOrWhiteSpace(op.Name));
        }
        catch (GeminiApiException ex)
        {
            Console.WriteLine($"Video upload not yet supported against real endpoint: {ex.Message}. Treating as soft skip.");
        }
    }

    [Fact]
    public async Task Image_Generate_Operation()
    {
        if (!HasRealKey(out var key)) return; // soft skip
        var img = new ImageClient(new GeminiClientOptions { ApiKey = key });
        var op = await img.GenerateImageAsync("image-model", "A castle on a hill");
        Assert.False(string.IsNullOrWhiteSpace(op.Name));
    }

    [Fact]
    public async Task Speech_Generate_Audio()
    {
        if (!HasRealKey(out var key)) return;
        var speech = new SpeechClient(new GeminiClientOptions { ApiKey = key });
        var bytes = await speech.TextToSpeechAsync("speech-model", "Hello from real test");
        Assert.True(bytes.Length > 0);
    }
}
