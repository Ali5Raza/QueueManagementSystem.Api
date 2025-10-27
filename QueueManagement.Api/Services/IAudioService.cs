namespace QueueManagement.Api.Services
{
    public interface IAudioService
    {
        Task SpeakAsync(string text);
    }
}