using System.Net.Http.Headers;
using Nayvid.Gemini.Core;
using Nayvid.Gemini.Video;
using Nayvid.Gemini.Video.Models;
using Nayvid.Gemini.Image;
using Nayvid.Gemini.Speech;

// Real API sample wiring (still uses current placeholder generation methods where real API not implemented)
// Set GEMINI_API_KEY in your environment before running.

var apiKey = Environment.GetEnvironmentVariable("GEMINI_API_KEY");
if (string.IsNullOrWhiteSpace(apiKey) || apiKey == "YOUR_API_KEY")
{
    Console.WriteLine("GEMINI_API_KEY not set or placeholder. Exiting.");
    return;
}

Console.WriteLine("API key length: " + apiKey.Length);

// Basic HttpClient transport with auth header example (if real endpoints existed)
var videoClient = new GeminiVideoClient(new GeminiVideoClientOptions { ApiKey = apiKey });
var imageClient = new ImageClient(new GeminiClientOptions { ApiKey = apiKey });
var speechClient = new SpeechClient(new GeminiClientOptions { ApiKey = apiKey });

// Video (placeholder pattern)
var startReq = new StartUploadRequest("sample.mp4", "video/mp4", 0);
var session = await videoClient.StartResumableUploadAsync(startReq);
var media = await videoClient.CompleteUploadAsync(session);
var op = await videoClient.GenerateFromVideoAsync(new GenerateFromVideoRequest("gemini-pro-video", new [] { new VideoPart(media.MediaId) }, "Describe this video"));
Console.WriteLine($"Video operation: {op.Name} status={op.Status}");

// Image
var imgOp = await imageClient.GenerateImageAsync("image-model", "A realistic waterfall at dusk");
Console.WriteLine($"Image operation: {imgOp.Name} status={imgOp.Status}");

// Speech
var audio = await speechClient.TextToSpeechAsync("speech-model", "Testing speech synthesis");
Console.WriteLine($"Speech bytes: {audio.Length}");

Console.WriteLine("Done.");
