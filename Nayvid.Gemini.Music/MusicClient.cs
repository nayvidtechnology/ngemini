using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Nayvid.Gemini.Core;

namespace Nayvid.Gemini.Music
{
    public interface IMusicClient
    {
        Task<Operation> GenerateMusicAsync(string model, string prompt, CancellationToken ct = default);
        IAsyncEnumerable<Operation> WaitForCompletionAsync(string operationName, TimeSpan poll, TimeSpan? timeout = null, CancellationToken ct = default);
    }
    public class MusicClient : IMusicClient
    {
        private readonly GeminiClientOptions _options;
        private readonly IHttpTransport _transport;
        public MusicClient(GeminiClientOptions options, IHttpTransport? transport = null)
        {
            _options = options ?? throw new ArgumentNullException(nameof(options));
            var http = options.HttpClient ?? new HttpClient { Timeout = options.Timeout };
            _transport = transport ?? new HttpClientTransport(http);
        }
        public Task<Operation> GenerateMusicAsync(string model, string prompt, CancellationToken ct = default)
        {
            return Task.FromResult(new Operation(Guid.NewGuid().ToString("N"), "PENDING", new { model, prompt }));
        }
        public async IAsyncEnumerable<Operation> WaitForCompletionAsync(string operationName, TimeSpan poll, TimeSpan? timeout = null, [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken ct = default)
        {
            var start = DateTime.UtcNow;
            while (true)
            {
                ct.ThrowIfCancellationRequested();
                if (timeout.HasValue && DateTime.UtcNow - start > timeout) yield break;
                var done = DateTime.UtcNow.Second % 7 == 0;
                var op = new Operation(operationName, done ? "DONE" : "PENDING", done ? new { audioUri = "gs://fake-audio" } : null);
                yield return op;
                if (op.IsDone) yield break;
                await Task.Delay(poll, ct).ConfigureAwait(false);
            }
        }
    }
}
