using System;
using System.Net.Http;

namespace Nayvid.Gemini.Video
{
    public class GeminiVideoClientOptions
    {
        public string BaseUrl { get; set; } = "https://generativelanguage.googleapis.com/v1";
        public string ApiKey { get; set; } = string.Empty;
        public HttpClient? HttpClient { get; set; }
        public TimeSpan Timeout { get; set; } = TimeSpan.FromSeconds(100);
        public int MaxRetries { get; set; } = 5;
        public TimeSpan RetryBackoff { get; set; } = TimeSpan.FromSeconds(2);
    }
}
