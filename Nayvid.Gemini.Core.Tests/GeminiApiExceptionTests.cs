using System.Net;
using Nayvid.Gemini.Core;
using Xunit;

namespace Nayvid.Gemini.Core.Tests
{
    public class GeminiApiExceptionTests
    {
        [Fact]
        public void Constructor_SetsProperties()
        {
            var ex = new GeminiApiException(HttpStatusCode.BadRequest, "ERR", "Error occurred", "raw");
            Assert.Equal(HttpStatusCode.BadRequest, ex.StatusCode);
            Assert.Equal("ERR", ex.ErrorCode);
            Assert.Equal("Error occurred", ex.ErrorMessage);
            Assert.Equal("raw", ex.RawPayload);
        }
    }
}
