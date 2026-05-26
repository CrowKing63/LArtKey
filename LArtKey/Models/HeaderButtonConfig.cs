using System.Text.Json.Serialization;

namespace LArtKey.Models;

public enum HeaderButtonKind
{
    BuiltIn,
    Custom
}

public enum HeaderButtonDisplayMode
{
    IconOnly
}

/// <summary>
/// [text] text.
/// [text] text.
/// [text] text.
/// </summary>
public class HeaderButtonConfig
{
    /// <summary>
    /// text.
    /// </summary>
    public const int MaxCustomButtonCount = 10;

    /// <summary>
    /// text.
    /// </summary>
    public const int MaxVisibleButtonsLeft = 8;

    /// <summary>
    /// text.
    /// </summary>
    public const int MaxVisibleButtonsRight = 8;

    /// <summary>
    /// text "custom-..." text.
    /// </summary>
    public string Id { get; set; } = "";

    /// <summary>
    /// text.
    /// </summary>
    public HeaderButtonKind Kind { get; set; } = HeaderButtonKind.BuiltIn;

    /// <summary>
    /// text.
    /// </summary>
    public bool Visible { get; set; } = true;

    /// <summary>
    /// text. "Left" text "Right"text.
    /// </summary>
    public string Position { get; set; } = "Right";

    /// <summary>
    /// text.
    /// </summary>
    public HeaderButtonDisplayMode DisplayMode { get; set; } = HeaderButtonDisplayMode.IconOnly;

    /// <summary>
    /// text: "Custom shortcut", "Custom shortcut", "📌"
    /// </summary>
    public string IconText { get; set; } = "";

    /// <summary>
    /// text.
    /// </summary>
    public string Tooltip { get; set; } = "";

    /// <summary>
    /// text.
    /// </summary>
    public string AccessibleName { get; set; } = "";

    /// <summary>
    /// text.
    /// </summary>
    public KeyAction? CustomAction { get; set; }

    /// <summary>
    /// text.
    /// </summary>
    [JsonIgnore]
    public string DisplayName => Kind == HeaderButtonKind.BuiltIn
        ? GetDisplayName(Id)
        : (string.IsNullOrWhiteSpace(Tooltip) ? "Custom shortcut" : Tooltip);

    /// <summary>
    /// text.
    /// </summary>
    [JsonIgnore]
    public string EffectiveIconText => Kind == HeaderButtonKind.BuiltIn
        ? GetBuiltInIconText(Id)
        : (string.IsNullOrWhiteSpace(IconText) ? "Custom shortcut" : IconText);

    /// <summary>
    /// text.
    /// </summary>
    [JsonIgnore]
    public string EffectiveTooltip => Kind == HeaderButtonKind.BuiltIn
        ? GetBuiltInTooltip(Id)
        : (string.IsNullOrWhiteSpace(Tooltip) ? "Custom shortcut" : Tooltip);

    /// <summary>
    /// text.
    /// </summary>
    [JsonIgnore]
    public string EffectiveAccessibleName => Kind == HeaderButtonKind.BuiltIn
        ? GetBuiltInAccessibleName(Id)
        : (string.IsNullOrWhiteSpace(AccessibleName) ? EffectiveTooltip : AccessibleName);

    // ── text ─────────────────────────────────────────────────
    public const string IdClipboard = "Clipboard";
    public const string IdEmoji = "Emoji";
    public const string IdAutoComplete = "AutoComplete";
    public const string IdOsIme = "OsIme";
    public const string IdOsk = "Osk";
    public const string IdSettings = "Settings";
    public const string IdAi = "Ai";

    /// <summary>
    /// text.
    /// </summary>
    public static List<HeaderButtonConfig> CreateDefaults() =>
    [
        CreateBuiltIn(IdClipboard, visible: true, position: "Right"),
        CreateBuiltIn(IdEmoji, visible: true, position: "Right"),
        CreateBuiltIn(IdAutoComplete, visible: true, position: "Right"),
        CreateBuiltIn(IdOsIme, visible: true, position: "Right"),
        CreateBuiltIn(IdOsk, visible: true, position: "Right"),
        CreateBuiltIn(IdSettings, visible: true, position: "Right"),
        CreateBuiltIn(IdAi, visible: false, position: "Right"),
    ];

