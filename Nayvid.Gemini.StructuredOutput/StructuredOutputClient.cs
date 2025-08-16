using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Nayvid.Gemini.Core;

namespace Nayvid.Gemini.StructuredOutput
{
    public interface IStructuredOutputClient
    {
        Task<T?> GenerateStructuredAsync<T>(string model, string prompt, CancellationToken ct = default);
    }
    public class StructuredOutputClient : IStructuredOutputClient
    {
        private readonly GeminiClientOptions _options;
        private readonly IHttpTransport _transport;
        public StructuredOutputClient(GeminiClientOptions options, IHttpTransport? transport = null)
        {
            _options = options ?? throw new ArgumentNullException(nameof(options));
            var http = options.HttpClient ?? new HttpClient { Timeout = options.Timeout };
            _transport = transport ?? new HttpClientTransport(http);
        }
        public Task<T?> GenerateStructuredAsync<T>(string model, string prompt, CancellationToken ct = default)
        {
            var json = GeminiJson.Serialize(new { model, prompt });
            var result = GeminiJson.Deserialize<T>(json);
            return Task.FromResult(result);
        }
    }
}
