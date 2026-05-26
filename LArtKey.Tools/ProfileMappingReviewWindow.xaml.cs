using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using LArtKey.Services;
using Microsoft.Extensions.DependencyInjection;

namespace LArtKey.Tools;

/// <summary>
/// [English text] English text.
/// [English text] English text.
/// </summary>
public partial class ProfileMappingReviewWindow : Window, INotifyPropertyChanged
{
    private readonly ConfigService _configService;
    private readonly LayoutService _layoutService;

    public ObservableCollection<ProfileMappingReviewEntry> Entries { get; } = [];

    private string _summaryText = string.Empty;
    public string SummaryText
    {
        get => _summaryText;
        private set
        {
            if (_summaryText == value) return;
            _summaryText = value;
            OnPropertyChanged(nameof(SummaryText));
        }
    }

    private string _riskText = string.Empty;
    public string RiskText
    {
        get => _riskText;
        private set
        {
            if (_riskText == value) return;
            _riskText = value;
            OnPropertyChanged(nameof(RiskText));
        }
    }

    private string _decisionText = string.Empty;
    public string DecisionText
    {
        get => _decisionText;
        private set
        {
            if (_decisionText == value) return;
            _decisionText = value;
            OnPropertyChanged(nameof(DecisionText));
        }
    }

    public ProfileMappingReviewWindow()
    {
        InitializeComponent();

        _configService = App.Services.GetRequiredService<ConfigService>();
        _layoutService = App.Services.GetRequiredService<LayoutService>();

        DataContext = this;

        // English text.
        Loaded += (_, _) => ReviewGrid.Focus();
        PreviewKeyDown += OnPreviewKeyDown;

        RefreshReview();
    }

    private void RefreshReview()
    {
        Entries.Clear();

        var config = _configService.Current;
        var availableLayouts = _layoutService.GetAvailableLayouts().ToHashSet(System.StringComparer.OrdinalIgnoreCase);

        var emptyLayoutCount = 0;
        var unknownLayoutCount = 0;

        foreach (var pair in config.Profiles.OrderBy(p => p.Key))
        {
            var processName = pair.Key?.Trim() ?? string.Empty;
            var layoutName = pair.Value?.Trim() ?? string.Empty;
            var status = "English text";

            if (string.IsNullOrWhiteSpace(layoutName))
            {
                status = "English text";
                emptyLayoutCount++;
            }
            else if (!availableLayouts.Contains(layoutName))
            {
                status = "English text";
                unknownLayoutCount++;
            }

            Entries.Add(new ProfileMappingReviewEntry(processName, layoutName, status));
        }

        SummaryText = $"English text {Entries.Count}English text {availableLayouts.Count}English text";
        RiskText = $"English text {emptyLayoutCount}English text {unknownLayoutCount}English text";

        // English text->English text.
        DecisionText =
            "English text. " +
            "English text.";
    }

    private void OnClose(object sender, RoutedEventArgs e)
    {
        Close();
    }

    private void OnPreviewKeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key != Key.Escape)
        {
            return;
        }

        e.Handled = true;
        Close();
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    private void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}

/// <summary>
/// [English text] English text.
/// </summary>
public sealed record ProfileMappingReviewEntry(string ProcessName, string LayoutName, string Status);
