using LAltKey.Models;

namespace LAltKey.Services.InputLanguage;

/// <summary>
/// Handles text input, word prediction, and learned bigram context for LAltKey.
/// </summary>
public sealed class EnglishInputModule : IInputLanguageModule
{
    private readonly InputService _input;
    private readonly EnglishDictionary _dictionary;
    private readonly ConfigService _config;

    private string _prefix = string.Empty;
    private string? _lastCommittedWord;
    private string? _suggestionContext;
    private bool _compositionCancelled;
    private bool _wordAlreadyCommitted;

    public EnglishInputModule(InputService input, EnglishDictionary dictionary, ConfigService config)
    {
        _input = input;
        _dictionary = dictionary;
        _config = config;
    }

    public string LanguageCode => "en";
    public InputSubmode ActiveSubmode => InputSubmode.QuietEnglish;
    public string ComposeStateLabel => "A";
    public string CurrentWord => _prefix;

    public event Action<IReadOnlyList<string>>? SuggestionsChanged;
    public event Action<InputSubmode>? SubmodeChanged;

    public bool HandleKey(KeySlot slot, KeyContext ctx)
    {
        if (slot.Action is not SendKeyAction { Vk: var vkText }
            || !Enum.TryParse<VirtualKeyCode>(vkText, out var vk))
        {
            return false;
        }

        if (vk == VirtualKeyCode.VK_BACK)
        {
            return HandleBackspace();
        }

        if (IsSeparator(vk, ctx.ShowUpperCase))
        {
            FinalizeComposition(keepContextForBigram: vk is VirtualKeyCode.VK_SPACE);
            return false;
        }

        var ch = GetEnglishCharFromSlot(slot, ctx.ShowUpperCase);
        if (ctx.HasActiveModifiersExcludingShift || ctx.InputMode == InputMode.VirtualKey)
        {
            TrackEnglishKey(ch != '\0' ? ch : VkToEnglishChar(vk, ctx.ShowUpperCase));
            return false;
        }

        if (ch == '\0')
        {
            ch = VkToEnglishChar(vk, ctx.ShowUpperCase);
        }

        if (ch == '\0')
        {
            return false;
        }

        TrackEnglishKey(ch);
        _input.SendUnicode(ch.ToString());
        return true;
    }

    public (int backspaceCount, string fullWord) AcceptSuggestion(string suggestion)
    {
        var backspaceCount = _prefix.Length;
        if (_config.Current.AutoCompleteEnabled)
        {
            _dictionary.RecordWord(suggestion);
            if (_lastCommittedWord is { Length: > 0 })
            {
                _dictionary.RecordBigram(_lastCommittedWord, suggestion);
            }
        }

        _prefix = string.Empty;
        _lastCommittedWord = suggestion;
        _suggestionContext = null;
        _compositionCancelled = false;
        _wordAlreadyCommitted = true;
        SuggestionsChanged?.Invoke(Array.Empty<string>());
        return (backspaceCount, suggestion);
    }

    public void OnSeparator() => FinalizeComposition(keepContextForBigram: true);

    public void Reset()
    {
        _prefix = string.Empty;
        _lastCommittedWord = null;
        _suggestionContext = null;
        _compositionCancelled = false;
        _wordAlreadyCommitted = false;
        SuggestionsChanged?.Invoke(Array.Empty<string>());
        SubmodeChanged?.Invoke(InputSubmode.QuietEnglish);
    }

    public void ToggleSubmode()
    {
        FinalizeComposition();
        SubmodeChanged?.Invoke(InputSubmode.QuietEnglish);
    }

    public void CommitCurrentWord()
    {
        if (_compositionCancelled || _prefix.Length < 2)
        {
            ResetAfterCommit();
            return;
        }

        if (_config.Current.AutoCompleteEnabled)
        {
            _dictionary.RecordWord(_prefix);
            if (_lastCommittedWord is { Length: > 0 })
            {
                _dictionary.RecordBigram(_lastCommittedWord, _prefix);
            }
        }

        _lastCommittedWord = _prefix;
        _wordAlreadyCommitted = true;
        ResetAfterCommit();
    }

