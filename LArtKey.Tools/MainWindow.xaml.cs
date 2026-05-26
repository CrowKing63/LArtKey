using System.Windows;
using System.Windows.Input;
using LArtKey.ViewModels;
using LArtKey.Views;
using Microsoft.Extensions.DependencyInjection;

namespace LArtKey.Tools;

/// <summary>
/// [English text] LArtKey English text.
/// [English text] English text.
/// </summary>
public partial class MainWindow : Window
{
    private LayoutEditorWindow? _layoutEditorWindow;
    private UserDictionaryEditorWindow? _userDictionaryEditorWindow;
    private ProfileMappingEditorWindow? _profileMappingEditorWindow;
    private AiPromptEditorWindow? _aiPromptEditorWindow;
    private HeaderShortcutEditorWindow? _headerShortcutEditorWindow;

    public MainWindow()
    {
        InitializeComponent();

        // English text.
        Loaded += (_, _) => LayoutEditorButton.Focus();

        // English text.
        PreviewKeyDown += OnPreviewKeyDown;
    }

    /// <summary>
    /// English text: "layout", "dictionary", "profile", "ai-prompt", "header-shortcut"
    /// </summary>
    public void ApplyStartupArguments(string[] args)
    {
        if (args is null || args.Length == 0)
        {
            return;
        }

        var toolName = GetToolArgument(args);
        if (string.IsNullOrWhiteSpace(toolName))
        {
            return;
        }

        Loaded += (_, _) =>
        {
            var headerButtonId = GetArgumentValue(args, "--header-button-id");
            var createNewHeaderButton = string.Equals(GetArgumentValue(args, "--header-button-mode"), "create", StringComparison.OrdinalIgnoreCase);

            if (string.Equals(toolName, "layout", StringComparison.OrdinalIgnoreCase))
            {
                OpenLayoutEditorWindow(attachOwner: false);
                // English text.
                Close();
                return;
            }

            if (string.Equals(toolName, "dictionary", StringComparison.OrdinalIgnoreCase))
            {
                OpenUserDictionaryEditorWindow(attachOwner: false);
                Close();
                return;
            }

            if (string.Equals(toolName, "profile", StringComparison.OrdinalIgnoreCase))
            {
                OpenProfileMappingEditorWindow(attachOwner: false);
                Close();
                return;
            }

            if (string.Equals(toolName, "ai-prompt", StringComparison.OrdinalIgnoreCase))
            {
                OpenAiPromptEditorWindow(attachOwner: false);
                Close();
                return;
            }

            if (string.Equals(toolName, "header-shortcut", StringComparison.OrdinalIgnoreCase))
            {
                OpenHeaderShortcutEditorWindow(attachOwner: false, headerButtonId, createNewHeaderButton);
                Close();
            }
        };
    }

