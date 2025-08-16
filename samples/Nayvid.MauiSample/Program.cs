// Simplified console-style placeholder for a future .NET MAUI UI app.
using Nayvid.Gemini.Video;
using Nayvid.Gemini.Video.Models;
using Nayvid.Gemini.Image;
using Nayvid.Gemini.Music;
using Nayvid.Gemini.Speech;
using Nayvid.Gemini.Core;

var apiKey = Environment.GetEnvironmentVariable("GEMINI_API_KEY") ?? "YOUR_API_KEY";
Console.WriteLine("MAUI placeholder sample - using API key length: " + apiKey.Length);

var video = new GeminiVideoClient(new GeminiVideoClientOptions { ApiKey = apiKey });
var start = await video.StartResumableUploadAsync(new StartUploadRequest("placeholder.mp4","video/mp4",0));
var media = await video.CompleteUploadAsync(start);
var op = await video.GenerateFromVideoAsync(new GenerateFromVideoRequest("gemini-pro-video", new[] { new VideoPart(media.MediaId) }, "Describe this video"));
Console.WriteLine($"Video op: {op.Name} status {op.Status}");

var imageClient = new ImageClient(new GeminiClientOptions { ApiKey = apiKey });
var imgOp = await imageClient.GenerateImageAsync("image-model", "A lake at dawn");
Console.WriteLine($"Image op: {imgOp.Name} status {imgOp.Status}");

var musicClient = new MusicClient(new GeminiClientOptions { ApiKey = apiKey });
var musicOp = await musicClient.GenerateMusicAsync("music-model", "Ambient pad");
Console.WriteLine($"Music op: {musicOp.Name} status {musicOp.Status}");

var speechClient = new SpeechClient(new GeminiClientOptions { ApiKey = apiKey });
var audio = await speechClient.TextToSpeechAsync("speech-model", "Welcome to the MAUI sample");
Console.WriteLine($"Speech bytes: {audio.Length}");
