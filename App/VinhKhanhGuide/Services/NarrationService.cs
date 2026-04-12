using System;
using System.Linq;
using System.Speech.Synthesis;
using VinhKhanhGuide.Models;

namespace VinhKhanhGuide.Services
{
    /// <summary>
    /// Wraps <see cref="SpeechSynthesizer"/> and exposes a tiny async API
    /// for the rest of the app. Speaking is queued on a background thread
    /// so the UI never blocks; a new request can cancel whatever is
    /// currently playing via <see cref="Stop"/>.
    /// </summary>
    public class NarrationService : IDisposable
    {
        private readonly SpeechSynthesizer _synth;
        public string Language { get; private set; } = "VN";
        public bool   IsSpeaking => _synth.State == SynthesizerState.Speaking;

        public event EventHandler<string> SpeakingStarted;
        public event EventHandler          SpeakingCompleted;

        public NarrationService()
        {
            _synth = new SpeechSynthesizer();
            _synth.SetOutputToDefaultAudioDevice();
            _synth.SpeakStarted   += (s, e) => SpeakingStarted?.Invoke(this, _currentText);
            _synth.SpeakCompleted += (s, e) => SpeakingCompleted?.Invoke(this, EventArgs.Empty);
            SetLanguage("VN");
        }

        public void SetLanguage(string lang)
        {
            Language = string.Equals(lang, "EN", StringComparison.OrdinalIgnoreCase) ? "EN" : "VN";

            // Try to pick a matching voice; fall back to the default if the
            // OS doesn't have a Vietnamese pack installed.
            try
            {
                var voices = _synth.GetInstalledVoices()
                    .Where(v => v.Enabled)
                    .Select(v => v.VoiceInfo)
                    .ToList();

                VoiceInfo match = null;
                if (Language == "VN")
                    match = voices.FirstOrDefault(v => v.Culture.Name.StartsWith("vi"));
                if (match == null && Language == "EN")
                    match = voices.FirstOrDefault(v => v.Culture.Name.StartsWith("en"));

                if (match != null) _synth.SelectVoice(match.Name);
            }
            catch { /* keep default voice */ }
        }

        private string _currentText;

        /// <summary>Speak the description of a POI in the current language.</summary>
        public void Speak(PointOfInterest poi)
        {
            if (poi == null) return;
            Stop();
            _currentText = $"{poi.Name}. {poi.GetDescription(Language)}";
            _synth.SpeakAsync(_currentText);
        }

        public void SpeakRaw(string text)
        {
            if (string.IsNullOrWhiteSpace(text)) return;
            Stop();
            _currentText = text;
            _synth.SpeakAsync(text);
        }

        public void Stop()
        {
            if (_synth.State != SynthesizerState.Ready)
                _synth.SpeakAsyncCancelAll();
        }

        public void Dispose()
        {
            Stop();
            _synth.Dispose();
        }
    }
}
