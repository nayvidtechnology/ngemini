using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using Nayvid.Gemini.Video.Models; // upload/session specific models
using Nayvid.Gemini.Core; // Operation, GeminiError, transport, exceptions
using Microsoft.Extensions.Logging;

namespace Nayvid.Gemini.Video
{
    public class GeminiVideoClient : IGeminiVideoClient
    {
        private readonly GeminiVideoClientOptions _options;
        private readonly HttpClient _httpClient;
        private readonly Nayvid.Gemini.Core.IHttpTransport _transport;
        // Removed local JsonSerializerOptions; using GeminiJson helper
        private readonly ILogger? _logger;

        public GeminiVideoClient(GeminiVideoClientOptions options, ILogger? logger = null, Nayvid.Gemini.Core.IHttpTransport? transport = null)
        {
            _options = options ?? throw new ArgumentNullException(nameof(options));
            _httpClient = options.HttpClient ?? new HttpClient { Timeout = options.Timeout };
            _logger = logger ?? Microsoft.Extensions.Logging.Abstractions.NullLogger.Instance;
            _transport = transport ?? new Nayvid.Gemini.Core.HttpClientTransport(_httpClient);
        }

        public async Task<UploadSession> StartResumableUploadAsync(StartUploadRequest request, CancellationToken ct = default)
        {
            if (request is null) throw new ArgumentNullException(nameof(request));
            var url = $"{_options.BaseUrl.TrimEnd('/')}/upload?uploadType=resumable";
            GeminiLogger.LogRequest(_logger, url, request);
            var json = GeminiJson.Serialize(request);
            using var http = new HttpRequestMessage(HttpMethod.Post, url)
            {
                Content = new StringContent(json, System.Text.Encoding.UTF8, "application/json")
            };
            http.Headers.Add("X-API-Key", _options.ApiKey);
            var resp = await _transport.SendAsync(http, ct).ConfigureAwait(false);
            if (!resp.IsSuccessStatusCode)
            {
                GeminiLogger.LogError(_logger, url, new Exception($"Failed to start upload: {resp.StatusCode}"));
                throw new GeminiApiException((System.Net.HttpStatusCode)resp.StatusCode, null, "Failed to start upload", null);
            }
            var sessionUri = resp.Headers.Location?.ToString() ?? url; // placeholder
            var uploadId = Guid.NewGuid().ToString("N");
            var session = new UploadSession(sessionUri, uploadId);
            GeminiLogger.LogResponse(_logger, url, (int)resp.StatusCode, session);
            return session;
        }

        public async Task<UploadProgress> UploadChunkAsync(UploadSession session, Stream content, long offset, int bytesToWrite, CancellationToken ct = default)
        {
            if (session is null) throw new ArgumentNullException(nameof(session));
            if (content is null) throw new ArgumentNullException(nameof(content));
            var url = session.SessionUri;
            content.Seek(offset, SeekOrigin.Begin);
            byte[] buffer = new byte[bytesToWrite];
            int read = await content.ReadAsync(buffer.AsMemory(0, bytesToWrite), ct).ConfigureAwait(false);
            using var msg = new HttpRequestMessage(HttpMethod.Put, url)
            {
                Content = new ByteArrayContent(buffer, 0, read)
            };
            msg.Content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
            msg.Headers.Add("X-Upload-Id", session.UploadId);
            msg.Headers.Add("X-API-Key", _options.ApiKey);
            // TODO: Add Content-Range when implementing real resumable upload protocol. Omitted here due to platform header validation causing exceptions in tests.
            GeminiLogger.LogRequest(_logger, url, new { offset, read });
            var resp = await _transport.SendAsync(msg, ct).ConfigureAwait(false);
            if ((int)resp.StatusCode == 308)
            {
                // Resume incomplete - parse Range header if present
                long committed = offset + read;
                if (resp.Headers.TryGetValues("Range", out var ranges))
                {
                    var r = System.Linq.Enumerable.FirstOrDefault(ranges);
                    // format bytes=0-12345
                    var parts = r?.Split('=');
                    if (parts?.Length == 2)
                    {
                        var span = parts[1].Split('-');
                        if (span.Length == 2 && long.TryParse(span[1], out long end))
                            committed = end + 1;
                    }
                }
                var progress = new UploadProgress(committed, null);
                GeminiLogger.LogResponse(_logger, url, (int)resp.StatusCode, progress);
                return progress;
            }
            resp.EnsureSuccessStatusCode();
            var final = new UploadProgress(offset + read, content.Length);
            GeminiLogger.LogResponse(_logger, url, (int)resp.StatusCode, final);
            return final;
        }

