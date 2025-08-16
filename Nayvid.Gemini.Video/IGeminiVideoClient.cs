using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Nayvid.Gemini.Video.Models;
using Nayvid.Gemini.Core;

namespace Nayvid.Gemini.Video
{
    public interface IGeminiVideoClient
    {
        Task<UploadSession> StartResumableUploadAsync(StartUploadRequest request, CancellationToken ct = default);
        Task<UploadProgress> UploadChunkAsync(UploadSession session, Stream content, long offset, int bytesToWrite, CancellationToken ct = default);
        Task<UploadedMedia> CompleteUploadAsync(UploadSession session, CancellationToken ct = default);
        Task<UploadedMedia> UploadSmallAsync(SmallUploadRequest request, Stream content, CancellationToken ct = default);
    Task<Operation> GenerateFromVideoAsync(GenerateFromVideoRequest request, CancellationToken ct = default);
    Task<Operation> GetOperationAsync(string operationName, CancellationToken ct = default);
    IAsyncEnumerable<Operation> WaitForCompletionAsync(string operationName, System.TimeSpan pollInterval, System.TimeSpan? timeout = null, CancellationToken ct = default);
        IAsyncEnumerable<GenerateChunk> StreamGenerateFromVideoAsync(GenerateFromVideoRequest request, CancellationToken ct = default);
    }
}
