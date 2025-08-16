using System.Collections.Generic;

namespace Nayvid.Gemini.Video.Models
{
    public sealed record GenerateFromVideoRequest(
        string Model,
        VideoPart[] Video,
        string Prompt,
        IDictionary<string, string>? Parameters = null);
}
