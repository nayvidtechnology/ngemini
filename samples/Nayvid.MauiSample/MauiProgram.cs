using Microsoft.Maui;
using Microsoft.Maui.Hosting;
using Nayvid.Gemini.Video;
using Nayvid.Gemini.Video.Models;
using Nayvid.Gemini.Image;
using Nayvid.Gemini.Speech;
using Nayvid.Gemini.Music;
using Nayvid.Gemini.Core;

namespace Nayvid.MauiSample;
public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder.UseMauiApp<App>();
        var apiKey = Environment.GetEnvironmentVariable("GEMINI_API_KEY") ?? "YOUR_API_KEY";
        builder.Services.AddSingleton(new GeminiVideoClientOptions { ApiKey = apiKey });
        builder.Services.AddSingleton<IGeminiVideoClient, GeminiVideoClient>();
        builder.Services.AddSingleton(new GeminiClientOptions { ApiKey = apiKey });
        builder.Services.AddSingleton<IImageClient, ImageClient>();
        builder.Services.AddSingleton<ISpeechClient, SpeechClient>();
        builder.Services.AddSingleton<IMusicClient, MusicClient>();
        return builder.Build();
    }
}
