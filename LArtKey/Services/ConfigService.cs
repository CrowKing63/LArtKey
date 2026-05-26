using System.IO;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading;
using LArtKey.Models;

namespace LArtKey.Services;

/// <summary>
/// [text] text.
/// [text] text.
/// </summary>
public class ConfigService
{
    private const string LegacyDefaultLayoutName = "Bagic";
    private const string LegacyDefaultLayoutPlusName = "Bagic Plus";
    private const string CurrentDefaultLayoutName = "Basic";
    private const string CurrentDefaultLayoutPlusName = "Basic Plus";

    /// <summary>text.</summary>
    public AppConfig Current { get; private set; } = new();

    /// <summary>text.</summary>
    /// <param name="propertyName">text.</param>
    public event Action<string?>? ConfigChanged;

    public ConfigService()
    {
        // text.
        Directory.CreateDirectory(Path.GetDirectoryName(PathResolver.ConfigPath)!);
        Load();
    }

    /// <summary>
    /// config.jsontext.
    /// </summary>
    public void Load()
    {
        if (!File.Exists(PathResolver.ConfigPath))
        {
            Save();
            return;
        }

        try
        {
            var json = File.ReadAllText(PathResolver.ConfigPath);
            Current = JsonSerializer.Deserialize<AppConfig>(json, JsonOptions.Default) ?? new();

            MigrateWindowConfig(json);
            MigrateLegacyLayoutNames();

            // text.
            Current.HeaderButtons ??= [];
            if (Current.HeaderButtons.Count == 0)
            {
                Current.HeaderButtons = HeaderButtonConfig.CreateDefaults();
                Save();
                return;
            }

            if (NormalizeHeaderButtons())
            {
                Save();
            }
        }
        catch
        {
            Current = new AppConfig();
        }
    }

    /// <summary>
    /// text.
    /// </summary>
    public void ReloadFromDiskAndNotify(string? propertyName = null)
    {
        Load();
        ConfigChanged?.Invoke(propertyName);
    }

    /// <summary>
    /// text.
    /// </summary>
    private void MigrateWindowConfig(string json)
    {
        try
        {
            var node = JsonNode.Parse(json)?["Window"]?.AsObject();
            if (node == null || node.ContainsKey("Scale"))
                return;

            if (node.TryGetPropertyValue("Width", out var widthNode) && widthNode != null)
            {
                var width = (double)widthNode;
                var scale = (int)Math.Round(width / 900.0 * 100);
                Current.Window.Scale = Math.Clamp(scale, 60, 200);
            }
        }
        catch
        {
        }
    }

    /// <summary>
    /// text.
    /// </summary>
    private void MigrateLegacyLayoutNames()
    {
        Current.DefaultLayout = MigrateLegacyLayoutName(Current.DefaultLayout);

        if (Current.Profiles.Count == 0)
            return;

        var migratedProfiles = new Dictionary<string, string>(Current.Profiles.Count, StringComparer.OrdinalIgnoreCase);
        foreach (var pair in Current.Profiles)
            migratedProfiles[pair.Key] = MigrateLegacyLayoutName(pair.Value);

        Current.Profiles = migratedProfiles;
    }

    /// <summary>
    /// text.
    /// </summary>
    private static string MigrateLegacyLayoutName(string? layoutName)
    {
        return layoutName switch
        {
            LegacyDefaultLayoutName => CurrentDefaultLayoutName,
            LegacyDefaultLayoutPlusName => CurrentDefaultLayoutPlusName,
            _ => layoutName ?? CurrentDefaultLayoutName
        };
    }

    /// <summary>
    /// text.
    /// </summary>
    private bool NormalizeHeaderButtons()
    {
        var changed = false;

        foreach (var button in Current.HeaderButtons)
        {
            var normalizedPosition = HeaderButtonConfig.NormalizePosition(button.Position);
            if (button.Position != normalizedPosition)
            {
                button.Position = normalizedPosition;
                changed = true;
            }

            if (button.DisplayMode != HeaderButtonDisplayMode.IconOnly)
            {
                button.DisplayMode = HeaderButtonDisplayMode.IconOnly;
                changed = true;
            }

            if (button.Kind == HeaderButtonKind.BuiltIn && !HeaderButtonConfig.IsBuiltInId(button.Id))
            {
                button.Kind = HeaderButtonKind.Custom;
                changed = true;
            }

            if (button.Kind != HeaderButtonKind.Custom)
                continue;

            if (string.IsNullOrWhiteSpace(button.Id))
            {
                button.Id = HeaderButtonConfig.CreateCustomDefault().Id;
                changed = true;
            }

            var iconText = string.IsNullOrWhiteSpace(button.IconText) ? "A" : button.IconText.Trim();
            if (button.IconText != iconText)
            {
                button.IconText = iconText;
                changed = true;
            }

            var tooltip = string.IsNullOrWhiteSpace(button.Tooltip) ? "Custom shortcut" : button.Tooltip.Trim();
            if (button.Tooltip != tooltip)
            {
                button.Tooltip = tooltip;
                changed = true;
            }

            var accessibleName = string.IsNullOrWhiteSpace(button.AccessibleName) ? button.Tooltip : button.AccessibleName.Trim();
            if (button.AccessibleName != accessibleName)
            {
                button.AccessibleName = accessibleName;
                changed = true;
            }

            if (button.CustomAction is null)
            {
                button.CustomAction = new SendKeyAction("VK_A");
                changed = true;
            }
        }

        // text.
        var visibleLeft = 0;
        var visibleRight = 0;
        foreach (var button in Current.HeaderButtons)
        {
            if (!button.Visible)
                continue;

            if (HeaderButtonConfig.NormalizePosition(button.Position) == "Left")
            {
                if (visibleLeft >= HeaderButtonConfig.MaxVisibleButtonsLeft)
                {
                    button.Visible = false;
                    changed = true;
                    continue;
                }

                visibleLeft++;
                continue;
            }

            if (visibleRight >= HeaderButtonConfig.MaxVisibleButtonsRight)
            {
                button.Visible = false;
                changed = true;
                continue;
            }

            visibleRight++;
        }

        return changed;
    }

    /// <summary>
    /// text.
    /// </summary>
    public void Save()
    {
        const int maxRetries = 3;
        for (int i = 0; i < maxRetries; i++)
        {
            try
            {
                File.WriteAllText(
                    PathResolver.ConfigPath,
                    JsonSerializer.Serialize(Current, JsonOptions.Default));
                return;
            }
            catch (IOException) when (i < maxRetries - 1)
            {
                Thread.Sleep(300);
            }
        }
    }

    /// <summary>
    /// text.
    /// </summary>
    public void Update(Action<AppConfig> updater, string? propertyName = null)
    {
        updater(Current);
        Save();
        ConfigChanged?.Invoke(propertyName);
    }
}