    public static HeaderButtonConfig CreateBuiltIn(string id, bool visible = true, string position = "Right") => new()
    {
        Id = id,
        Kind = HeaderButtonKind.BuiltIn,
        Visible = visible,
        Position = NormalizePosition(position),
        DisplayMode = HeaderButtonDisplayMode.IconOnly
    };

    /// <summary>
    /// text.
    /// </summary>
    public static HeaderButtonConfig CreateCustomDefault() => new()
    {
        Id = $"custom-{Guid.NewGuid():N}",
        Kind = HeaderButtonKind.Custom,
        Visible = true,
        Position = "Right",
        DisplayMode = HeaderButtonDisplayMode.IconOnly,
        IconText = "A",
        Tooltip = "Custom shortcut",
        AccessibleName = "Custom shortcut",
        CustomAction = new SendKeyAction("VK_A")
    };

    public static bool IsBuiltInId(string id) => id is
        IdClipboard or IdEmoji or IdAutoComplete or IdOsIme or IdOsk or IdSettings or IdAi;

    public static string GetDisplayName(string id) => id switch
    {
        IdClipboard => "Clipboard",
        IdEmoji => "Emoji",
        IdAutoComplete => "Prediction",
        IdOsIme => "OS input language",
        IdOsk => "Windows on-screen keyboard",
        IdSettings => "Settings",
        IdAi => "AI",
        _ => "Custom shortcut"
    };

    public static string GetBuiltInIconText(string id) => id switch
    {
        IdClipboard => "📋",
        IdEmoji => "😊",
        IdAutoComplete => "ABC",
        IdOsIme => "IME",
        IdOsk => "⌨",
        IdSettings => "⚙",
        IdAi => "✨",
        _ => "?"
    };

    public static string GetBuiltInTooltip(string id) => id switch
    {
        IdClipboard => "Open clipboard history",
        IdEmoji => "Open emoji panel",
        IdAutoComplete => "Toggle prediction",
        IdOsIme => "Switch OS input language",
        IdOsk => "Open Windows on-screen keyboard",
        IdSettings => "Open settings",
        IdAi => "Run AI tool",
        _ => GetDisplayName(id)
    };

    public static string GetBuiltInAccessibleName(string id) => id switch
    {
        IdClipboard => "Clipboard button",
        IdEmoji => "Emoji button",
        IdAutoComplete => "Prediction toggle button",
        IdOsIme => "OS input language button",
        IdOsk => "Windows on-screen keyboard button",
        IdSettings => "Settings button",
        IdAi => "AI tool button",
        _ => GetDisplayName(id)
    };

    public static string NormalizePosition(string? position) =>
        string.Equals(position, "Left", StringComparison.OrdinalIgnoreCase) ? "Left" : "Right";

    /// <summary>
    /// text.
    /// </summary>
    public static int GetMaxVisibleButtons(string? position) =>
        NormalizePosition(position) == "Left" ? MaxVisibleButtonsLeft : MaxVisibleButtonsRight;

    /// <summary>
    /// text, "Custom shortcut"text "Custom shortcut" text.
    /// </summary>
    public static int CountCustomButtons(IEnumerable<HeaderButtonConfig> buttons, string? excludingId = null) =>
        buttons.Count(button =>
            button.Kind == HeaderButtonKind.Custom
            && !string.Equals(button.Id, excludingId, StringComparison.Ordinal));

    /// <summary>
    /// text.
    /// </summary>
    public static int CountVisibleButtons(IEnumerable<HeaderButtonConfig> buttons, string? position, string? excludingId = null)
    {
        var normalizedPosition = NormalizePosition(position);
        return buttons.Count(button =>
            button.Visible
            && NormalizePosition(button.Position) == normalizedPosition
            && !string.Equals(button.Id, excludingId, StringComparison.Ordinal));
    }
}
