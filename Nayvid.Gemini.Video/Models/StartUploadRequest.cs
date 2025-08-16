namespace Nayvid.Gemini.Video.Models
{
    public sealed record StartUploadRequest(string FileName, string? MimeType, long? SizeBytes);
}
