using System;
using System.IO;
using System.Threading.Tasks;
using Acme.Gemini.Video;
using Acme.Gemini.Video.Models;

class Program
{
    static async Task Main(string[] args)
    {
        var options = new GeminiVideoClientOptions
        {
            ApiKey = "YOUR_API_KEY"
        };
        var client = new GeminiVideoClient(options);
        using var stream = File.OpenRead("sample.mp4");
        var startReq = new StartUploadRequest("sample.mp4", "video/mp4", stream.Length);
        var session = await client.StartResumableUploadAsync(startReq);
        long offset = 0;
        int chunkSize = 1024 * 1024;
        while (offset < stream.Length)
        {
            int toWrite = (int)Math.Min(chunkSize, stream.Length - offset);
            var progress = await client.UploadChunkAsync(session, stream, offset, toWrite);
            offset += toWrite;
        }
        var media = await client.CompleteUploadAsync(session);
        var videoPart = new VideoPart(media.MediaId);
        var genReq = new GenerateFromVideoRequest("gemini-pro-video", new[] { videoPart }, "Describe this video");
        var op = await client.GenerateFromVideoAsync(genReq);
        await foreach (var result in client.WaitForCompletionAsync(op.Name, TimeSpan.FromSeconds(2), TimeSpan.FromSeconds(30)))
        {
            Console.WriteLine($"Status: {result.Status}");
            if (result.Status == "DONE")
                Console.WriteLine($"Result: {result.Result}");
        }
    }
}
