namespace Nayvid.Gemini.Video.Models
{
    public sealed record GeminiError(string Code, string Message, object? Details);
}
