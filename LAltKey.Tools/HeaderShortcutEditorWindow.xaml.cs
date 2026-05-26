using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using LAltKey.Models;
using LAltKey.Services;
using LAltKey.ViewModels;
using Microsoft.Extensions.DependencyInjection;

namespace LAltKey.Tools;

/// <summary>
/// [text] text.
/// [text] text.
/// </summary>
public partial class HeaderShortcutEditorWindow : Window, INotifyPropertyChanged
{
    private readonly ConfigService _configService;
    private readonly string? _requestedHeaderButtonId;
    private string _editingId = "";
    private bool _isCreateMode;
    private string _windowTitle = "Header shortcut";
    private string _iconText = "";
    private string _tooltipText = "";
    private string _accessibleNameText = "";
    private string _selectedPosition = "Right";
    private bool _isHeaderButtonVisible = true;

    public event PropertyChangedEventHandler? PropertyChanged;

    public ActionBuilderViewModel ActionBuilder { get; } = new();
    public IReadOnlyList<string> Positions { get; } = ["Left", "Right"];

    public string WindowTitle
    {
        get => _windowTitle;
        private set
        {
            if (_windowTitle == value) return;
            _windowTitle = value;
            OnPropertyChanged(nameof(WindowTitle));
        }
    }

    public string IconText
    {
        get => _iconText;
        set
        {
            if (_iconText == value) return;
            _iconText = value;
            OnPropertyChanged(nameof(IconText));
        }
    }

    public string TooltipText
    {
        get => _tooltipText;
        set
        {
            if (_tooltipText == value) return;
            _tooltipText = value;
            OnPropertyChanged(nameof(TooltipText));
        }
    }

    public string AccessibleNameText
    {
        get => _accessibleNameText;
        set
        {
            if (_accessibleNameText == value) return;
            _accessibleNameText = value;
            OnPropertyChanged(nameof(AccessibleNameText));
        }
    }

    public string SelectedPosition
    {
        get => _selectedPosition;
        set
        {
            var normalized = HeaderButtonConfig.NormalizePosition(value);
            if (_selectedPosition == normalized) return;
            _selectedPosition = normalized;
            OnPropertyChanged(nameof(SelectedPosition));
        }
    }

    public bool IsHeaderButtonVisible
    {
        get => _isHeaderButtonVisible;
        set
        {
            if (_isHeaderButtonVisible == value) return;
            _isHeaderButtonVisible = value;
            OnPropertyChanged(nameof(IsHeaderButtonVisible));
        }
    }

    public HeaderShortcutEditorWindow(string? headerButtonId = null, bool createNew = false)
    {
        InitializeComponent();

        _configService = App.Services.GetRequiredService<ConfigService>();
        _requestedHeaderButtonId = headerButtonId;
        _isCreateMode = createNew;

        DataContext = this;

        Loaded += (_, _) => IconTextBox.Focus();
        PreviewKeyDown += OnPreviewKeyDown;

        LoadFromConfig();
    }

    /// <summary>
    /// text.
    /// </summary>
    private void LoadFromConfig()
    {
        var current = _configService.Current.HeaderButtons
            .FirstOrDefault(button => button.Kind == HeaderButtonKind.Custom && button.Id == _requestedHeaderButtonId);

        if (current is null && !_isCreateMode)
        {
            current = _configService.Current.HeaderButtons
                .FirstOrDefault(button => button.Kind == HeaderButtonKind.Custom);
        }

        current ??= HeaderButtonConfig.CreateCustomDefault();
        _isCreateMode = _isCreateMode || !_configService.Current.HeaderButtons.Any(button => button.Id == current.Id);

        _editingId = current.Id;
        WindowTitle = _isCreateMode ? "New header shortcut" : "Edit header shortcut";
        IconText = current.EffectiveIconText;
        TooltipText = current.EffectiveTooltip;
        AccessibleNameText = current.EffectiveAccessibleName;
        SelectedPosition = current.Position;
        IsHeaderButtonVisible = current.Visible;
        ActionBuilder.LoadFromAction(current.CustomAction);
    }

    /// <summary>
    /// text.
    /// </summary>
    private void OnSave(object sender, RoutedEventArgs e)
    {
        var savedButton = BuildEditedButton();
        var nextButtons = _configService.Current.HeaderButtons
            .Select(CloneHeaderButtonConfig)
            .ToList();

        var existingIndex = nextButtons.FindIndex(button => button.Id == _editingId);
        if (existingIndex >= 0)
        {
            nextButtons[existingIndex] = savedButton;
        }
        else
        {
            nextButtons.Add(savedButton);
        }

        if (!ValidateHeaderButtonLimits(nextButtons, savedButton, existingIndex >= 0))
        {
            return;
        }

        _configService.Update(config => config.HeaderButtons = nextButtons);
        ToolsReloadSignalService.NotifyReloadHeaderButtons();

        _editingId = savedButton.Id;
        _isCreateMode = false;
        WindowTitle = "Edit header shortcut";

        MessageBox.Show(
            "Header shortcut saved.",
            "Header shortcut",
            MessageBoxButton.OK,
            MessageBoxImage.Information);
    }

