using System.Speech.Synthesis;

namespace QueueManagement.Api.Services
{
    public class AudioService : IAudioService
    {
        private readonly SpeechSynthesizer _synthesizer;

        public AudioService()
        {
            _synthesizer = new SpeechSynthesizer();
            _synthesizer.SetOutputToDefaultAudioDevice();

            // Set voice properties for better clarity
            _synthesizer.Rate = 0; // Normal rate
            _synthesizer.Volume = 100; // Maximum volume

            // Try to set a specific voice if available
            try
            {
                _synthesizer.SelectVoiceByHints(VoiceGender.Female, VoiceAge.Adult);
            }
            catch
            {
                // Use default voice if specific voice not available
            }
        }

        public async Task SpeakAsync(string text)
        {
            await Task.Run(() =>
            {
                try
                {
                    _synthesizer.Speak(text);
                }
                catch (Exception ex)
                {
                    // Log the error but don't crash the application
                    Console.WriteLine($"Error speaking audio: {ex.Message}");
                }
            });
        }

        public void Dispose()
        {
            _synthesizer?.Dispose();
        }
    }
}