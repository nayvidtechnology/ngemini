using Nayvid.Gemini.Video; 
using Nayvid.Gemini.Video.Models;
using Nayvid.Gemini.Image;
using Nayvid.Gemini.Speech;
using Nayvid.Gemini.Music;
using Nayvid.Gemini.Core;
var builder = WebApplication.CreateBuilder(args);
var apiKey = Environment.GetEnvironmentVariable("GEMINI_API_KEY") ?? "YOUR_API_KEY";

// Add Swagger/OpenAPI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddSingleton(new GeminiVideoClientOptions { ApiKey = apiKey });
builder.Services.AddSingleton<IGeminiVideoClient, GeminiVideoClient>();
builder.Services.AddSingleton(new GeminiClientOptions { ApiKey = apiKey });
builder.Services.AddSingleton<IImageClient, ImageClient>();
builder.Services.AddSingleton<ISpeechClient, SpeechClient>();
builder.Services.AddSingleton<IMusicClient, MusicClient>();

var app = builder.Build();

// Always expose Swagger for this sample (even in Production) to ease local exploration
app.UseSwagger();
app.UseSwaggerUI(c => c.DocumentTitle = "Nayvid Gemini Sample API");

// Root landing endpoint to prevent 404 when navigating to '/'
app.MapGet("/", () => Results.Ok(new
{
    message = "Nayvid Gemini Sample API",
    swagger = "/swagger",
    health = "/health",
    version = "/version",
    endpoints = new[] { "/generate/video", "/generate/image", "/generate/speech", "/generate/music" }
}));

app.MapGet("/health", () => Results.Ok(new { status = "ok" }));

// Simple version & capabilities endpoint
app.MapGet("/version", () =>
{
    var ver = typeof(Program).Assembly.GetName().Version?.ToString() ?? "0.0.0";
    var domains = new[] { "video", "image", "speech", "music" };
    return Results.Ok(new { version = ver, domains });
});

app.MapPost("/generate/video", async (IGeminiVideoClient videoClient, GeminiVideoPrompt prompt, CancellationToken ct) =>
{
    if (string.IsNullOrWhiteSpace(prompt.Text)) return Results.BadRequest(new { error = "text required" });
    try
    {
        var start = await videoClient.StartResumableUploadAsync(new StartUploadRequest("placeholder.mp4", "video/mp4", 0), ct);
        var media = await videoClient.CompleteUploadAsync(start, ct);
        var opReal = await videoClient.GenerateFromVideoAsync(new GenerateFromVideoRequest(prompt.Model ?? "gemini-pro-video", new[] { new VideoPart(media.MediaId) }, prompt.Text), ct);
        return Results.Ok(new { operation = opReal.Name, status = opReal.Status, simulated = false });
    }
    catch (Exception ex)
    {
        var op = new Nayvid.Gemini.Core.Operation(Guid.NewGuid().ToString("N"), "PENDING", null, null);
        return Results.Ok(new { operation = op.Name, status = op.Status, simulated = true, error = ex.Message });
    }
});

app.MapPost("/generate/image", async (IImageClient imageClient, ImagePrompt prompt, CancellationToken ct) =>
{
    if (string.IsNullOrWhiteSpace(prompt.Text)) return Results.BadRequest(new { error = "text required" });
    var op = await imageClient.GenerateImageAsync(prompt.Model ?? "image-model", prompt.Text, ct);
    return Results.Ok(new { operation = op.Name, status = op.Status });
});

app.MapPost("/generate/speech", async (ISpeechClient speechClient, SpeechPrompt prompt, CancellationToken ct) =>
{
    if (string.IsNullOrWhiteSpace(prompt.Text)) return Results.BadRequest(new { error = "text required" });
    var bytes = await speechClient.TextToSpeechAsync(prompt.Model ?? "speech-model", prompt.Text, prompt.Voice ?? "default", ct);
    return Results.File(bytes, "audio/wav");
});

app.MapPost("/generate/music", async (IMusicClient musicClient, MusicPrompt prompt, CancellationToken ct) =>
{
    if (string.IsNullOrWhiteSpace(prompt.Text)) return Results.BadRequest(new { error = "text required" });
    var op = await musicClient.GenerateMusicAsync(prompt.Model ?? "music-model", prompt.Text, ct);
    return Results.Ok(new { operation = op.Name, status = op.Status });
});

app.Run();

public partial class Program { }

record GeminiVideoPrompt(string? Text, string? Model);
record ImagePrompt(string? Text, string? Model);
record SpeechPrompt(string? Text, string? Model, string? Voice);
record MusicPrompt(string? Text, string? Model);
