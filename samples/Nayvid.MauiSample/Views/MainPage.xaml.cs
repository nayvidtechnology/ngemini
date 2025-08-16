using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using Nayvid.Gemini.Video;
using Nayvid.Gemini.Video.Models;
using Nayvid.Gemini.Image;
using Nayvid.Gemini.Speech;
using Nayvid.Gemini.Music;

namespace Nayvid.MauiSample.Views;
public partial class MainPage : ContentPage
{
    public MainPage()
    {
        InitializeComponent();
        BindingContext = new MainViewModel(App.Current.Services.GetService<IGeminiVideoClient>()!,
                                           App.Current.Services.GetService<IImageClient>()!,
                                           App.Current.Services.GetService<ISpeechClient>()!,
                                           App.Current.Services.GetService<IMusicClient>()!);
    }
}

public class MainViewModel : INotifyPropertyChanged
{
    private readonly IGeminiVideoClient _video;
    private readonly IImageClient _image;
    private readonly ISpeechClient _speech;
    private readonly IMusicClient _music;
    public MainViewModel(IGeminiVideoClient video, IImageClient image, ISpeechClient speech, IMusicClient music)
    { _video = video; _image = image; _speech = speech; _music = music; }

    public string VideoPrompt { get; set; } = "Describe this video";
    public string VideoStatus { get => _videoStatus; set { _videoStatus = value; OnPropertyChanged(); } }
    private string _videoStatus = string.Empty;

    public string ImagePrompt { get; set; } = "A sunset";
    public string ImageStatus { get => _imageStatus; set { _imageStatus = value; OnPropertyChanged(); } }
    private string _imageStatus = string.Empty;

    public string SpeechText { get; set; } = "Hello MAUI";
    public string SpeechStatus { get => _speechStatus; set { _speechStatus = value; OnPropertyChanged(); } }
    private string _speechStatus = string.Empty;

    public ICommand GenerateVideoCommand => new Command(async () => await GenerateVideo());
    public ICommand GenerateImageCommand => new Command(async () => await GenerateImage());
    public ICommand GenerateSpeechCommand => new Command(async () => await GenerateSpeech());

    private async Task GenerateVideo()
    {
        var start = await _video.StartResumableUploadAsync(new StartUploadRequest("placeholder.mp4", "video/mp4", 0));
        var media = await _video.CompleteUploadAsync(start);
        var op = await _video.GenerateFromVideoAsync(new GenerateFromVideoRequest("gemini-pro-video", new[] { new VideoPart(media.MediaId) }, VideoPrompt));
        VideoStatus = $"Op: {op.Name} Status: {op.Status}";
    }
    private async Task GenerateImage()
    {
        var op = await _image.GenerateImageAsync("image-model", ImagePrompt);
        ImageStatus = $"Op: {op.Name} Status: {op.Status}";
    }
    private async Task GenerateSpeech()
    {
        var bytes = await _speech.TextToSpeechAsync("speech-model", SpeechText);
        SpeechStatus = $"Bytes: {bytes.Length}";
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    void OnPropertyChanged([CallerMemberName] string? name=null) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}