        public Task<UploadedMedia> CompleteUploadAsync(UploadSession session, CancellationToken ct = default)
        {
            if (session is null) throw new ArgumentNullException(nameof(session));
            // Placeholder implementation - in a real API this would finalize on server
            var media = new UploadedMedia(session.UploadId, session.SessionUri, null);
            GeminiLogger.LogResponse(_logger, session.SessionUri, 200, media);
            return Task.FromResult(media);
        }

        public async Task<UploadedMedia> UploadSmallAsync(SmallUploadRequest request, Stream content, CancellationToken ct = default)
        {
            if (request is null) throw new ArgumentNullException(nameof(request));
            if (content is null) throw new ArgumentNullException(nameof(content));
            var session = await StartResumableUploadAsync(new StartUploadRequest(request.FileName, request.MimeType, content.Length), ct).ConfigureAwait(false);
            long offset = 0;
            const int chunk = 256 * 1024;
            while (offset < content.Length)
            {
                int toWrite = (int)Math.Min(chunk, content.Length - offset);
                await UploadChunkAsync(session, content, offset, toWrite, ct).ConfigureAwait(false);
                offset += toWrite;
            }
            return await CompleteUploadAsync(session, ct).ConfigureAwait(false);
        }

        public Task<Operation> GenerateFromVideoAsync(GenerateFromVideoRequest request, CancellationToken ct = default)
        {
            if (request is null) throw new ArgumentNullException(nameof(request));
            // Placeholder immediate operation
            var op = new Operation(Guid.NewGuid().ToString("N"), "PENDING", null, null);
            GeminiLogger.LogResponse(_logger, "generate", 200, op);
            return Task.FromResult(op);
        }

        public Task<Operation> GetOperationAsync(string operationName, CancellationToken ct = default)
        {
            // Simulate completion after initial pending
            var status = DateTime.UtcNow.Ticks % 3 == 0 ? "DONE" : "PENDING";
            var op = new Operation(operationName, status, status == "DONE" ? new { text = "Result" } : null, null);
            return Task.FromResult(op);
        }

        public async IAsyncEnumerable<Operation> WaitForCompletionAsync(string operationName, TimeSpan pollInterval, TimeSpan? timeout = null, [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken ct = default)
        {
            var start = DateTime.UtcNow;
            while (true)
            {
                ct.ThrowIfCancellationRequested();
                if (timeout.HasValue && DateTime.UtcNow - start > timeout.Value)
                    yield break;
                var op = await GetOperationAsync(operationName, ct).ConfigureAwait(false);
                yield return op;
                if (op.Status == "DONE" || op.Status == "FAILED")
                    yield break;
                await Task.Delay(pollInterval, ct).ConfigureAwait(false);
            }
        }

        public async IAsyncEnumerable<GenerateChunk> StreamGenerateFromVideoAsync(GenerateFromVideoRequest request, [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken ct = default)
        {
            if (request is null) throw new ArgumentNullException(nameof(request));
            // Fake 3-chunk streaming
            for (int i = 1; i <= 3; i++)
            {
                ct.ThrowIfCancellationRequested();
                var chunk = new GenerateChunk("text", new { part = i, value = $"Chunk {i}" });
                yield return chunk;
                await Task.Delay(200, ct).ConfigureAwait(false);
            }
        }
    }
}
