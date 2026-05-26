using LArtKey.Models;

namespace LArtKey.Services.InputLanguage;

/// <summary>
/// Connects keyboard slots to the active text input and prediction engine.
/// </summary>
public interface IInputLanguageModule
{
    string LanguageCode { get; }
    InputSubmode ActiveSubmode { get; }
    string ComposeStateLabel { get; }
    string CurrentWord { get; }

    event Action<IReadOnlyList<string>>? SuggestionsChanged;
    event Action<InputSubmode>? SubmodeChanged;

    bool HandleKey(KeySlot slot, KeyContext ctx);
    (int backspaceCount, string fullWord) AcceptSuggestion(string suggestion);
    void OnSeparator();
    void Reset();
    void ToggleSubmode();
    void CommitCurrentWord();
    void CancelComposition();
}