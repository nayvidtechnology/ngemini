using System;

namespace Nayvid.Gemini.Core
{
    public sealed class Operation
    {
        public string Name { get; }
        public string Status { get; }
        public object? Result { get; }
        public GeminiError? Error { get; }
        public Operation(string name, string status, object? result = null, GeminiError? error = null)
        {
            Name = name;
            Status = status;
            Result = result;
            Error = error;
        }
        public bool IsDone => string.Equals(Status, "DONE", StringComparison.OrdinalIgnoreCase);
        public bool IsFailed => string.Equals(Status, "FAILED", StringComparison.OrdinalIgnoreCase);
    }

    public sealed class GeminiError
    {
        public string Code { get; }
        public string Message { get; }
        public object? Details { get; }
        public GeminiError(string code, string message, object? details = null)
        {
            Code = code;
            Message = message;
            Details = details;
        }
    }

    public static class RetryHelper
    {
    private static readonly Random _rng = new Random();
        public static TimeSpan ComputeBackoff(int attempt, TimeSpan baseDelay, TimeSpan? max = null)
        {
            var exp = Math.Min(attempt, 10);
            double factor = Math.Pow(2, exp - 1);
            var delay = TimeSpan.FromMilliseconds(baseDelay.TotalMilliseconds * factor);
            var jitter = TimeSpan.FromMilliseconds(_rng.Next(25, 125));
            var total = delay + jitter;
            if (max.HasValue && total > max.Value) return max.Value;
            return total;
        }
        public static bool ShouldRetry(int statusCode) => statusCode == 429 || (statusCode >= 500 && statusCode < 600);
    }
}
