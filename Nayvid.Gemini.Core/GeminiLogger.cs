using System;
using Microsoft.Extensions.Logging;

namespace Nayvid.Gemini.Core
{
    public static class GeminiLogger
    {
        public static void LogRequest(ILogger? logger, string url, object? payload)
        {
            logger?.LogInformation("[1000] Gemini request: {Url} Payload: {Payload}", url, Redact(payload));
        }
        public static void LogResponse(ILogger? logger, string url, int statusCode, object? payload)
        {
            logger?.LogInformation("[1001] Gemini response: {Url} Status: {StatusCode} Payload: {Payload}", url, statusCode, Redact(payload));
        }
        public static void LogError(ILogger? logger, string url, Exception ex)
        {
            logger?.LogError(ex, "[1099] Gemini error: {Url}", url);
        }
        private static object? Redact(object? payload)
        {
            // Redact secrets from payload
            return payload; // TODO: implement redaction
        }
    }
}
