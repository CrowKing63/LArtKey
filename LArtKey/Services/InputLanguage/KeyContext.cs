namespace LArtKey.Services.InputLanguage;

/// <summary>
/// Captures the keyboard state needed by the input module for one key press.
/// </summary>
public readonly record struct KeyContext(
    bool ShowUpperCase,
    bool HasActiveModifiers,
    bool HasActiveModifiersExcludingShift,
    InputMode InputMode,
    int TrackedOnScreenLength);