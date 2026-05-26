using System.IO;
using System.Text.Json;
using LAltKey.Models;

namespace LAltKey.Services;

/// <summary>
/// [text] text.
/// [text] text.
/// </summary>
public class LayoutService
{
    private readonly string _layoutsDir; // text.
    private readonly Dictionary<string, LayoutConfig> _cache = []; // text.

    public event Action? LayoutsChanged;

    public LayoutService() : this(PathResolver.LayoutsDir) { }

    protected LayoutService(string layoutsDir)
    {
        _layoutsDir = layoutsDir;
        Directory.CreateDirectory(_layoutsDir);
    }

    public IReadOnlyList<string> GetAvailableLayouts()
    {
        return Directory.GetFiles(_layoutsDir, "*.json")
            .Select(Path.GetFileNameWithoutExtension)
            .Where(n => n is not null)
            .Select(n => n!)
            .OrderBy(n => n)
            .ToList();
    }

    public LayoutConfig Load(string name)
    {
        if (_cache.TryGetValue(name, out var cached)) return cached;

        var path = Path.Combine(_layoutsDir, $"{name}.json");
        if (!File.Exists(path))
            throw new FileNotFoundException($"Layout file not found: {path}");

        string json;
        try
        {
            json = File.ReadAllText(path);
        }
        catch (IOException ex)
        {
            throw new InvalidDataException($"Could not read layout: {name}", ex);
        }

        LayoutConfig layout;
        try
        {
            layout = JsonSerializer.Deserialize<LayoutConfig>(json, JsonOptions.Default)
                ?? throw new InvalidDataException($"Layout file is empty or invalid: {name}");
        }
        catch (JsonException ex)
        {
            // T-6.7: text JSON → text
            throw new InvalidDataException($"Invalid layout JSON: {name} — {ex.Message}", ex);
        }

        _cache[name] = layout;
        return layout;
    }

    /// <summary>T-6.7: text)</summary>
    public LayoutConfig? TryLoad(string name, Action<Exception>? onError = null)
    {
        try { return Load(name); }
        catch (Exception ex)
        {
            onError?.Invoke(ex);
            return null;
        }
    }

    /// <summary>T-9.2: text</summary>
    public void Save(string name, LayoutConfig config)
    {
        var path = Path.Combine(_layoutsDir, name + ".json");
        var json = JsonSerializer.Serialize(config, JsonOptions.Default);
        File.WriteAllText(path, json);
        InvalidateCache();
        LayoutsChanged?.Invoke();
    }

    /// <summary>text</summary>
    public bool Delete(string name)
    {
        var path = Path.Combine(_layoutsDir, name + ".json");
        if (!File.Exists(path)) return false;
        File.Delete(path);
        InvalidateCache();
        LayoutsChanged?.Invoke();
        return true;
    }

    /// <summary>config text</summary>
    public void InvalidateCache() => _cache.Clear();

    /// <summary>
    /// [text] text.
    /// [text] text.
    /// </summary>
    public void NotifyExternalLayoutsChanged()
    {
        InvalidateCache();
        LayoutsChanged?.Invoke();
    }
}
