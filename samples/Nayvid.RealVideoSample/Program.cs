using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;

// Real video generation sample calling Gemini Long Running operation endpoint directly
// Prerequisites: set GEMINI_API_KEY environment variable
// Usage: dotnet run --project samples/Nayvid.RealVideoSample/Nayvid.RealVideoSample.csproj -- "your prompt here"

var apiKey = Environment.GetEnvironmentVariable("GEMINI_API_KEY");
if (string.IsNullOrWhiteSpace(apiKey)) { Console.WriteLine("GEMINI_API_KEY not set"); return; }

var prompt = args.Length > 0 ? string.Join(' ', args) : "A cinematic sunrise over a misty forest, drone shot.";
Console.WriteLine($"Prompt: {prompt}");

var http = new HttpClient { BaseAddress = new Uri("https://generativelanguage.googleapis.com/v1beta/") };
http.DefaultRequestHeaders.Add("x-goog-api-key", apiKey);

var requestBody = new
{
    instances = new[] { new { prompt } }
};

Console.WriteLine("Submitting generate request...");
var predictResp = await http.PostAsJsonAsync("models/veo-3.0-generate-preview:predictLongRunning", requestBody);
predictResp.EnsureSuccessStatusCode();
var predictJson = await predictResp.Content.ReadAsStringAsync();
using var predictDoc = JsonDocument.Parse(predictJson);
var operationName = predictDoc.RootElement.GetProperty("name").GetString();
Console.WriteLine($"Operation: {operationName}");

// Poll
var pollInterval = TimeSpan.FromSeconds(10);
while (true)
{
    await Task.Delay(pollInterval);
    var opResp = await http.GetAsync(operationName);
    opResp.EnsureSuccessStatusCode();
    var opJson = await opResp.Content.ReadAsStringAsync();
    using var opDoc = JsonDocument.Parse(opJson);
    var done = opDoc.RootElement.TryGetProperty("done", out var doneEl) && doneEl.GetBoolean();
    Console.WriteLine(DateTime.UtcNow.ToString("HH:mm:ss") + " status: " + (done ? "DONE" : "PENDING"));
    if (done)
    {
        // path: response.generateVideoResponse.generatedSamples[0].video.uri
        if (opDoc.RootElement.TryGetProperty("response", out var respEl) &&
            respEl.TryGetProperty("generateVideoResponse", out var gvr) &&
            gvr.TryGetProperty("generatedSamples", out var samples) && samples.ValueKind == JsonValueKind.Array && samples.GetArrayLength() > 0)
        {
            var first = samples[0];
            if (first.TryGetProperty("video", out var videoEl) && videoEl.TryGetProperty("uri", out var uriEl))
            {
                var videoUri = uriEl.GetString();
                Console.WriteLine("Downloading video: " + videoUri);
                using var videoReq = new HttpRequestMessage(HttpMethod.Get, videoUri);
                videoReq.Headers.Add("x-goog-api-key", apiKey);
                using var videoResp = await http.SendAsync(videoReq);
                videoResp.EnsureSuccessStatusCode();
                var bytes = await videoResp.Content.ReadAsByteArrayAsync();
                var fileName = "generated_video.mp4";
                await File.WriteAllBytesAsync(fileName, bytes);
                Console.WriteLine($"Saved {fileName} ({bytes.Length / 1024} KB)");
            }
        }
        break;
    }
}

Console.WriteLine("Complete.");
