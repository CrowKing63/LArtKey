using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.Windows.Data;
using LArtKey.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using WpfMsgBox = System.Windows.MessageBox;
using WpfMsgBoxButton = System.Windows.MessageBoxButton;
using WpfMsgBoxImage = System.Windows.MessageBoxImage;
using WpfMsgBoxResult = System.Windows.MessageBoxResult;

namespace LArtKey.ViewModels;

public partial class UserDictionaryEditorViewModel : ObservableObject
{
    private readonly IUserDictionaryRepository _dictionaryRepository;

    [ObservableProperty]
    private ObservableCollection<WordEntryVm> words = [];

    [ObservableProperty]
    private string searchQuery = "";

    [ObservableProperty]
    private string newWord = "";

    // text.
    [ObservableProperty]
    private int newWordFrequency = 1;

    [ObservableProperty]
    private string statusText = "";

    [ObservableProperty]
    private ObservableCollection<BigramPairRow> bigramRows = [];

    [ObservableProperty]
    private BigramPairRow? selectedBigramRow;

    // text.
    [ObservableProperty]
    private string newBigramPrev = "";

    [ObservableProperty]
    private string newBigramNext = "";

    [ObservableProperty]
    private int newBigramCount = 1;

    [ObservableProperty]
    private bool isWordTabSelected = true;

    partial void OnIsWordTabSelectedChanged(bool value)
    {
        OnPropertyChanged(nameof(WordTabVisibility));
        OnPropertyChanged(nameof(BigramTabVisibility));
        OnPropertyChanged(nameof(IsBigramTabSelected));
    }

    public bool IsBigramTabSelected
    {
        get => !IsWordTabSelected;
        set
        {
            if (value && IsWordTabSelected)
            {
                IsWordTabSelected = false;
            }
        }
    }

    public Visibility WordTabVisibility => IsWordTabSelected ? Visibility.Visible : Visibility.Collapsed;
    public Visibility BigramTabVisibility => IsWordTabSelected ? Visibility.Collapsed : Visibility.Visible;

    private bool _isPrimaryTabActive = true;
    public bool IsPrimaryTabActive
    {
        get => _isPrimaryTabActive;
        set
        {
            if (SetProperty(ref _isPrimaryTabActive, value) && value)
                SwitchTab(korean: true);
        }
    }
    public bool IsEnglishTabActive
    {
        get => !_isPrimaryTabActive;
        set
        {
            if (value && _isPrimaryTabActive)
            {
                _isPrimaryTabActive = false;
                OnPropertyChanged(nameof(IsPrimaryTabActive));
                OnPropertyChanged(nameof(IsEnglishTabActive));
                SwitchTab(korean: false);
            }
        }
    }

    public ICollectionView FilteredWords { get; }

    public UserDictionaryEditorViewModel(IUserDictionaryRepository dictionaryRepository)
    {
        _dictionaryRepository = dictionaryRepository;
        _dictionaryRepository.SelectLanguage(korean: true);

        FilteredWords = CollectionViewSource.GetDefaultView(Words);
        FilteredWords.Filter = obj =>
        {
            if (obj is not WordEntryVm entry) return false;
            if (string.IsNullOrWhiteSpace(SearchQuery)) return true;
            return entry.Word.Contains(SearchQuery, StringComparison.OrdinalIgnoreCase);
        };
    }

    public void OnLoaded()
    {
        SearchQuery = "";
        _isPrimaryTabActive = true;
        OnPropertyChanged(nameof(IsPrimaryTabActive));
        OnPropertyChanged(nameof(IsEnglishTabActive));
        _dictionaryRepository.SelectLanguage(korean: true);
        ReloadWords();
        LoadBigrams();
    }

    public void OnClosing()
    {
        _dictionaryRepository.Flush();
    }

    partial void OnSearchQueryChanged(string value)
    {
        FilteredWords.Refresh();
        UpdateStatus();
    }

    private void SwitchTab(bool korean)
    {
        _dictionaryRepository.SelectLanguage(korean);
        ReloadWords();
        LoadBigrams();
    }

    private void ReloadWords()
    {
        Words.Clear();
        foreach (var (w, f) in _dictionaryRepository.GetAllWords())
        {
            var entry = new WordEntryVm(w, f);
            entry.FrequencyChanged += OnEntryFrequencyChanged;
            Words.Add(entry);
        }
        FilteredWords.Refresh();
        UpdateStatus();
    }

    private void OnEntryFrequencyChanged(WordEntryVm entry)
    {
        _dictionaryRepository.SetWordFrequency(entry.Word, entry.Frequency);
        ToolsReloadSignalService.NotifyReloadUserDictionary();
        if (entry.Frequency <= 0)
        {
            Words.Remove(entry);
            FilteredWords.Refresh();
            UpdateStatus();
        }
    }

    [RelayCommand]
    private void AddWord()
    {
        var w = NewWord.Trim();
        if (w.Length == 0) return;

        var normalized = _dictionaryRepository.NormalizeWord(w);
        var appliedFrequency = Math.Max(1, NewWordFrequency);
        _dictionaryRepository.SetWordFrequency(normalized, GetFrequencyOrDefault(normalized, appliedFrequency));
        ToolsReloadSignalService.NotifyReloadUserDictionary();
        NewWord = "";
        NewWordFrequency = 1;
        ReloadWords();
    }

