using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Nayvid.Gemini.Core;

namespace Nayvid.Gemini.Speech
{
    public interface ISpeechClient
    {
        Task<byte[]> TextToSpeechAsync(string model, string text, string voice = "default", CancellationToken ct = default);
        Task<string> SpeechToTextAsync(string model, byte[] audio, string mimeType = "audio/wav", CancellationToken ct = default);
    }
    public class SpeechClient : ISpeechClient
    {
        private readonly GeminiClientOptions _options;
        private readonly IHttpTransport _transport;
        public SpeechClient(GeminiClientOptions options, IHttpTransport? transport = null)
        {
            _options = options ?? throw new ArgumentNullException(nameof(options));
            var http = options.HttpClient ?? new HttpClient { Timeout = options.Timeout };
            _transport = transport ?? new HttpClientTransport(http);
        }
        public Task<byte[]> TextToSpeechAsync(string model, string text, string voice = "default", CancellationToken ct = default)
        {
            var bytes = System.Text.Encoding.UTF8.GetBytes($"FAKE-AUDIO:{model}:{voice}:{text}");
            return Task.FromResult(bytes);
        }
        public Task<string> SpeechToTextAsync(string model, byte[] audio, string mimeType = "audio/wav", CancellationToken ct = default)
        {
            var txt = $"Transcript({model},{mimeType}):{audio.Length}bytes";
            return Task.FromResult(txt);
        }
    }
}
