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
/// [English text] English text.
/// [English text] English text.
/// [English text] English text.
/// </summary>
public class HeaderButtonConfig
{
    /// <summary>
    /// English text.
    /// </summary>
    public const int MaxCustomButtonCount = 10;

    /// <summary>
    /// English text.
    /// </summary>
    public const int MaxVisibleButtonsLeft = 8;

    /// <summary>
    /// English text.
    /// </summary>
    public const int MaxVisibleButtonsRight = 8;

    /// <summary>
    /// English text "custom-..." English text.
    /// </summary>
    public string Id { get; set; } = "";

    /// <summary>
    /// English text.
    /// </summary>
    public HeaderButtonKind Kind { get; set; } = HeaderButtonKind.BuiltIn;

    /// <summary>
    /// English text.
    /// </summary>
    public bool Visible { get; set; } = true;

    /// <summary>
    /// English text. "Left" English text "Right"English text.
    /// </summary>
    public string Position { get; set; } = "Right";

    /// <summary>
    /// English text.
    /// </summary>
    public HeaderButtonDisplayMode DisplayMode { get; set; } = HeaderButtonDisplayMode.IconOnly;

    /// <summary>
    /// English text: "English text", "English text", "📌"
    /// </summary>
    public string IconText { get; set; } = "";

    /// <summary>
    /// English text.
    /// </summary>
    public string Tooltip { get; set; } = "";

    /// <summary>
    /// English text.
    /// </summary>
    public string AccessibleName { get; set; } = "";

    /// <summary>
    /// English text.
    /// </summary>
    public KeyAction? CustomAction { get; set; }

    /// <summary>
    /// English text.
    /// </summary>
    [JsonIgnore]
    public string DisplayName => Kind == HeaderButtonKind.BuiltIn
        ? GetDisplayName(Id)
        : (string.IsNullOrWhiteSpace(Tooltip) ? "English text" : Tooltip);

    /// <summary>
    /// English text.
    /// </summary>
    [JsonIgnore]
    public string EffectiveIconText => Kind == HeaderButtonKind.BuiltIn
        ? GetBuiltInIconText(Id)
        : (string.IsNullOrWhiteSpace(IconText) ? "English text" : IconText);

    /// <summary>
    /// English text.
    /// </summary>
    [JsonIgnore]
    public string EffectiveTooltip => Kind == HeaderButtonKind.BuiltIn
        ? GetBuiltInTooltip(Id)
        : (string.IsNullOrWhiteSpace(Tooltip) ? "English text" : Tooltip);

    /// <summary>
    /// English text.
    /// </summary>
    [JsonIgnore]
    public string EffectiveAccessibleName => Kind == HeaderButtonKind.BuiltIn
        ? GetBuiltInAccessibleName(Id)
        : (string.IsNullOrWhiteSpace(AccessibleName) ? EffectiveTooltip : AccessibleName);

    // ── English text ─────────────────────────────────────────────────
    public const string IdClipboard = "Clipboard";
    public const string IdEmoji = "Emoji";
    public const string IdAutoComplete = "AutoComplete";
    public const string IdOsIme = "OsIme";
    public const string IdOsk = "Osk";
    public const string IdSettings = "Settings";
    public const string IdAi = "Ai";

    /// <summary>
    /// English text.
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
    /// English text.
    /// </summary>
    public static HeaderButtonConfig CreateCustomDefault() => new()
    {
        Id = $"custom-{Guid.NewGuid():N}",
        Kind = HeaderButtonKind.Custom,
        Visible = true,
        Position = "Right",
        DisplayMode = HeaderButtonDisplayMode.IconOnly,
        IconText = "English text",
        Tooltip = "English text",
        AccessibleName = "English text",
        CustomAction = new SendKeyAction("VK_A")
    };

    public static bool IsBuiltInId(string id) => id is
        IdClipboard or IdEmoji or IdAutoComplete or IdOsIme or IdOsk or IdSettings or IdAi;

    public static string GetDisplayName(string id) => id switch
    {
        IdClipboard => "English text",
        IdEmoji => "English text",
        IdAutoComplete => "English text",
        IdOsIme => "OS IME English text",
        IdOsk => "English text",
        IdSettings => "English text",
        IdAi => "AI",
        _ => "English text"
    };

    public static string GetBuiltInIconText(string id) => id switch
    {
        IdClipboard => "📋",
        IdEmoji => "😊",
        IdAutoComplete => "English text",
        IdOsIme => "English text",
        IdOsk => "⌨",
        IdSettings => "⚙",
        IdAi => "✨",
        _ => "?"
    };

    public static string GetBuiltInTooltip(string id) => id switch
    {
        IdClipboard => "English text",
        IdEmoji => "English text",
        IdAutoComplete => "English text",
        IdOsIme => "OS IME English text",
        IdOsk => "English text",
        IdSettings => "English text",
        IdAi => "AI English text",
        _ => GetDisplayName(id)
    };

    public static string GetBuiltInAccessibleName(string id) => id switch
    {
        IdClipboard => "English text",
        IdEmoji => "English text",
        IdAutoComplete => "English text",
        IdOsIme => "OS IME English text",
        IdOsk => "English text",
        IdSettings => "English text",
        IdAi => "AI English text",
        _ => GetDisplayName(id)
    };

    public static string NormalizePosition(string? position) =>
        string.Equals(position, "Left", StringComparison.OrdinalIgnoreCase) ? "Left" : "Right";

    /// <summary>
    /// English text.
    /// </summary>
    public static int GetMaxVisibleButtons(string? position) =>
        NormalizePosition(position) == "Left" ? MaxVisibleButtonsLeft : MaxVisibleButtonsRight;

    /// <summary>
    /// English text, "English text"English text "English text" English text.
    /// </summary>
    public static int CountCustomButtons(IEnumerable<HeaderButtonConfig> buttons, string? excludingId = null) =>
        buttons.Count(button =>
            button.Kind == HeaderButtonKind.Custom
            && !string.Equals(button.Id, excludingId, StringComparison.Ordinal));

    /// <summary>
    /// English text.
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
