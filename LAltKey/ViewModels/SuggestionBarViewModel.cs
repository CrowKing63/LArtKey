using System.Collections.ObjectModel;
using System.Linq;
using LAltKey.Models;
using LAltKey.Services;
using LAltKey.Services.InputLanguage;
using WpfApp = System.Windows.Application;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace LAltKey.ViewModels;

public partial class SuggestionBarViewModel : ObservableObject
{
    private readonly AutoCompleteService _autoComplete;
    private readonly InputService        _inputService;
    private readonly ConfigService       _configService;
    private readonly EnglishDictionary   _enDict;

    [ObservableProperty]
    private ObservableCollection<string> suggestions = [];

    [ObservableProperty]
    private bool hasSuggestions;

    [ObservableProperty]
    private bool isVisible;

    [ObservableProperty]
    private string currentWord = "";

    [ObservableProperty]
    private bool hasCurrentWord;

    // [text][L3] text.
    [ObservableProperty]
    private ObservableCollection<ScanTargetVm> scanTargets = [];
    [ObservableProperty]
    private ObservableCollection<ScanTargetVm> suggestionScanTargets = [];

    // [text][L3] text.
    public event Action? ScanTargetsChanged;

    public SuggestionBarViewModel(
        AutoCompleteService autoComplete,
        InputService inputService,
        ConfigService configService,
        EnglishDictionary enDict)
    {
        _autoComplete = autoComplete;
        _inputService = inputService;
        _configService = configService;
        _enDict = enDict;
        _autoComplete.SuggestionsChanged += OnSuggestionsChanged;
        _configService.ConfigChanged += OnConfigChanged;

        SetVisibleFromConfig();
    }

    private void SetVisibleFromConfig()
    {
        IsVisible = _configService.Current.AutoCompleteEnabled;
        RebuildScanTargets();
    }

    private void OnConfigChanged(string? propertyName)
    {
        if (propertyName is null or nameof(AppConfig.AutoCompleteEnabled))
            SetVisibleFromConfig();
    }

    private void OnSuggestionsChanged(IReadOnlyList<string> newSuggestions)
    {
        string captured = _autoComplete.CurrentWord;
        void Apply()
        {
            CurrentWord = captured;
            HasCurrentWord = captured.Length > 0;
            Suggestions = new ObservableCollection<string>(newSuggestions);
            HasSuggestions = Suggestions.Count > 0;
            RebuildScanTargets();
        }

        var app = WpfApp.Current;
        if (app?.Dispatcher is null)
            Apply();
        else
            app.Dispatcher.Invoke(Apply);
    }

    private void RebuildScanTargets()
    {
        var nextTargets = new List<ScanTargetVm>();
        if (IsVisible)
        {
            if (HasCurrentWord && !string.IsNullOrWhiteSpace(CurrentWord))
            {
                nextTargets.Add(new ScanTargetVm
                {
                    DisplayText = CurrentWord,
                    Kind = "CurrentWord",
                    AccessibleName = $"Save current word {CurrentWord}",
                    Activate = () => CommitCurrentWordCommand.Execute(null),
                    SetScanFocused = isFocused => CurrentWordScanFocused = isFocused
                });
            }

            foreach (var suggestion in Suggestions)
            {
                string item = suggestion;
                nextTargets.Add(new ScanTargetVm
                {
                    DisplayText = item,
                    Kind = "Suggestion",
                    AccessibleName = $"Suggested word {item}",
                    Activate = () => AcceptSuggestionCommand.Execute(item),
                    SetScanFocused = isFocused =>
                    {
                        if (isFocused) FocusedSuggestion = item;
                        else if (FocusedSuggestion == item) FocusedSuggestion = "";
                    }
                });
            }
        }

        ScanTargets = new ObservableCollection<ScanTargetVm>(nextTargets);
        SuggestionScanTargets = new ObservableCollection<ScanTargetVm>(
            nextTargets.Where(t => t.Kind == "Suggestion"));
        ScanTargetsChanged?.Invoke();
    }

    [ObservableProperty]
    private bool currentWordScanFocused;

    [ObservableProperty]
    private string focusedSuggestion = "";

    [RelayCommand]
    private void AcceptSuggestion(string suggestion)
    {
        var (bsCount, fullWord) = _autoComplete.AcceptSuggestion(suggestion);
        if (_inputService.Mode == InputMode.Unicode)
        {
            _inputService.SendAtomicReplace(bsCount, fullWord);
            _inputService.ResetTrackedLength();
        }
        else
        {
            for (int i = 0; i < bsCount; i++)
                _inputService.SendKeyPress(VirtualKeyCode.VK_BACK);
            if (fullWord.Length > 0)
                _inputService.SendUnicode(fullWord);
        }
    }

    [RelayCommand]
    private void CommitCurrentWord()
    {
        _autoComplete.CommitCurrentWord();
        CurrentWord = "";
        HasCurrentWord = false;
        RebuildScanTargets();
    }

    [RelayCommand]
    private void CancelCurrentWord()
    {
        _autoComplete.CancelComposition();
        CurrentWord = "";
        HasCurrentWord = false;
        RebuildScanTargets();
    }

    [RelayCommand]
    private void RemoveSuggestion(string suggestion)
    {
        if (string.IsNullOrWhiteSpace(suggestion)) return;

        bool removed = _enDict.TryRemoveUserWord(suggestion);

        if (removed)
        {
            void Apply()
            {
                Suggestions.Remove(suggestion);
                HasSuggestions = Suggestions.Count > 0;
                RebuildScanTargets();
            }

            var app = WpfApp.Current;
            if (app?.Dispatcher is null)
                Apply();
            else
                app.Dispatcher.Invoke(Apply);
        }
    }
}
