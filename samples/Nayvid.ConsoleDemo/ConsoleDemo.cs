using System;
using System.IO;
using System.Threading.Tasks;
using Nayvid.Gemini.Video;
using Nayvid.Gemini.Video.Models;
using Nayvid.Gemini.Image;
using Nayvid.Gemini.Music;
using Nayvid.Gemini.Embeddings;
using Nayvid.Gemini.Speech;
using Nayvid.Gemini.StructuredOutput;
using Nayvid.Gemini.LongContext;
using Nayvid.Gemini.Core;

// Minimal fake transport to allow the demo to run without a real API key.
// When no valid key is supplied, we bypass real HTTP and simulate successful responses.
class FakeTransport : Nayvid.Gemini.Core.IHttpTransport
{
    public Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken = default)
    {
        if (request.Method == HttpMethod.Post && request.RequestUri != null && request.RequestUri.AbsoluteUri.Contains("upload?uploadType=resumable"))
        {
            var resp = new HttpResponseMessage(System.Net.HttpStatusCode.OK);
            resp.Headers.Location = new Uri(request.RequestUri + "/session123");
            return Task.FromResult(resp);
        }
        if (request.Method == HttpMethod.Put)
        {
            // Simulate either 308 (resume incomplete) once or final 200. Keep simple with 200.
            return Task.FromResult(new HttpResponseMessage(System.Net.HttpStatusCode.OK));
        }
        return Task.FromResult(new HttpResponseMessage(System.Net.HttpStatusCode.OK));
    }
}

class Program
{
    static async Task Main(string[] args)
    {
    var apiKey = Environment.GetEnvironmentVariable("GEMINI_API_KEY") ?? "YOUR_API_KEY";
    Console.WriteLine("Using API Key placeholder length: " + apiKey.Length);

    var useMock = string.IsNullOrWhiteSpace(apiKey) || apiKey == "YOUR_API_KEY" || apiKey.Length < 20;
    if (useMock) Console.WriteLine("No real API key detected – using mock transport (no network calls).");
    var videoOptions = new GeminiVideoClientOptions { ApiKey = apiKey };
    var client = useMock ? new GeminiVideoClient(videoOptions, null, new FakeTransport()) : new GeminiVideoClient(videoOptions);
        // Ensure a dummy sample.mp4 exists (placeholder bytes only)
        if (!File.Exists("sample.mp4"))
        {
            Console.WriteLine("sample.mp4 not found; creating dummy file (1.5MB) for demo...");
            var dummy = new byte[1536 * 1024];
            new Random().NextBytes(dummy);
            File.WriteAllBytes("sample.mp4", dummy);
        }
        UploadedMedia? media = null;
        try
        {
            using var stream = File.OpenRead("sample.mp4");
            var startReq = new StartUploadRequest("sample.mp4", "video/mp4", stream.Length);
            var session = await client.StartResumableUploadAsync(startReq);
            long offset = 0;
            int chunkSize = 1024 * 1024;
            while (offset < stream.Length)
            {
                int toWrite = (int)Math.Min(chunkSize, stream.Length - offset);
                var progress = await client.UploadChunkAsync(session, stream, offset, toWrite);
                offset += toWrite;
            }
            media = await client.CompleteUploadAsync(session);
        }
        catch (GeminiApiException ex)
        {
            Console.WriteLine("Video upload API error, switching to mock video media: " + ex.Message);
            media = new UploadedMedia("mock-media", "http://mock/session", null);
        }

        var videoPart = new VideoPart(media!.MediaId);
        var genReq = new GenerateFromVideoRequest("gemini-pro-video", new[] { videoPart }, "Describe this video");
        var op = await client.GenerateFromVideoAsync(genReq);
        await foreach (var result in client.WaitForCompletionAsync(op.Name, TimeSpan.FromSeconds(2), TimeSpan.FromSeconds(10)))
        {
            Console.WriteLine($"Video Op Status: {result.Status}");
            if (result.Status == "DONE") Console.WriteLine($"Video Result: {result.Result}");
        }

    // Image client demo (fake operation)
    var imageClient = new ImageClient(new GeminiClientOptions { ApiKey = apiKey });
    var imgOp = await imageClient.GenerateImageAsync("image-model", "A sunset over mountains");
    Console.WriteLine($"Image operation status: {imgOp.Status}");

    // Music client demo
    var musicClient = new MusicClient(new GeminiClientOptions { ApiKey = apiKey });
    var musicOp = await musicClient.GenerateMusicAsync("music-model", "Calm piano");
    Console.WriteLine($"Music operation status: {musicOp.Status}");

    // Embeddings client demo
    var embedClient = new EmbeddingsClient(new GeminiClientOptions { ApiKey = apiKey });
    var vector = await embedClient.EmbedAsync("embed-model", "hello world");
    Console.WriteLine("Embedding length: " + vector.Length);

    // Speech client demo
    var speechClient = new SpeechClient(new GeminiClientOptions { ApiKey = apiKey });
    var audio = await speechClient.TextToSpeechAsync("speech-model", "Welcome to Gemini");
    Console.WriteLine("Generated audio bytes: " + audio.Length);

    // Structured output demo
    var structuredClient = new StructuredOutputClient(new GeminiClientOptions { ApiKey = apiKey });
    var structured = await structuredClient.GenerateStructuredAsync<dynamic>("struct-model", "prompt text");
    Console.WriteLine("Structured echo model: " + structured?.model);

    // Long context demo
    var longCtx = new LongContextClient();
    var chunks = longCtx.Chunk(new string('x', 30), 10);
    Console.WriteLine("Chunks: " + string.Join('|', chunks));
    }
}
