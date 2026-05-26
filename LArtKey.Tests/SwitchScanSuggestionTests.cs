using LArtKey.Models;
using LArtKey.Services;
using LArtKey.Services.InputLanguage;
using LArtKey.Tests.InputLanguage;
using LArtKey.ViewModels;

namespace LArtKey.Tests;

public class SwitchScanSuggestionTests
{
    [Fact]
    public void SuggestionBar_scan_targets_include_current_word_and_suggestions()
    {
        var module = new FakeInputLanguageModule();
        var autoComplete = new AutoCompleteService(module);
        var config = new ConfigService();
        config.Current.AutoCompleteEnabled = true;
        var vm = new SuggestionBarViewModel(
            autoComplete,
            new FakeInputService(),
            config,
            new EnglishDictionaryTestable());

        module.SetState("he", ["hello", "help"]);

        Assert.Equal(3, vm.ScanTargets.Count);
        Assert.Equal("CurrentWord", vm.ScanTargets[0].Kind);
        Assert.Equal("Suggestion", vm.ScanTargets[1].Kind);
        Assert.Equal("Suggestion", vm.ScanTargets[2].Kind);
    }

    [Fact]
    public void SuggestionBar_scan_targets_empty_when_autocomplete_off()
    {
        var module = new FakeInputLanguageModule();
        var config = new ConfigService();
        config.Current.AutoCompleteEnabled = false;
        var autoComplete = new AutoCompleteService(module);
        var vm = new SuggestionBarViewModel(
            autoComplete,
            new FakeInputService(),
            config,
            new EnglishDictionaryTestable());

        module.SetState("he", ["hello"]);

        Assert.Empty(vm.ScanTargets);
    }

    private sealed class FakeInputLanguageModule : IInputLanguageModule
    {
        public string LanguageCode => "en";
        public InputSubmode ActiveSubmode => InputSubmode.QuietEnglish;
        public string ComposeStateLabel => "A";
        public string CurrentWord { get; private set; } = "";

        public event Action<IReadOnlyList<string>>? SuggestionsChanged;

        // This test does not exercise submode changes; the event only satisfies the interface.
        public event Action<InputSubmode>? SubmodeChanged
        {
            add { }
            remove { }
        }

        public bool HandleKey(KeySlot slot, KeyContext ctx) => false;
        public (int backspaceCount, string fullWord) AcceptSuggestion(string suggestion) => (0, suggestion);
        public void ToggleSubmode() { }
        public void OnSeparator() { }
        public void Reset() { }
        public void CommitCurrentWord() { }
        public void CancelComposition() { }

        public void SetState(string currentWord, IReadOnlyList<string> suggestions)
        {
            CurrentWord = currentWord;
            SuggestionsChanged?.Invoke(suggestions);
        }
    }
}
