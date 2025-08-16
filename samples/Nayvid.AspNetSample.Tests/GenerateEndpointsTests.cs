using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Xunit;
using Microsoft.AspNetCore.Mvc.Testing;

public class GenerateEndpointsTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;
    public GenerateEndpointsTests(WebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Health_ReturnsOk()
    {
        var resp = await _client.GetAsync("/health");
        resp.EnsureSuccessStatusCode();
    }

    [Fact]
    public async Task Version_ReturnsVersionAndDomains()
    {
        var resp = await _client.GetAsync("/version");
        resp.EnsureSuccessStatusCode();
        var json = await resp.Content.ReadFromJsonAsync<dynamic>();
        Assert.NotNull(json);
    }

    [Fact]
    public async Task GenerateImage_ReturnsOperation()
    {
        var resp = await _client.PostAsJsonAsync("/generate/image", new { text = "A tree", model = "image-model" });
        resp.EnsureSuccessStatusCode();
        var json = await resp.Content.ReadFromJsonAsync<dynamic>();
        Assert.NotNull(json);
    }

    [Fact]
    public async Task GenerateVideo_ReturnsOperation()
    {
        var resp = await _client.PostAsJsonAsync("/generate/video", new { text = "Describe sample", model = "gemini-pro-video" });
        resp.EnsureSuccessStatusCode();
        var json = await resp.Content.ReadFromJsonAsync<dynamic>();
        Assert.NotNull(json);
    }

    [Fact]
    public async Task GenerateSpeech_ReturnsAudio()
    {
        var resp = await _client.PostAsJsonAsync("/generate/speech", new { text = "Hello test", model = "speech-model", voice = "default" });
        resp.EnsureSuccessStatusCode();
        Assert.Equal("audio/wav", resp.Content.Headers.ContentType?.MediaType);
        var bytes = await resp.Content.ReadAsByteArrayAsync();
        Assert.True(bytes.Length > 0);
    }

    [Fact]
    public async Task GenerateMusic_ReturnsOperation()
    {
        var resp = await _client.PostAsJsonAsync("/generate/music", new { text = "Calm theme", model = "music-model" });
        resp.EnsureSuccessStatusCode();
        var json = await resp.Content.ReadFromJsonAsync<dynamic>();
        Assert.NotNull(json);
    }

    [Fact]
    public async Task GenerateImage_EmptyText_ReturnsBadRequest()
    {
        var resp = await _client.PostAsJsonAsync("/generate/image", new { text = "" });
        Assert.Equal(System.Net.HttpStatusCode.BadRequest, resp.StatusCode);
    }

    [Fact]
    public async Task GenerateVideo_EmptyText_ReturnsBadRequest()
    {
        var resp = await _client.PostAsJsonAsync("/generate/video", new { text = "" });
        Assert.Equal(System.Net.HttpStatusCode.BadRequest, resp.StatusCode);
    }
}
