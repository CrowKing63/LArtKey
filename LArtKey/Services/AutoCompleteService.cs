using LArtKey.Models;
using LArtKey.Services.InputLanguage;

namespace LArtKey.Services;

/// <summary>
/// [English text] English text.
/// [English text] English text.
/// </summary>
public sealed class AutoCompleteService
{
    private readonly IInputLanguageModule _module; // English text

    public AutoCompleteService(IInputLanguageModule module)
    {
        _module = module;
        // English text.
        _module.SuggestionsChanged += list => SuggestionsChanged?.Invoke(list);
        _module.SubmodeChanged += submode => SubmodeChanged?.Invoke(submode);
    }

    /// English text.
    public string CurrentWord => _module.CurrentWord;

    public event Action<IReadOnlyList<string>>? SuggestionsChanged;
    public event Action<InputSubmode>? SubmodeChanged;

    /// <summary>
    /// English text.
    /// </summary>
    public bool OnKey(KeySlot slot, KeyContext ctx) => _module.HandleKey(slot, ctx);

    /// <summary>
    /// English text.
    /// </summary>
    public (int backspaceCount, string fullWord) AcceptSuggestion(string suggestion)
        => _module.AcceptSuggestion(suggestion);

    /// English text.
    public void OnSeparator() => _module.OnSeparator();

    /// English text.
    public void ResetState() => _module.Reset();

    /// "English text/A" English text.
    public void ToggleInputMode() => _module.ToggleSubmode();

    /// English text.
    public void CommitCurrentWord() => _module.CommitCurrentWord();
    
    /// English text.
    public void CancelComposition() => _module.CancelComposition();

    public InputSubmode ActiveSubmode => _module.ActiveSubmode;
    public string ComposeStateLabel => _module.ComposeStateLabel;
}
