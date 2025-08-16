namespace Nayvid.Gemini.Video.Models
{
    public sealed record UploadProgress(long BytesCommitted, long? TotalBytes);
}
