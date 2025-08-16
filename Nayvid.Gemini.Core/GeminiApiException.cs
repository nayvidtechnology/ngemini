using System;
using System.Net;

namespace Nayvid.Gemini.Core
{
    public class GeminiApiException : Exception
    {
        public HttpStatusCode StatusCode { get; }
        public string? ErrorCode { get; }
        public string? ErrorMessage { get; }
        public string? RawPayload { get; }

        public GeminiApiException(HttpStatusCode statusCode, string? errorCode, string? errorMessage, string? rawPayload)
            : base($"Gemini API error: {errorCode} {errorMessage}")
        {
            StatusCode = statusCode;
            ErrorCode = errorCode;
            ErrorMessage = errorMessage;
            RawPayload = rawPayload;
        }
    }
}
