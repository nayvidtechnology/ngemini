using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Nayvid.Gemini.Core;

namespace Nayvid.Gemini.LongContext
{
    public interface ILongContextUtilities
    {
        IEnumerable<string> Chunk(string text, int maxChars);
        Task<string> SummarizeAsync(string model, IEnumerable<string> chunks, CancellationToken ct = default);
    }
    public class LongContextClient : ILongContextUtilities
    {
        public IEnumerable<string> Chunk(string text, int maxChars)
        {
            if (string.IsNullOrEmpty(text)) yield break;
            for (int i = 0; i < text.Length; i += maxChars)
                yield return text.Substring(i, Math.Min(maxChars, text.Length - i));
        }
        public Task<string> SummarizeAsync(string model, IEnumerable<string> chunks, CancellationToken ct = default)
        {
            var summary = string.Join(" ", chunks);
            if (summary.Length > 120) summary = summary[..120] + "...";
            return Task.FromResult($"Summary[{model}]: {summary}");
        }
    }
}
