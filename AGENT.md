# AGENT.md

## Purpose
Build a clean, production-ready **.NET client library** for the **Google Gemini Video API (REST)** so application developers can:
- Upload video (direct or resumable), attach prompts, and request model outputs.
- Poll and retrieve operation results.
- Stream or page large responses where supported.
- Handle auth, errors, retries, and timeouts predictably.

The library MUST be idiomatic C#, easy to test, and NuGet-publishable.

> **Docs reference (for humans):** Use the public Gemini API video docs as the ground truth for endpoint paths, request/response shapes, and long-running operations semantics.

## Supported Runtimes & Packaging
- **Target Frameworks:** `net8.0` (primary), `netstandard2.1` (if feasible).
- **Package Id (example):** `Acme.Gemini.Video`
- **Namespaces:** `Acme.Gemini.Video` and sub-namespaces `.Models`, `.Internal`

## Library Design Rules
1. **Abstractions**
   - Single entry point: `IGeminiVideoClient` with `GeminiVideoClient` implementation.
   - Hide HTTP behind `IHttpTransport` (default uses `HttpClient`).
   - Async-only methods with `Async` suffix.
   - POCO request/response models; avoid `dynamic`.

2. **Configuration**
   - `GeminiVideoClientOptions`:
     - `BaseUrl` (default to current REST base),
     - `ApiKey`,
     - `HttpClient` (optional injection),
     - `Timeout`, `MaxRetries`, `RetryBackoff`.
   - DI-friendly patterns (`IOptions<T>` happy path).

3. **Auth**
   - API key via header or query per docs; prefer header by default.
   - Extensible `IAccessTokenProvider` for future OAuth.

4. **Resilience**
   - Retries on 429/5xx with jittered exponential backoff.
   - Per-request cancellation via `CancellationToken`.
   - Bubble up rate-limit headers where present.

5. **Errors**
   - Non-2xx → `GeminiApiException` containing `StatusCode`, `ErrorCode`, `ErrorMessage`, raw payload.

6. **Long-Running Operations**
   - Return an operation handle model. Provide:
     - `GetOperationAsync(opName)`
     - `WaitForCompletionAsync(opName, pollInterval, timeout)`

7. **Uploads**
   - Support direct multipart (small) and resumable (large): start → chunk PUTs (`Content-Range`) → finalize.
   - Pluggable `IStreamChunker` for file/stream sources.

8. **Streaming/Chunked Responses**
   - If API supports streamed output, expose `IAsyncEnumerable<ResponseChunk>` with clean cancellation.

9. **Validation & UX**
   - Validate required fields before HTTP call.
   - XML docs for all public APIs with minimal `<example>`s.

10. **Versioning**
   - SemVer. Breaking changes require major bump and `[Obsolete]` path first.

## Public API Sketch
```csharp
namespace Acme.Gemini.Video;

public interface IGeminiVideoClient
{
    // Uploads
    Task<UploadSession> StartResumableUploadAsync(StartUploadRequest request, CancellationToken ct = default);
    Task<UploadProgress> UploadChunkAsync(UploadSession session, Stream content, long offset, int bytesToWrite, CancellationToken ct = default);
    Task<UploadedMedia> CompleteUploadAsync(UploadSession session, CancellationToken ct = default);
    Task<UploadedMedia> UploadSmallAsync(SmallUploadRequest request, Stream content, CancellationToken ct = default);

    // Inference / Generate
    Task<Operation> GenerateFromVideoAsync(GenerateFromVideoRequest request, CancellationToken ct = default);

    // Operations
    Task<Operation> GetOperationAsync(string operationName, CancellationToken ct = default);
    IAsyncEnumerable<Operation> WaitForCompletionAsync(string operationName, TimeSpan pollInterval, TimeSpan? timeout = null, CancellationToken ct = default);

    // Streaming output (if supported)
    IAsyncEnumerable<GenerateChunk> StreamGenerateFromVideoAsync(GenerateFromVideoRequest request, CancellationToken ct = default);
}

```
## Models (illustrative; align to actual schema)

```csharp
namespace Acme.Gemini.Video.Models;

public sealed record StartUploadRequest(string FileName, string? MimeType, long? SizeBytes);
public sealed record SmallUploadRequest(string FileName, string? MimeType);
public sealed record UploadSession(string SessionUri, string UploadId);
public sealed record UploadProgress(long BytesCommitted, long? TotalBytes);
public sealed record UploadedMedia(string MediaId, string Uri, string? Md5);
public sealed record VideoPart(string MediaId, TimeSpan? Start = null, TimeSpan? End = null);

public sealed record GenerateFromVideoRequest(
    string Model,
    VideoPart[] Video,
    string Prompt,
    IDictionary<string, string>? Parameters = null);

public sealed record Operation(string Name, string Status, object? Result = null, GeminiError? Error = null);
public sealed record GeminiError(string Code, string Message, object? Details);
public sealed record GenerateChunk(string Kind, object Payload);


```
**HTTP Surface (fill exact paths per docs)**

Base URL: Gemini REST base.

Auth: API key via header or query; prefer header.

Endpoints (examples; replace with exact):

POST /videos:upload (start resumable / small negotiation)

PUT {uploadSessionUri} with Content-Range for chunks

POST /models/{model}:generateVideo (or documented equivalent)

GET /operations/{name}

Conventions: JSON; Accept: application/json. Resumable: handle 308 Resume Incomplete.

Error Handling & Retries

Retry on 429, 500, 502, 503, 504. Respect Retry-After. Abort on other 4xx except safe cases (408, 409).

**Streaming Guidance**

Use IAsyncEnumerable<T>; parse SSE/chunked frames; support cancellation.

**Logging & Telemetry**

ILogger<GeminiVideoClient> with event IDs:

1000 request start, 1001 response, 1099 error.

Redact secrets; optional OpenTelemetry ActivitySource.

**Testing**

80%+ coverage core paths.

Unit tests with fake HttpMessageHandler.

Integration tests gated by GEMINI_API_KEY.

**CI & Quality**

Build: dotnet build -c Release

Tests: dotnet test --collect:"XPlat Code Coverage"

Analyzers: Microsoft.CodeAnalysis.NetAnalyzers, StyleCop.Analyzers; treat warnings as errors in CI.
\n### API Key Handling\nClients resolve API key from environment variables in order: `GEMINI_API_KEY`, `GOOGLE_GEMINI_API_KEY`, `GEMINI_KEY` if `GeminiClientOptions.ApiKey` not set. See `APIKEYS.md`.\n\n### Retry Strategy\n`RetryHelper` (Core) applies exponential backoff (base 250ms, jitter 25-125ms) up to `MaxRetries` (default 3) for status 429 and 5xx. Customize via `GeminiClientOptions`.\n\n### CI Packaging\nWorkflow packs only changed projects (plus Core). Publishing occurs on `main` commits whose message starts with `release:` using `NUGET_API_KEY` secret. Tests have access to `GEMINI_API_KEY` secret if configured.\n*** End Patch