using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Nayvid.Gemini.Core
{
    /// <summary>
    /// Default transport wrapper over HttpClient implementing <see cref="IHttpTransport"/>.
    /// </summary>
    public sealed class HttpClientTransport : IHttpTransport
    {
        private readonly HttpClient _client;
        public HttpClientTransport(HttpClient client) => _client = client;
        public Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken = default)
            => _client.SendAsync(request, cancellationToken);
    }
}
