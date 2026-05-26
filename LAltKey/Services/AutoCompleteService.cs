using LAltKey.Models;
using LAltKey.Services.InputLanguage;

namespace LAltKey.Services;

/// <summary>
/// [text] text.
/// [text] text.
/// </summary>
public sealed class AutoCompleteService
{
    private readonly IInputLanguageModule _module; // text

    public AutoCompleteService(IInputLanguageModule module)
    {
        _module = module;
        // text.
        _module.SuggestionsChanged += list => SuggestionsChanged?.Invoke(list);
        _module.SubmodeChanged += submode => SubmodeChanged?.Invoke(submode);
    }

    /// text.
    public string CurrentWord => _module.CurrentWord;

    public event Action<IReadOnlyList<string>>? SuggestionsChanged;
    public event Action<InputSubmode>? SubmodeChanged;

    /// <summary>
    /// text.
    /// </summary>
    public bool OnKey(KeySlot slot, KeyContext ctx) => _module.HandleKey(slot, ctx);

    /// <summary>
    /// text.
    /// </summary>
    public (int backspaceCount, string fullWord) AcceptSuggestion(string suggestion)
        => _module.AcceptSuggestion(suggestion);

    /// text.
    public void OnSeparator() => _module.OnSeparator();

    /// text.
    public void ResetState() => _module.Reset();

    /// "text/A" text.
    public void ToggleInputMode() => _module.ToggleSubmode();

    /// text.
    public void CommitCurrentWord() => _module.CommitCurrentWord();
    
    /// text.
    public void CancelComposition() => _module.CancelComposition();

    public InputSubmode ActiveSubmode => _module.ActiveSubmode;
    public string ComposeStateLabel => _module.ComposeStateLabel;
}
