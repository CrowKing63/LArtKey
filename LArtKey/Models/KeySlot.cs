using System.Text.Json.Serialization;

namespace LArtKey.Models;

/// <summary>
/// [English text] English text 'English text'English text.
/// [English text] English text.
/// </summary>
public record KeySlot(
    string Label, // English text.
    [property: JsonPropertyName("shift_label")]       string? ShiftLabel, // ShiftEnglish text.
    KeyAction? Action, // English text.
    double Width = 1.0,  // English text).
    double Height = 1.0, // English text).
    [property: JsonPropertyName("style_key")]         string StyleKey = "", // English text.
    [property: JsonPropertyName("gap_before")]        double GapBefore = 0.0, // English text.
    /// <summary>Shift English text)</summary>
    [property: JsonPropertyName("english_label")]      string? EnglishLabel = null,
    /// <summary>Shift English text</summary>
    [property: JsonPropertyName("english_shift_label")] string? EnglishShiftLabel = null,
    [property: JsonPropertyName("fn_action")]          KeyAction? FunctionAction = null,
    [property: JsonPropertyName("fn_label")]           string? FunctionLabel = null,
    [property: JsonPropertyName("fn_shift_label")]     string? FunctionShiftLabel = null,
    [property: JsonPropertyName("fn_english_label")]   string? FunctionEnglishLabel = null,
    [property: JsonPropertyName("fn_english_shift_label")] string? FunctionEnglishShiftLabel = null
);
