# Nayvid Gemini SDK (Multi-domain)

This repository provides a modular .NET SDK surface for interacting with Google Gemini style capabilities (video, image, music, embeddings, speech, long-context utilities, structured output). Current implementations are placeholder/fake for rapid iteration – they demonstrate patterns, shapes and testing strategy.

## Projects
- Core: shared primitives (Operation, GeminiError, RetryHelper, options, transport abstraction)
- Video, Image, Music, Embeddings, Speech, LongContext, StructuredOutput domain libraries
- Test projects per domain (xUnit)
- Sample console app demonstrating multi-domain usage

## Getting Started
Install the packages (once published to NuGet):
```
dotnet add package Nayvid.Gemini.Core
 dotnet add package Nayvid.Gemini.Video
# etc.
```

### API Key Configuration
Set an environment variable before running:
```
set GEMINI_API_KEY=your_key_here   # Windows PowerShell: $Env:GEMINI_API_KEY = 'your_key_here'
```
The console sample reads `GEMINI_API_KEY` and falls back to a placeholder. Avoid hard-coding secrets in source. Consider using `dotnet user-secrets` for local dev.

### Retry Guidance
`RetryHelper` centralizes exponential backoff with jitter. Usage pattern for HTTP calls (pseudo):
```
for attempt in 1..maxAttempts:
  response = await transport.SendAsync()
  if success: break
  if !RetryHelper.ShouldRetry(status): throw
  await Task.Delay(RetryHelper.ComputeBackoff(attempt, baseDelay, maxDelay))
```
Future real HTTP implementations should wrap transient (5xx, 429) failures with this logic and surface final errors via `GeminiApiException`.

### Long-running Operations
Video/Image/Music generation return an `Operation`. Use `WaitForCompletionAsync` (pattern demonstrated in video client) or poll manually. `Operation.IsDone` / `IsFailed` convenience flags assist flow control.

### Samples
Samples provided:

| Sample | Description | Run Command |
|--------|-------------|-------------|
| ConsoleDemo | Multi-domain sequential demo | `dotnet run --project samples/Nayvid.ConsoleDemo/ConsoleDemo.csproj` |
| AspNetSample | Minimal API with video/image/speech/music endpoints + Swagger | `dotnet run --project samples/Nayvid.AspNetSample/Nayvid.AspNetSample.csproj` then browse `/swagger` |
| BlazorSample | WebAssembly UI for prompts (video/image/speech) | (Serve with a static host: `dotnet run --project samples/Nayvid.BlazorSample/Nayvid.BlazorSample.csproj`) |
| MauiSample | Cross-platform UI (video/image/speech/music) | `dotnet build samples/Nayvid.MauiSample/Nayvid.MauiSample.csproj` then deploy via platform tooling |

Set environment variable first (PowerShell):
```
$Env:GEMINI_API_KEY = 'your_key'
```
Or Bash:
```
export GEMINI_API_KEY=your_key
```

### Roadmap
- Replace fake stub logic with real REST calls (Video upload/resumable already partially patterned)
- Add streaming APIs for text / media where supported
- Add cancellation + timeout wrappers per operation
- Introduce structured schema enforcement (System.Text.Json source generators or reflection-based validation)
- Provide a meta-package aggregator

### Contributing
1. Fork & create a feature branch
2. Add or update tests (all domains) – fast unit tests required
3. Run CI (build, test, pack) locally before PR
4. Submit PR with concise description

## CI / Packaging Strategy
The GitHub workflow will be enhanced to:
- Detect changed project directories vs main branch
- Only `dotnet pack` those changed domains (plus Core if shared code changed)
- On tagged release or version bump commit, push packages to NuGet

Planned detection script outline (pseudo):
```
changed = git diff --name-only origin/main...HEAD
projects = changed paths mapped to csproj roots
pack list = closure including Core when any domain changes
```

## License
MIT (add LICENSE file).