    /// <summary>
    /// text.
    /// </summary>
    private static bool ValidateHeaderButtonLimits(
        IReadOnlyCollection<HeaderButtonConfig> buttons,
        HeaderButtonConfig savedButton,
        bool isEditingExisting)
    {
        if (!isEditingExisting
            && HeaderButtonConfig.CountCustomButtons(buttons, savedButton.Id) >= HeaderButtonConfig.MaxCustomButtonCount)
        {
            MessageBox.Show(
                $"You can create up to {HeaderButtonConfig.MaxCustomButtonCount} custom header shortcuts.",
                "Header shortcut limit",
                MessageBoxButton.OK,
                MessageBoxImage.Information);
            return false;
        }

        if (!savedButton.Visible)
            return true;

        var maxVisibleButtons = HeaderButtonConfig.GetMaxVisibleButtons(savedButton.Position);
        var visibleButtonsOnSide = HeaderButtonConfig.CountVisibleButtons(buttons, savedButton.Position, savedButton.Id);
        if (visibleButtonsOnSide >= maxVisibleButtons)
        {
            var sideName = HeaderButtonConfig.NormalizePosition(savedButton.Position) == "Left" ? "left" : "right";
            MessageBox.Show(
                $"The {sideName} side can show up to {maxVisibleButtons} custom header shortcuts.\nHide another shortcut or move it to the other side.",
                "Header shortcut limit",
                MessageBoxButton.OK,
                MessageBoxImage.Information);
            return false;
        }

        return true;
    }

    private HeaderButtonConfig BuildEditedButton()
    {
        var action = ActionBuilder.BuildAction() ?? new SendKeyAction("VK_A");
        var tooltip = string.IsNullOrWhiteSpace(TooltipText) ? "Custom shortcut" : TooltipText.Trim();
        var accessibleName = string.IsNullOrWhiteSpace(AccessibleNameText) ? tooltip : AccessibleNameText.Trim();

        return new HeaderButtonConfig
        {
            Id = string.IsNullOrWhiteSpace(_editingId) ? HeaderButtonConfig.CreateCustomDefault().Id : _editingId,
            Kind = HeaderButtonKind.Custom,
            Visible = IsHeaderButtonVisible,
            Position = HeaderButtonConfig.NormalizePosition(SelectedPosition),
            DisplayMode = HeaderButtonDisplayMode.IconOnly,
            IconText = string.IsNullOrWhiteSpace(IconText) ? "A" : IconText.Trim(),
            Tooltip = tooltip,
            AccessibleName = accessibleName,
            CustomAction = CloneKeyAction(action)
        };
    }

    private static HeaderButtonConfig CloneHeaderButtonConfig(HeaderButtonConfig source) => new()
    {
        Id = source.Id,
        Kind = source.Kind,
        Visible = source.Visible,
        Position = HeaderButtonConfig.NormalizePosition(source.Position),
        DisplayMode = HeaderButtonDisplayMode.IconOnly,
        IconText = source.IconText,
        Tooltip = source.Tooltip,
        AccessibleName = source.AccessibleName,
        CustomAction = CloneKeyAction(source.CustomAction)
    };

    private static KeyAction? CloneKeyAction(KeyAction? action) => action switch
    {
        SendKeyAction sendKey => new SendKeyAction(sendKey.Vk),
        SendComboAction sendCombo => new SendComboAction(sendCombo.Keys.ToList()),
        ToggleStickyAction sticky => new ToggleStickyAction(sticky.Vk),
        SwitchLayoutAction switchLayout => new SwitchLayoutAction(switchLayout.Name),
        RunAppAction runApp => new RunAppAction(runApp.Path, runApp.Args),
        BoilerplateAction boilerplate => new BoilerplateAction(boilerplate.Text),
        ShellCommandAction shell => new ShellCommandAction(shell.Command, shell.Shell),
        VolumeControlAction volume => new VolumeControlAction(volume.Direction, volume.Step),
        ClipboardPasteAction clipboard => new ClipboardPasteAction(clipboard.Text),
        ToggleInputModeAction => new ToggleInputModeAction(),
        ToggleFunctionLayerAction => new ToggleFunctionLayerAction(),
        AiAction ai => new AiAction(ai.Prompt),
        _ => null
    };

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

    private void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
