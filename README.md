# ngemini – Nayvid Gemini SDK (Multi‑domain)

> Status: Experimental / pre-release. Core shapes are stabilizing; transports and real HTTP integrations are still stubs.

![CI](https://github.com/nayvidtechnology/ngemini/actions/workflows/ci.yml/badge.svg)

This repository provides a modular .NET 9 SDK surface for interacting with Google Gemini style capabilities:

Domains: Video, Image, Music, Embeddings, Speech, Long‑Context utilities, Structured Output.

Current implementations simulate responses (no real network) except the dedicated real samples which call public endpoints directly. This lets you explore client patterns, retry logic shaping, long‑running operation polling, and test strategy ahead of wiring real REST calls.

## Project Layout
| Folder | Purpose |
|--------|---------|
| `Nayvid.Gemini.Core` | Core primitives (Operation, exceptions, JSON helpers, transport abstraction) |
| `Nayvid.Gemini.*` | Domain clients (video, image, speech, music, embeddings, structured, long context) |
| `*.Tests` | xUnit unit tests per domain |
| `samples/` | End‑to‑end sample apps (console, API, Blazor, MAUI, real video/API) |
| `.github/workflows/ci.yml` | Build, test, selective pack workflow |

## Quick Start
Add packages (after publishing):
```powershell
dotnet add package Nayvid.Gemini.Core
dotnet add package Nayvid.Gemini.Video
# etc
```

Set your API key (PowerShell):
```powershell
$Env:GEMINI_API_KEY = 'your_real_key'
```
Or Bash:
```bash
export GEMINI_API_KEY=your_real_key
```

Run a sample:
```powershell
dotnet run --project samples/Nayvid.ConsoleDemo/ConsoleDemo.csproj
```

## Samples Matrix
| Sample | Description | Command |
|--------|-------------|---------|
| ConsoleDemo | Sequential multi‑domain demo (mock transport fallback) | `dotnet run --project samples/Nayvid.ConsoleDemo/ConsoleDemo.csproj` |
| AspNetSample | Minimal API (Swagger + video/image/speech/music endpoints) | `dotnet run --project samples/Nayvid.AspNetSample/Nayvid.AspNetSample.csproj` → browse `/swagger` |
| BlazorSample | WASM UI calling domain clients in-browser | `dotnet run --project samples/Nayvid.BlazorSample/Nayvid.BlazorSample.csproj` |
| MauiSample | Cross‑platform (Windows/Android/iOS) UI | `dotnet build samples/Nayvid.MauiSample/Nayvid.MauiSample.csproj` |
| RealApiSample | Uses real API key path for image/speech (placeholder logic inside) | `dotnet run --project samples/Nayvid.RealApiSample/Nayvid.RealApiSample.csproj` |
| RealVideoSample | Direct long‑running video generation via `veo-3.0-generate-preview` REST | `dotnet run --project samples/Nayvid.RealVideoSample/Nayvid.RealVideoSample.csproj -- "Prompt"` |

## Long‑Running Operations
Video (and some image/music scenarios) return an `Operation`. Poll with `WaitForCompletionAsync` or explicit GET loop. Sample pattern:
```csharp
await foreach (var op in client.WaitForCompletionAsync(operationName, TimeSpan.FromSeconds(2))) {
    if (op.Status == "DONE") { /* use op.Result */ }
}
```

## Retry Strategy (Planned)
Transient 5xx / 429 responses will use exponential backoff w/ jitter. Pseudocode:
```text
for attempt in 1..N
  send
  if success break
  if !ShouldRetry(status) throw
  await Delay(ComputeBackoff(attempt))
```

## Development Workflow
1. Create feature branch
2. Implement + add/update unit tests
3. `dotnet test` (all green) & run samples
4. Submit PR – CI builds, runs tests, identifies changed projects, packs only what changed

## Selective Packing
The CI script diffs against `origin/main` to compute affected project list and always adds `Core` if any domain changed. Packed `.nupkg` files are uploaded as artifacts; publishing requires environment approval + API keys.

## Real Video Sample
`samples/Nayvid.RealVideoSample` illustrates direct calling of the `predictLongRunning` endpoint for model `veo-3.0-generate-preview` and polls until `done` then downloads the MP4.

## Roadmap
- Implement real HTTP transport + auth (bearer / API key) per domain
- Streaming generation APIs
- Structured schema enforcement (STJ source generators)
- Cancellation/timeouts wrapping each call
- NuGet meta-package aggregator
- Rich telemetry (OpenTelemetry instrumentation hooks)

## Security / Keys
Never commit real API keys. Use environment variables or `dotnet user-secrets`. CI expects `GEMINI_API_KEY` only for integration tests (optional paths skip if absent).

## Contributing
PRs welcome after opening an issue for large changes. Keep tests fast & deterministic.

## License
MIT (LICENSE to be added).
