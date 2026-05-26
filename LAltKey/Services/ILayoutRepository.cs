using LAltKey.Models;

namespace LAltKey.Services;

/// <summary>
/// text.
/// </summary>
public interface ILayoutRepository
{
    event Action? LayoutsChanged;

    string DefaultLayoutName { get; }

    IReadOnlyList<string> GetAvailableLayouts();

    LayoutConfig? TryLoad(string name, Action<Exception>? onError = null);

    void Save(string name, LayoutConfig config);

    bool Delete(string name);
}

