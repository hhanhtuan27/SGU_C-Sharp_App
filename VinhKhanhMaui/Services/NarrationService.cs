using VinhKhanhMaui.Models;

namespace VinhKhanhMaui.Services;

/// <summary>
/// TTS với priority queue — nhiều POI cùng vào vùng sẽ phát nối tiếp,
/// không overlap. Đổi ngôn ngữ giữa chừng sẽ stop và clear queue.
/// </summary>
public class NarrationService
{
    private readonly Queue<(PointOfInterest Poi, string Lang)> _queue = new();
    private bool _isSpeaking = false;
    private CancellationTokenSource? _cts;

    public string CurrentLanguage { get; set; } = "vi";

    public event EventHandler<PointOfInterest>? SpeakingStarted;
    public event EventHandler? SpeakingCompleted;

    public void EnqueueSpeak(PointOfInterest poi)
    {
        _queue.Enqueue((poi, CurrentLanguage));
        _ = ProcessQueueAsync();
    }

    public void StopAll()
    {
        _queue.Clear();
        _cts?.Cancel();
    }

    private async Task ProcessQueueAsync()
    {
        if (_isSpeaking) return;
        _isSpeaking = true;

        while (_queue.Count > 0)
        {
            var (poi, lang) = _queue.Dequeue();
            string text = poi.GetDescription(lang);

            try
            {
                SpeakingStarted?.Invoke(this, poi);

                _cts = new CancellationTokenSource();
                var locales = await TextToSpeech.Default.GetLocalesAsync();

                var locale = locales.FirstOrDefault(l =>
                    l.Language.StartsWith(lang, StringComparison.OrdinalIgnoreCase))
                    ?? locales.FirstOrDefault(l =>
                        l.Language.StartsWith("en", StringComparison.OrdinalIgnoreCase));

                var options = new SpeechOptions { Locale = locale };
                await TextToSpeech.Default.SpeakAsync(text, options, _cts.Token);
            }
            catch (OperationCanceledException) { }
            catch { }
            finally
            {
                SpeakingCompleted?.Invoke(this, EventArgs.Empty);
            }
        }

        _isSpeaking = false;
    }
}