using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Nayvid.Gemini.Video;
using Nayvid.Gemini.Video.Models;
using Nayvid.Gemini.Image;
using Nayvid.Gemini.Music;
using Nayvid.Gemini.Speech;
using Nayvid.Gemini.Core;
using Nayvid.Gemini.StructuredOutput;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
var apiKey = builder.Configuration["GEMINI_API_KEY"] ?? "YOUR_API_KEY";

builder.Services.AddSingleton(new GeminiVideoClientOptions { ApiKey = apiKey });
builder.Services.AddSingleton<IGeminiVideoClient, GeminiVideoClient>();
builder.Services.AddSingleton(new GeminiClientOptions { ApiKey = apiKey });
builder.Services.AddSingleton<IImageClient, ImageClient>();
builder.Services.AddSingleton<ISpeechClient, SpeechClient>();
builder.Services.AddSingleton<IMusicClient, MusicClient>();

await builder.Build().RunAsync();
