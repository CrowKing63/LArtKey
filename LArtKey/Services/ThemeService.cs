using System.Linq;
using System.Windows;
using Microsoft.Win32;
using WpfApp = System.Windows.Application;

namespace LArtKey.Services;

/// <summary>
/// [text] text.
/// [text] text.
/// </summary>
public class ThemeService
{
    private readonly ConfigService _configService;
    private ResourceDictionary? _currentThemeDict;

    public ThemeService(ConfigService configService)
    {
        _configService = configService;

        // text.
        SystemParameters.StaticPropertyChanged += (s, e) =>
        {
            if (e.PropertyName == nameof(SystemParameters.HighContrast)
                || e.PropertyName == "WindowGlassColor")
            {
                if (_configService.Current.Theme == "system")
                    Apply("system");
            }
        };
    }

    /// <summary>
    /// text.
    /// </summary>
    public void Apply(string theme)
    {
        // 'system' text.
        var resolved = theme == "system" ? DetectSystemTheme() : theme;
        var uri = new Uri($"pack://application:,,,/LArtKey;component/Themes/{resolved}Theme.xaml",
                          UriKind.Absolute);
        var dict = new ResourceDictionary { Source = uri };

        var merged = WpfApp.Current.Resources.MergedDictionaries;
        // text.
        if (_currentThemeDict is not null)
            merged.Remove(_currentThemeDict);
        _currentThemeDict = dict;
        merged.Add(dict);
    }

    /// <summary>
    /// text.
    /// </summary>
    private static string DetectSystemTheme()
    {
        // 1. text
        if (SystemParameters.HighContrast)
            return "HighContrast";
        
        try
        {
            // 2. text
            using var key = Registry.CurrentUser.OpenSubKey(
                @"Software\Microsoft\Windows\CurrentVersion\Themes\Personalize");
            var val = key?.GetValue("AppsUseLightTheme");
            return val is int i && i == 0 ? "Dark" : "Light";
        }
        catch
        {
            return "Dark"; // Fallback theme.
        }
    }
}
