using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using LAltKey.Services;
using Microsoft.Extensions.DependencyInjection;

namespace LAltKey.Tools;

/// <summary>
/// [text] text → text.
/// [text] text.
/// </summary>
public partial class ProfileMappingEditorWindow : Window, INotifyPropertyChanged
{
    private readonly ConfigService _configService;
    private readonly LayoutService _layoutService;

    public ObservableCollection<ProfileMappingEditorRow> Rows { get; } = [];
    public ObservableCollection<string> AvailableLayouts { get; } = [];

    private string _validationSummaryText = string.Empty;
    public string ValidationSummaryText
    {
        get => _validationSummaryText;
        private set
        {
            if (_validationSummaryText == value) return;
            _validationSummaryText = value;
            OnPropertyChanged(nameof(ValidationSummaryText));
        }
    }

    public ProfileMappingEditorWindow()
    {
        InitializeComponent();

        _configService = App.Services.GetRequiredService<ConfigService>();
        _layoutService = App.Services.GetRequiredService<LayoutService>();

        DataContext = this;

        Rows.CollectionChanged += OnRowsCollectionChanged;
        Loaded += (_, _) => ProfileGrid.Focus();
        PreviewKeyDown += OnPreviewKeyDown;

        LoadFromConfig();
    }

    /// <summary>
    /// text.
    /// </summary>
    private void LoadFromConfig()
    {
        AvailableLayouts.Clear();
        foreach (var layout in _layoutService.GetAvailableLayouts().OrderBy(x => x))
        {
            AvailableLayouts.Add(layout);
        }

        Rows.Clear();
        foreach (var pair in _configService.Current.Profiles.OrderBy(p => p.Key))
        {
            var row = CreateRow(pair.Key, pair.Value);
            Rows.Add(row);
        }

        if (Rows.Count == 0)
        {
            Rows.Add(CreateRow(string.Empty, string.Empty));
        }

        UpdateRowStatuses();
    }

    private ProfileMappingEditorRow CreateRow(string processName, string layoutName)
    {
        var row = new ProfileMappingEditorRow(processName, layoutName);
        row.PropertyChanged += OnRowPropertyChanged;
        return row;
    }

    private void OnRowsCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (e.OldItems is not null)
        {
            foreach (var oldItem in e.OldItems.OfType<ProfileMappingEditorRow>())
            {
                oldItem.PropertyChanged -= OnRowPropertyChanged;
            }
        }

        if (e.NewItems is not null)
        {
            foreach (var newItem in e.NewItems.OfType<ProfileMappingEditorRow>())
            {
                newItem.PropertyChanged += OnRowPropertyChanged;
            }
        }
    }

    private void OnRowPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        UpdateRowStatuses();
    }

    /// <summary>
    /// text.
    /// </summary>
    private void UpdateRowStatuses()
    {
        var duplicated = Rows
            .Where(r => !string.IsNullOrWhiteSpace(r.ProcessName))
            .GroupBy(r => r.ProcessName.Trim().ToLowerInvariant())
            .Where(g => g.Count() > 1)
            .Select(g => g.Key)
            .ToHashSet();

        var emptyProcessCount = 0;
        var emptyLayoutCount = 0;
        var unknownLayoutCount = 0;
        var duplicateCount = 0;

        foreach (var row in Rows)
        {
            var processName = row.ProcessName?.Trim() ?? string.Empty;
            var layoutName = row.LayoutName?.Trim() ?? string.Empty;

            if (string.IsNullOrWhiteSpace(processName))
            {
                row.Status = "Missing process";
                emptyProcessCount++;
                continue;
            }

            if (duplicated.Contains(processName.ToLowerInvariant()))
            {
                row.Status = "Duplicate process";
                duplicateCount++;
                continue;
            }

            if (string.IsNullOrWhiteSpace(layoutName))
            {
                row.Status = "Missing layout";
                emptyLayoutCount++;
                continue;
            }

            if (!AvailableLayouts.Contains(layoutName))
            {
                row.Status = "Unknown layout";
                unknownLayoutCount++;
                continue;
            }

            row.Status = "OK";
        }

        ValidationSummaryText =
            $"Rows: {Rows.Count} | missing process: {emptyProcessCount} | missing layout: {emptyLayoutCount} | " +
            $"unknown layout: {unknownLayoutCount} | duplicate process: {duplicateCount}";
    }

    private void OnAddRow(object sender, RoutedEventArgs e)
    {
        Rows.Add(CreateRow(string.Empty, string.Empty));
        UpdateRowStatuses();
    }

    private void OnRemoveRow(object sender, RoutedEventArgs e)
    {
        if (sender is not Button { Tag: ProfileMappingEditorRow row })
        {
            return;
        }

        Rows.Remove(row);
        if (Rows.Count == 0)
        {
            Rows.Add(CreateRow(string.Empty, string.Empty));
        }

        UpdateRowStatuses();
    }

    /// <summary>
    /// text.
    /// </summary>
    private void OnSave(object sender, RoutedEventArgs e)
    {
        var validRows = Rows
            .Where(r => string.Equals(r.Status, "OK", StringComparison.Ordinal))
            .Select(r => new
            {
                ProcessName = r.ProcessName.Trim().ToLowerInvariant(),
                LayoutName = r.LayoutName.Trim()
            })
            .ToList();

        _configService.Update(c =>
        {
            c.Profiles = validRows.ToDictionary(x => x.ProcessName, x => x.LayoutName);
        });

        // text.
        ToolsReloadSignalService.NotifyReloadProfiles();

        MessageBox.Show(
            $"Saved {validRows.Count} profile mappings.",
            "Profile mappings",
            MessageBoxButton.OK,
            MessageBoxImage.Information);
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
/// [text] text.
/// </summary>
public sealed class ProfileMappingEditorRow : INotifyPropertyChanged
{
    private string _processName;
    private string _layoutName;
    private string _status;

    public string ProcessName
    {
        get => _processName;
        set
        {
            if (_processName == value) return;
            _processName = value;
            OnPropertyChanged(nameof(ProcessName));
        }
    }

    public string LayoutName
    {
        get => _layoutName;
        set
        {
            if (_layoutName == value) return;
            _layoutName = value;
            OnPropertyChanged(nameof(LayoutName));
        }
    }

    public string Status
    {
        get => _status;
        set
        {
            if (_status == value) return;
            _status = value;
            OnPropertyChanged(nameof(Status));
        }
    }

    public ProfileMappingEditorRow(string processName, string layoutName)
    {
        _processName = processName ?? string.Empty;
        _layoutName = layoutName ?? string.Empty;
        _status = "Ready";
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    private void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