    public void CancelComposition()
    {
        _compositionCancelled = true;
        _prefix = string.Empty;
        _lastCommittedWord = null;
        _suggestionContext = null;
        _input.ResetTrackedLength();
        SuggestionsChanged?.Invoke(Array.Empty<string>());
    }

    private bool HandleBackspace()
    {
        if (_prefix.Length > 0)
        {
            _prefix = _prefix[..^1];
            SuggestionsChanged?.Invoke(_dictionary.GetSuggestions(_prefix, _suggestionContext));
        }
        return false;
    }

    private void TrackEnglishKey(char ch)
    {
        if (ch == '\0') return;

        _compositionCancelled = false;
        _wordAlreadyCommitted = false;
        _prefix += ch;
        SuggestionsChanged?.Invoke(_dictionary.GetSuggestions(_prefix, _suggestionContext));
    }

    private void FinalizeComposition(bool keepContextForBigram = false)
    {
        if (_prefix.Length == 0 && _lastCommittedWord is null) return;

        var learningEnabled = _config.Current.AutoCompleteEnabled && !_compositionCancelled && !_wordAlreadyCommitted;
        string? committed = null;

        if (_prefix.Length >= 2)
        {
            committed = _prefix;
            if (learningEnabled)
            {
                _dictionary.RecordWord(_prefix);
                if (_lastCommittedWord is { Length: > 0 })
                {
                    _dictionary.RecordBigram(_lastCommittedWord, _prefix);
                }
            }
        }

        if (committed is not null) _lastCommittedWord = committed;

        _prefix = string.Empty;
        _compositionCancelled = false;
        _wordAlreadyCommitted = false;

        if (keepContextForBigram && _lastCommittedWord is { Length: > 0 })
        {
            _suggestionContext = _lastCommittedWord;
            SuggestionsChanged?.Invoke(_dictionary.GetSuggestions(string.Empty, _lastCommittedWord));
        }
        else
        {
            _suggestionContext = null;
            _lastCommittedWord = null;
            SuggestionsChanged?.Invoke(Array.Empty<string>());
        }

        _input.ResetTrackedLength();
    }

    private void ResetAfterCommit()
    {
        _suggestionContext = null;
        _prefix = string.Empty;
        _input.ResetTrackedLength();
        SuggestionsChanged?.Invoke(Array.Empty<string>());
    }

    private static char GetEnglishCharFromSlot(KeySlot slot, bool showUpperCase)
    {
        var label = showUpperCase && slot.ShiftLabel is { Length: > 0 } ? slot.ShiftLabel : slot.Label;
        if (label is { Length: 1 } && label[0] < 128)
        {
            return showUpperCase ? char.ToUpperInvariant(label[0]) : label[0];
        }
        return '\0';
    }

    private static bool IsSeparator(VirtualKeyCode vk, bool isShifted)
    {
        if (vk is VirtualKeyCode.VK_SPACE or VirtualKeyCode.VK_RETURN or VirtualKeyCode.VK_TAB
            or VirtualKeyCode.VK_OEM_PERIOD or VirtualKeyCode.VK_OEM_COMMA or VirtualKeyCode.VK_ESCAPE
            or VirtualKeyCode.VK_DELETE or VirtualKeyCode.VK_OEM_7 or VirtualKeyCode.VK_OEM_4
            or VirtualKeyCode.VK_OEM_6 or VirtualKeyCode.VK_OEM_1)
        {
            return true;
        }
        return isShifted && vk is VirtualKeyCode.VK_1 or VirtualKeyCode.VK_OEM_2 or VirtualKeyCode.VK_9 or VirtualKeyCode.VK_0;
    }

    private static char VkToEnglishChar(VirtualKeyCode vk, bool upperCase)
    {
        if (vk >= VirtualKeyCode.VK_A && vk <= VirtualKeyCode.VK_Z)
        {
            var c = (char)('a' + ((int)vk - (int)VirtualKeyCode.VK_A));
            return upperCase ? char.ToUpperInvariant(c) : c;
        }
        return '\0';
    }
}