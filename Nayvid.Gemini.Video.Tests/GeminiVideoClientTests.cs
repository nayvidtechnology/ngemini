using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Nayvid.Gemini.Video;
using Nayvid.Gemini.Video.Models;
using Xunit;

namespace Nayvid.Gemini.Video.Tests
{
    public class GeminiVideoClientTests
    {
        [Fact]
        public async Task UploadChunkAsync_HandlesResumeIncomplete_308()
        {
            var handler = new StubHandler((req, ct) =>
            {
                var resp = new HttpResponseMessage((HttpStatusCode)308);
                resp.Headers.Add("Range", "bytes=0-1023");
                return Task.FromResult(resp);
            });
            var client = new GeminiVideoClient(new GeminiVideoClientOptions { ApiKey = "test", HttpClient = new HttpClient(handler) });
            var session = new UploadSession("http://upload", "id");
            using var stream = new MemoryStream(new byte[1024]);
            var progress = await client.UploadChunkAsync(session, stream, 0, 1024);
            Assert.Equal(1024, progress.BytesCommitted);
        }

        private class StubHandler : HttpMessageHandler
        {
            private readonly Func<HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>> _handler;
            public StubHandler(Func<HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>> handler) => _handler = handler;
            protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken) => _handler(request, cancellationToken);
        }
    }
}
