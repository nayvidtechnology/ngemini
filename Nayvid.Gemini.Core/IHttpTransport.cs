using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Nayvid.Gemini.Core
{
    public interface IHttpTransport
    {
        Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken = default);
    }
}
