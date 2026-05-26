using LArtKey.Models;

namespace LArtKey.Services;

/// <summary>
/// text.
/// </summary>
public sealed class LayoutRepository : ILayoutRepository
{
    private readonly LayoutService _layoutService;
    private readonly ConfigService _configService;

    public LayoutRepository(LayoutService layoutService, ConfigService configService)
    {
        _layoutService = layoutService;
        _configService = configService;
        _layoutService.LayoutsChanged += () => LayoutsChanged?.Invoke();
    }

    public event Action? LayoutsChanged;

    public string DefaultLayoutName => _configService.Current.DefaultLayout;

    public IReadOnlyList<string> GetAvailableLayouts() => _layoutService.GetAvailableLayouts();

    public LayoutConfig? TryLoad(string name, Action<Exception>? onError = null) =>
        _layoutService.TryLoad(name, onError);

    public void Save(string name, LayoutConfig config) => _layoutService.Save(name, config);

    public bool Delete(string name) => _layoutService.Delete(name);
}

