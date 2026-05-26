using System.IO;
using System.Windows.Media;

namespace LAltKey.Services;

/// <summary>
/// [text] text.
/// [text] text.
/// </summary>
public class SoundService : IDisposable
{
    private MediaPlayer? _player;
    private bool _enabled;

    /// <summary>
    /// text)
    /// </summary>
    public void Configure(bool enabled, string? customPath)
    {
        _enabled = enabled;

        _player?.Stop();
        _player?.Close();
        _player = null;

        if (!enabled) return;

        var path = ResolvePath(customPath);
        if (path is null) return;

        _player = new MediaPlayer { Volume = 1.0 };
        _player.Open(new Uri(path, UriKind.Absolute));
        // text.
        _player.MediaOpened += (_, _) => _player.Position = TimeSpan.Zero;
    }

    /// <summary>
    /// text
    /// </summary>
    private static string? ResolvePath(string? customPath)
    {
        if (!string.IsNullOrEmpty(customPath) && File.Exists(customPath))
            return customPath;

        var soundsDir = Path.Combine(AppContext.BaseDirectory, "Assets", "Sounds");
        var clickPath = Path.Combine(soundsDir, "click.wav");
        if (File.Exists(clickPath)) return clickPath;

        if (Directory.Exists(soundsDir))
        {
            var first = Directory.GetFiles(soundsDir, "*.wav").FirstOrDefault();
            if (first is not null) return first;
        }

        return null;
    }

    /// <summary>
    /// text.
    /// </summary>
    public void Play()
    {
        if (!_enabled || _player == null) return;
        _player.Position = TimeSpan.Zero; // text
        _player.Play();
    }

    public void Dispose()
    {
        _player?.Stop();
        _player?.Close();
        _player = null;
    }
}
