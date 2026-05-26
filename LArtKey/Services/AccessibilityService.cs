using System.Speech.Synthesis;
using LArtKey.Models;

namespace LArtKey.Services;

/// <summary>
/// [English text] English text.
/// [English text] English text.
/// </summary>
public sealed class AccessibilityService : IDisposable
{
    private readonly SpeechSynthesizer _synth;
    private readonly ConfigService _configService;
    private string _lastSpokenLabel = "";
    private DateTime _lastSpokenTime = DateTime.MinValue;

    public AccessibilityService(ConfigService configService)
    {
        _configService = configService;
        _synth = new SpeechSynthesizer();
        _synth.SetOutputToDefaultAudioDevice();

        // English text.
        try
        {
            var koVoice = _synth.GetInstalledVoices()
                .FirstOrDefault(v => v.VoiceInfo.Culture.Name.StartsWith("ko", StringComparison.OrdinalIgnoreCase));
            if (koVoice != null)
                _synth.SelectVoice(koVoice.VoiceInfo.Name);
        }
        catch
        {
            // English text — English text
        }
    }

    /// <summary>
    /// English text.
    /// </summary>
    public void SpeakLabel(string? label)
    {
        if (!_configService.Current.TtsEnabled)
            return;

        if (string.IsNullOrWhiteSpace(label))
            return;

        // English text.
        if (label == _lastSpokenLabel && (DateTime.UtcNow - _lastSpokenTime).TotalMilliseconds < 500)
            return;

        _lastSpokenLabel = label;
        _lastSpokenTime = DateTime.UtcNow;

        try
        {
            int rate = Math.Clamp(_configService.Current.TtsRate, -5, 5);
            _synth.Rate = rate;
            // English text.
            _synth.SpeakAsyncCancelAll();
            _synth.SpeakAsync(label);
        }
        catch
        {
            // TTS English text — English text.
        }
    }

    public void Dispose()
    {
        try { _synth.Dispose(); } catch { /* English text */ }
    }
}