    private int GetFrequencyOrDefault(string word, int fallback)
    {
        var existing = _dictionaryRepository.GetAllWords()
            .FirstOrDefault(p => p.Word == word);
        return existing.Word == null ? fallback : existing.Frequency + 1;
    }

    [RelayCommand]
    private void RemoveOne(WordEntryVm entry)
    {
        if (entry is null) return;
        _dictionaryRepository.RemoveWord(entry.Word);
        ToolsReloadSignalService.NotifyReloadUserDictionary();
        Words.Remove(entry);
        FilteredWords.Refresh();
        UpdateStatus();
    }

    [RelayCommand]
    private void RemoveSelected()
    {
        var toRemove = Words.Where(w => w.IsSelected).ToList();
        if (toRemove.Count == 0) return;

        var result = WpfMsgBox.Show(
            $"Delete {toRemove.Count} selected words?",
            "Custom shortcut",
            WpfMsgBoxButton.YesNo,
            WpfMsgBoxImage.Question);
        if (result != WpfMsgBoxResult.Yes) return;

        foreach (var entry in toRemove)
        {
            _dictionaryRepository.RemoveWord(entry.Word);
            Words.Remove(entry);
        }
        ToolsReloadSignalService.NotifyReloadUserDictionary();
        FilteredWords.Refresh();
        UpdateStatus();
    }

    [RelayCommand]
    private void ClearAll()
    {
        var label = _isPrimaryTabActive ? "Custom shortcut" : "Custom shortcut";
        var result = WpfMsgBox.Show(
            $"Delete all {label} entries ({Words.Count} items)?\nThis cannot be undone.",
            "Custom shortcut",
            WpfMsgBoxButton.YesNo,
            WpfMsgBoxImage.Warning);
        if (result != WpfMsgBoxResult.Yes) return;

        _dictionaryRepository.ClearWords();
        ToolsReloadSignalService.NotifyReloadUserDictionary();
        Words.Clear();
        FilteredWords.Refresh();
        UpdateStatus();
    }

    private void UpdateStatus()
    {
        int total = Words.Count;
        int shown = FilteredWords.Cast<object>().Count();
        StatusText = string.IsNullOrWhiteSpace(SearchQuery)
            ? $"Total {total} entries"
            : $"Total {total} entries, showing {shown}";
    }

    public void LoadBigrams()
    {
        var pairs = _dictionaryRepository.GetAllBigrams();
        BigramRows.Clear();
        foreach (var (prev, next, count) in pairs)
            BigramRows.Add(new BigramPairRow(prev, next, count));
    }

    [RelayCommand]
    private void RemoveBigramPair(BigramPairRow row)
    {
        if (row is null) return;
        if (_dictionaryRepository.RemoveBigramPair(row.Prev, row.Next))
        {
            ToolsReloadSignalService.NotifyReloadBigramData();
            BigramRows.Remove(row);
        }
    }

    [RelayCommand]
    private void RemoveBigramsByPrev(BigramPairRow row)
    {
        if (row is null) return;
        int removed = _dictionaryRepository.RemoveAllBigramsFor(row.Prev);
        if (removed > 0)
        {
            ToolsReloadSignalService.NotifyReloadBigramData();
            for (int i = BigramRows.Count - 1; i >= 0; i--)
                if (BigramRows[i].Prev == row.Prev) BigramRows.RemoveAt(i);
        }
    }

    [RelayCommand]
    private void ClearAllBigrams()
    {
        var label = _isPrimaryTabActive ? "Custom shortcut" : "Custom shortcut";
        var result = WpfMsgBox.Show(
            $"Delete all {label} data?\nThis cannot be undone.",
            "Custom shortcut",
            WpfMsgBoxButton.YesNo,
            WpfMsgBoxImage.Warning);
        if (result != WpfMsgBoxResult.Yes) return;

        _dictionaryRepository.ClearBigrams();
        ToolsReloadSignalService.NotifyReloadBigramData();
        BigramRows.Clear();
    }

    [RelayCommand]
    private void AddBigramPair()
    {
        var prev = _dictionaryRepository.NormalizeWord(NewBigramPrev);
        var next = _dictionaryRepository.NormalizeWord(NewBigramNext);
        var count = Math.Max(1, NewBigramCount);

        if (string.IsNullOrWhiteSpace(prev) || string.IsNullOrWhiteSpace(next))
        {
            return;
        }

        _dictionaryRepository.SetBigramCount(prev, next, count);
        ToolsReloadSignalService.NotifyReloadBigramData();

        NewBigramPrev = "";
        NewBigramNext = "";
        NewBigramCount = 1;
        LoadBigrams();
    }
}

public sealed record BigramPairRow(string Prev, string Next, int Count);

public partial class WordEntryVm : ObservableObject
{
    public string Word { get; }

    private int _frequency;
    public int Frequency
    {
        get => _frequency;
        set
        {
            if (SetProperty(ref _frequency, value))
                FrequencyChanged?.Invoke(this);
        }
    }

    [ObservableProperty]
    private bool isSelected;

    public event Action<WordEntryVm>? FrequencyChanged;

    public WordEntryVm(string word, int frequency)
    {
        Word = word;
        _frequency = frequency;
    }
}
