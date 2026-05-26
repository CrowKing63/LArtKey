using System.IO;
using System.Text.Json;
using System.Windows.Threading;
using LAltKey.Models;

namespace LAltKey.Services;

/// <summary>
/// [text] text.
/// [text] text.
/// [text] text.
/// </summary>
public class ClipboardService : IDisposable
{
    // ── text ──────────────────────────────────────────────────────────────
    private const int MaxHistory = 50;   // text
    private const int MaxFavorites = 10; // text

    // ── text ────────────────────────────────────────────────────────────
    public event Action? HistoryChanged;
    public event Action? FavoritesChanged;

    // ── text ──────────────────────────────────────────────────────────────
    // text
    public IReadOnlyList<string> History => _history;
    public IReadOnlyList<string> Favorites => _favorites;

    // ── text ──────────────────────────────────────────────────────────────
    private readonly List<string> _history = [];
    private readonly List<string> _favorites = [];
    private readonly DispatcherTimer _pollTimer;
    private string? _lastClipboard;

    // text (PathResolver.DataDir + clipboard_history.json)
    private static string SavePath => Path.Combine(PathResolver.DataDir, "clipboard_history.json");

    public ClipboardService()
    {
        // text
        Load();

        // text)
        _pollTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(500) };
        _pollTimer.Tick += OnPollTick;
        _pollTimer.Start();
    }

    // ── text ─────────────────────────────────────────────────────

    private void OnPollTick(object? sender, EventArgs e)
    {
        // text)
        if (!ClipboardHelper.ContainsTextWithRetry()) return;
        var current = ClipboardHelper.GetTextWithRetry();
        if (current == null || current == _lastClipboard) return;
        _lastClipboard = current;
        AddToHistory(current);
    }

    // ── text ─────────────────────────────────────────────────────

    /// <summary>
    /// text.
    /// </summary>
    private void AddToHistory(string text)
    {
        _history.Remove(text);
        _history.Insert(0, text);
        if (_history.Count > MaxHistory)
            _history.RemoveAt(_history.Count - 1);
        HistoryChanged?.Invoke();
        Save();
    }

    /// <summary>
    /// text).
    /// </summary>
    public void PromoteItem(string text)
    {
        if (!_history.Contains(text)) return;
        _history.Remove(text);
        _history.Insert(0, text);
        HistoryChanged?.Invoke();
        Save();
    }

    /// <summary>
    /// text.
    /// </summary>
    public void PasteItem(string text)
    {
        // text)
        ClipboardHelper.SetTextWithRetry(text);
        _lastClipboard = text; // text
        PromoteItem(text);     // text
    }

    /// <summary>
    /// text.
    /// </summary>
    public void ClearHistory()
    {
        _history.Clear();
        _lastClipboard = null;
        HistoryChanged?.Invoke();
        Save();
    }

    // ── text ─────────────────────────────────────────────────────

    /// <summary>
    /// text.
    /// </summary>
    public void AddFavorite(string text)
    {
        if (_favorites.Contains(text)) return;
        if (_favorites.Count >= MaxFavorites)
            _favorites.RemoveAt(_favorites.Count - 1);
        _favorites.Insert(0, text);
        FavoritesChanged?.Invoke();
        Save();
    }

    /// <summary>
    /// text.
    /// </summary>
    public void RemoveFavorite(string text)
    {
        if (!_favorites.Remove(text)) return;
        FavoritesChanged?.Invoke();
        Save();
    }

    /// <summary>
    /// text).
    /// </summary>
    public void ToggleFavorite(string text)
    {
        if (_favorites.Contains(text))
            RemoveFavorite(text);
        else
            AddFavorite(text);
    }

    /// <summary>
    /// text.
    /// </summary>
    public bool IsFavorite(string text) => _favorites.Contains(text);

    // ── text ────────────────────────────────────────────────────

    /// <summary>
    /// text.
    /// </summary>
    private void Save()
    {
        try
        {
            var dir = Path.GetDirectoryName(SavePath);
            if (dir != null) Directory.CreateDirectory(dir);

            var data = new ClipboardData
            {
                History = [.. _history],
                Favorites = [.. _favorites]
            };
            File.WriteAllText(SavePath,
                JsonSerializer.Serialize(data, JsonOptions.Default));
        }
        catch { /* text) */ }
    }

    /// <summary>
    /// JSON text.
    /// </summary>
    private void Load()
    {
        if (!File.Exists(SavePath)) return;
        try
        {
            var json = File.ReadAllText(SavePath);
            var data = JsonSerializer.Deserialize<ClipboardData>(json, JsonOptions.Default);
            if (data == null) return;

            _history.Clear();
            _history.AddRange(data.History.Take(MaxHistory));

            _favorites.Clear();
            _favorites.AddRange(data.Favorites.Take(MaxFavorites));
        }
        catch
        {
            // text
        }
    }

    // ── text ──────────────────────────────────────────────────────────────

    public void Dispose()
    {
        _pollTimer.Stop();
        Save(); // text
    }
}

/// <summary>
/// text.
/// </summary>
file class ClipboardData
{
    public List<string> History { get; set; } = [];
    public List<string> Favorites { get; set; } = [];
}
