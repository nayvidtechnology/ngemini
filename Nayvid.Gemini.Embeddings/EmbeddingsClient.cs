using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Nayvid.Gemini.Core;

namespace Nayvid.Gemini.Embeddings
{
    public interface IEmbeddingsClient
    {
        Task<float[]> EmbedAsync(string model, string text, CancellationToken ct = default);
    }
    public class EmbeddingsClient : IEmbeddingsClient
    {
        private readonly GeminiClientOptions _options;
        private readonly IHttpTransport _transport;
        public EmbeddingsClient(GeminiClientOptions options, IHttpTransport? transport = null)
        {
            _options = options ?? throw new ArgumentNullException(nameof(options));
            var http = options.HttpClient ?? new HttpClient { Timeout = options.Timeout };
            _transport = transport ?? new HttpClientTransport(http);
        }
        public Task<float[]> EmbedAsync(string model, string text, CancellationToken ct = default)
        {
            var hash = text.GetHashCode();
            var vec = new float[8];
            var r = new System.Random(hash);
            for (int i = 0; i < vec.Length; i++) vec[i] = (float)r.NextDouble();
            return Task.FromResult(vec);
        }
    }
}
