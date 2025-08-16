using System;

namespace Nayvid.Gemini.Video.Models
{
    public sealed record VideoPart(string MediaId, TimeSpan? Start = null, TimeSpan? End = null);
}
