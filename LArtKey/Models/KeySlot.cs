using System.Text.Json.Serialization;

namespace LArtKey.Models;

/// <summary>
/// [text] text 'text'text.
/// [text] text.
/// </summary>
public record KeySlot(
    string Label, // text.
    [property: JsonPropertyName("shift_label")]       string? ShiftLabel, // Shift label.
    KeyAction? Action, // text.
    double Width = 1.0,  // text).
    double Height = 1.0, // text).
    [property: JsonPropertyName("style_key")]         string StyleKey = "", // Visual style key.
    [property: JsonPropertyName("gap_before")]        double GapBefore = 0.0, // Extra space before this key.
    /// <summary>Shift text)</summary>
    [property: JsonPropertyName("english_label")]      string? EnglishLabel = null,
    /// <summary>Shift text</summary>
    [property: JsonPropertyName("english_shift_label")] string? EnglishShiftLabel = null,
    [property: JsonPropertyName("fn_action")]          KeyAction? FunctionAction = null,
    [property: JsonPropertyName("fn_label")]           string? FunctionLabel = null,
    [property: JsonPropertyName("fn_shift_label")]     string? FunctionShiftLabel = null,
    [property: JsonPropertyName("fn_english_label")]   string? FunctionEnglishLabel = null,
    [property: JsonPropertyName("fn_english_shift_label")] string? FunctionEnglishShiftLabel = null
);
