namespace LAltKey.Services;

public sealed class LiveRegionService
{
    public event Action<string>? Announced;
    
    private string _lastMessage = "";
    private DateTime _lastAnnouncedAtUtc = DateTime.MinValue;
    private static readonly TimeSpan DuplicateWindow = TimeSpan.FromMilliseconds(500);

    /// <summary>
    /// [text] text.
    /// </summary>
    public void Announce(string message)
    {
        if (string.IsNullOrWhiteSpace(message))
            return;

        var now = DateTime.UtcNow;
        if (message == _lastMessage && (now - _lastAnnouncedAtUtc) < DuplicateWindow)
            return;

        _lastMessage = message;
        _lastAnnouncedAtUtc = now;
        Announced?.Invoke(message);
    }
}
