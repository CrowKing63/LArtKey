using System.ComponentModel;
using System.Windows;
using System.Windows.Input;
using LArtKey.Services;
using Microsoft.Extensions.DependencyInjection;

namespace LArtKey.Tools;

/// <summary>
/// [text] AI tool.
/// [text] text.
/// </summary>
public partial class AiPromptEditorWindow : Window, INotifyPropertyChanged
{
    private readonly ConfigService _configService;
    private string _promptText = string.Empty;

    public string PromptText
    {
        get => _promptText;
        set
        {
            if (_promptText == value)
            {
                return;
            }

            _promptText = value;
            OnPropertyChanged(nameof(PromptText));
        }
    }

    public AiPromptEditorWindow()
    {
        InitializeComponent();

        _configService = App.Services.GetRequiredService<ConfigService>();
        DataContext = this;

        Loaded += (_, _) => PromptTextBox.Focus();
        PreviewKeyDown += OnPreviewKeyDown;

        LoadFromConfig();
    }

    /// <summary>
    /// text.
    /// </summary>
    private void LoadFromConfig()
    {
        PromptText = _configService.Current.AiDefaultPrompt ?? string.Empty;
    }

    /// <summary>
    /// text.
    /// </summary>
    private void OnSave(object sender, RoutedEventArgs e)
    {
        var nextPrompt = PromptText?.Trim() ?? string.Empty;

        _configService.Update(c => c.AiDefaultPrompt = nextPrompt);
        ToolsReloadSignalService.NotifyReloadAiSettings();

        MessageBox.Show(
            "AI prompt saved.",
            "AI prompt",
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