    /// <summary>
    /// "--tool layout" English text.
    /// </summary>
    private static string? GetToolArgument(string[] args)
    {
        for (var i = 0; i < args.Length; i++)
        {
            if (!string.Equals(args[i], "--tool", StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            if (i + 1 < args.Length)
            {
                return args[i + 1];
            }
        }

        return null;
    }

    private static string? GetArgumentValue(string[] args, string optionName)
    {
        for (var i = 0; i < args.Length; i++)
        {
            if (!string.Equals(args[i], optionName, StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            if (i + 1 < args.Length)
            {
                return args[i + 1];
            }
        }

        return null;
    }

    /// <summary>
    /// English text.
    /// </summary>
    private void OnOpenLayoutEditor(object sender, RoutedEventArgs e)
    {
        OpenLayoutEditorWindow(attachOwner: true);
    }

    /// <summary>
    /// English text.
    /// attachOwner=falseEnglish text.
    /// </summary>
    private void OpenLayoutEditorWindow(bool attachOwner)
    {
        if (_layoutEditorWindow is { IsLoaded: true })
        {
            _layoutEditorWindow.Activate();
            return;
        }

        var vm = App.Services.GetRequiredService<LayoutEditorViewModel>();
        _layoutEditorWindow = new LayoutEditorWindow(vm);
        if (attachOwner)
        {
            _layoutEditorWindow.Owner = this;
        }
        _layoutEditorWindow.Closed += (_, _) => _layoutEditorWindow = null;
        _layoutEditorWindow.Show();
    }

    /// <summary>
    /// English text.
    /// </summary>
    private void OnOpenUserDictionaryEditor(object sender, RoutedEventArgs e)
    {
        OpenUserDictionaryEditorWindow(attachOwner: true);
    }

    /// <summary>
    /// English text.
    /// attachOwner=falseEnglish text.
    /// </summary>
    private void OpenUserDictionaryEditorWindow(bool attachOwner)
    {
        if (_userDictionaryEditorWindow is { IsLoaded: true })
        {
            _userDictionaryEditorWindow.Activate();
            return;
        }

        var vm = App.Services.GetRequiredService<UserDictionaryEditorViewModel>();
        _userDictionaryEditorWindow = new UserDictionaryEditorWindow(vm);
        if (attachOwner)
        {
            _userDictionaryEditorWindow.Owner = this;
        }
        _userDictionaryEditorWindow.Closed += (_, _) => _userDictionaryEditorWindow = null;
        _userDictionaryEditorWindow.Show();
    }

    /// <summary>
    /// English text.
    /// </summary>
    private void OnOpenProfileMappingEditor(object sender, RoutedEventArgs e)
    {
        OpenProfileMappingEditorWindow(attachOwner: true);
    }

    /// <summary>
    /// English text.
    /// attachOwner=falseEnglish text.
    /// </summary>
    private void OpenProfileMappingEditorWindow(bool attachOwner)
    {
        if (_profileMappingEditorWindow is { IsLoaded: true })
        {
            _profileMappingEditorWindow.Activate();
            return;
        }

        _profileMappingEditorWindow = new ProfileMappingEditorWindow();
        if (attachOwner)
        {
            _profileMappingEditorWindow.Owner = this;
        }
        _profileMappingEditorWindow.Closed += (_, _) => _profileMappingEditorWindow = null;
        _profileMappingEditorWindow.Show();
    }

    /// <summary>
    /// AI English text.
    /// </summary>
    private void OnOpenAiPromptEditor(object sender, RoutedEventArgs e)
    {
        OpenAiPromptEditorWindow(attachOwner: true);
    }

    /// <summary>
    /// AI English text.
    /// attachOwner=falseEnglish text.
    /// </summary>
    private void OpenAiPromptEditorWindow(bool attachOwner)
    {
        if (_aiPromptEditorWindow is { IsLoaded: true })
        {
            _aiPromptEditorWindow.Activate();
            return;
        }

        _aiPromptEditorWindow = new AiPromptEditorWindow();
        if (attachOwner)
        {
            _aiPromptEditorWindow.Owner = this;
        }
        _aiPromptEditorWindow.Closed += (_, _) => _aiPromptEditorWindow = null;
        _aiPromptEditorWindow.Show();
    }

    /// <summary>
    /// English text.
    /// </summary>
    private void OnOpenHeaderShortcutEditor(object sender, RoutedEventArgs e)
    {
        OpenHeaderShortcutEditorWindow(attachOwner: true, headerButtonId: null, createNew: false);
    }

    private void OpenHeaderShortcutEditorWindow(bool attachOwner, string? headerButtonId, bool createNew)
    {
        if (_headerShortcutEditorWindow is { IsLoaded: true })
        {
            _headerShortcutEditorWindow.Activate();
            return;
        }

        _headerShortcutEditorWindow = new HeaderShortcutEditorWindow(headerButtonId, createNew);
        if (attachOwner)
        {
            _headerShortcutEditorWindow.Owner = this;
        }

        _headerShortcutEditorWindow.Closed += (_, _) => _headerShortcutEditorWindow = null;
        _headerShortcutEditorWindow.Show();
    }

    /// <summary>
    /// English text.
    /// </summary>
    private void OnClose(object sender, RoutedEventArgs e)
    {
        Close();
    }

    /// <summary>
    /// Esc English text.
    /// </summary>
    private void OnPreviewKeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key != Key.Escape)
        {
            return;
        }

        e.Handled = true;
        Close();
    }
}
