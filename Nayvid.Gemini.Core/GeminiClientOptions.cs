using System;
using System.Net.Http;

namespace Nayvid.Gemini.Core
{
    /// <summary>
    /// Shared client options for Gemini domain clients.
    /// </summary>
    public class GeminiClientOptions
    {
        public string BaseUrl { get; set; } = "https://generativelanguage.googleapis.com/v1";
        private string _apiKey = string.Empty;
        /// <summary>API Key. If not explicitly set, resolves from environment variables GEMINI_API_KEY or GOOGLE_GEMINI_API_KEY.</summary>
        public string ApiKey
        {
            get
            {
                if (!string.IsNullOrEmpty(_apiKey)) return _apiKey;
                var fromEnv = Environment.GetEnvironmentVariable("GEMINI_API_KEY")
                              ?? Environment.GetEnvironmentVariable("GOOGLE_GEMINI_API_KEY")
                              ?? Environment.GetEnvironmentVariable("GEMINI_KEY");
                return fromEnv ?? string.Empty;
            }
            set => _apiKey = value ?? string.Empty;
        }
        public HttpClient? HttpClient { get; set; }
        public TimeSpan Timeout { get; set; } = TimeSpan.FromSeconds(100);
        public int MaxRetries { get; set; } = 3;
        public TimeSpan RetryBackoff { get; set; } = TimeSpan.FromMilliseconds(250);
    }
}
