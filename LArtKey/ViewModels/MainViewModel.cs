using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using LArtKey.Models;
using LArtKey.Platform;
using Microsoft.Extensions.DependencyInjection;
using WpfApp = System.Windows.Application;
using WpfMsgBox = System.Windows.MessageBox;
using WpfMsgBoxButton = System.Windows.MessageBoxButton;
using WpfMsgBoxImage = System.Windows.MessageBoxImage;
using LArtKey.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace LArtKey.ViewModels;

/// <summary>
/// [English text] LArtKeyEnglish text 'English text'English text.
/// [English text] English text.
/// </summary>
public partial class MainViewModel : ObservableObject
{
    private readonly ConfigService  _configService; // English text
    private readonly LayoutService  _layoutService;
    private readonly ProfileService _profileService;
    private readonly AutoCompleteService _autoCompleteService;
    private readonly InputService _inputService;
    private readonly OskLauncherService _oskLauncher;
    private readonly LiveRegionService _liveRegion;
    private readonly AiService _aiService;

    // English text → English text)
    private readonly Dictionary<string, string> _displayToFileName = [];
    // SwitchLayout English text
    private bool _isSwitching;

    public KeyboardViewModel       Keyboard    { get; }
    public SettingsViewModel       Settings    { get; }
    public EmojiViewModel          Emoji       { get; }
    public ClipboardViewModel      Clipboard   { get; }
    public SuggestionBarViewModel  AutoComplete { get; }

    [ObservableProperty]
    private string currentLayoutName = "";

    [ObservableProperty]
    private ObservableCollection<string> availableLayouts = [];

    // T-9.5: English text
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasUpdate))]
    [NotifyPropertyChangedFor(nameof(UpdateIndicatorTooltip))]
    private string? updateVersion;

    [ObservableProperty]
    private string? updateUrl;

    [ObservableProperty]
    private string? updateInstallerUrl;

    /// English text)
    public bool HasUpdate => UpdateVersion is not null;

    /// English text
    public string UpdateIndicatorTooltip => IsDownloading
        ? $"English text... {DownloadProgress:P0}"
        : $"English text {UpdateVersion} — English text";

    /// T-9.5: English text
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(UpdateIndicatorTooltip))]
    private bool isDownloading;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(UpdateIndicatorTooltip))]
    private double downloadProgress;

    [ObservableProperty]
    private bool isInstalling;

    [ObservableProperty]
    private string? updateStatusMessage;

    /// T-5.1: English text)
    public bool DwellEnabled
    {
        get => _configService.Current.DwellEnabled;
        set
        {
            _configService.Current.DwellEnabled = value;
            OnPropertyChanged();
        }
    }

    public bool AutoCompleteEnabled
    {
        get => _configService.Current.AutoCompleteEnabled;
        set
        {
            if (_configService.Current.AutoCompleteEnabled == value) return;

            var target = value ? InputMode.Unicode : InputMode.VirtualKey;
            bool ok = _inputService.TrySetMode(target);

            if (!ok && value)
            {
                System.Media.SystemSounds.Beep.Play();
                _liveRegion.Announce("English text");
                OnPropertyChanged(nameof(AutoCompleteEnabled));
                return;
            }

            _configService.Update(c => c.AutoCompleteEnabled = value);
            OnPropertyChanged();

            _autoCompleteService.ResetState();
            AutoComplete.IsVisible = _configService.Current.AutoCompleteEnabled;

            _liveRegion.Announce(value ? "English text" : "English text");
        }
    }

    public bool CanToggleAutoComplete => !_inputService.IsElevated;

    /// T-5.1: English text)
    public int DwellTimeMs
    {
        get => _configService.Current.DwellTimeMs;
        set
        {
            _configService.Current.DwellTimeMs = value;
            OnPropertyChanged();
        }
    }

    /// T-10: English text)
    public bool KeyRepeatEnabled
    {
        get => _configService.Current.KeyRepeatEnabled;
        set
        {
            _configService.Current.KeyRepeatEnabled = value;
            OnPropertyChanged();
        }
    }

    /// T-10: English text)
    public int KeyRepeatDelayMs
    {
        get => _configService.Current.KeyRepeatDelayMs;
        set
        {
            _configService.Current.KeyRepeatDelayMs = value;
            OnPropertyChanged();
        }
    }

    /// T-10: English text)
    public int KeyRepeatIntervalMs
    {
        get => _configService.Current.KeyRepeatIntervalMs;
        set
        {
            _configService.Current.KeyRepeatIntervalMs = value;
            OnPropertyChanged();
        }
    }

    /// L1: English text)
    public bool KeyboardA11yNavigationEnabled
    {
        get => _configService.Current.KeyboardA11yNavigationEnabled;
        set
        {
            _configService.Current.KeyboardA11yNavigationEnabled = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// [English text] English text.
    /// </summary>
    public A11yFocusOwner A11yFocusOwner => Keyboard.A11yFocusOwner;

    /// L2: English text)
    public bool ReducedMotionEnabled
    {
        get => _configService.Current.ReducedMotionEnabled;
        set
        {
            _configService.Current.ReducedMotionEnabled = value;
            OnPropertyChanged();
        }
    }

    /// L2: TTS English text)
    public bool TtsOnHover
    {
        get => _configService.Current.TtsOnHover;
        set
        {
            _configService.Current.TtsOnHover = value;
            OnPropertyChanged();
        }
    }

    public MainViewModel(
        ConfigService          configService,
        LayoutService          layoutService,
        KeyboardViewModel      keyboardViewModel,
        ProfileService         profileService,
        SettingsViewModel      settingsViewModel,
        EmojiViewModel         emojiViewModel,
        ClipboardViewModel     clipboardViewModel,
        SuggestionBarViewModel suggestionBarViewModel,
        AutoCompleteService    autoCompleteService,
        InputService           inputService,
        OskLauncherService     oskLauncher,
        LiveRegionService      liveRegion,
        AiService              aiService)
    {
        _configService  = configService;
        _layoutService  = layoutService;
        _profileService = profileService;
        _liveRegion = liveRegion;
        _aiService = aiService;

        Keyboard     = keyboardViewModel;
        Settings     = settingsViewModel;
        Emoji        = emojiViewModel;
        Clipboard    = clipboardViewModel;
        AutoComplete = suggestionBarViewModel;
        _autoCompleteService = autoCompleteService;
        _inputService = inputService;
        _oskLauncher = oskLauncher;

        _profileService.ForegroundAppChanged += OnForegroundAppChanged;
        _configService.ConfigChanged += OnConfigChanged;
        _inputService.SpecialActionRequested += OnSpecialActionRequested;
        Keyboard.PropertyChanged += (_, e) =>
        {
            if (e.PropertyName == nameof(KeyboardViewModel.A11yFocusOwner))
                OnPropertyChanged(nameof(A11yFocusOwner));
        };
        _layoutService.LayoutsChanged += OnLayoutsChanged;
        Keyboard.KeyTapped += () =>
        {
            Emoji.IsVisible     = false;
            Clipboard.IsVisible = false;
        };

        // English text
        RebuildHeaderButtons();
    }

    private void OnConfigChanged(string? propertyName)
    {
        if (propertyName == "DefaultLayout")
        {
            SwitchLayout(_configService.Current.DefaultLayout);
            return;
        }
        if (propertyName is "HeaderButtons" or nameof(AppConfig.AiEnabled))
            RebuildHeaderButtons();

        OnPropertyChanged(nameof(DwellEnabled));
        OnPropertyChanged(nameof(DwellTimeMs));
        OnPropertyChanged(nameof(AutoCompleteEnabled));
        OnPropertyChanged(nameof(AiEnabled));
        OnPropertyChanged(nameof(KeyRepeatEnabled));
        OnPropertyChanged(nameof(KeyRepeatDelayMs));
        OnPropertyChanged(nameof(KeyRepeatIntervalMs));
        OnPropertyChanged(nameof(KeyboardA11yNavigationEnabled));
        OnPropertyChanged(nameof(ReducedMotionEnabled));
        OnPropertyChanged(nameof(TtsOnHover));
    }

    private void OnLayoutsChanged()
    {
        _layoutService.InvalidateCache();
        _displayToFileName.Clear();
        var fileNames    = _layoutService.GetAvailableLayouts();
        var displayNames = new List<string>();
        foreach (var fn in fileNames)
        {
            var l = _layoutService.TryLoad(fn);
            var display = l?.Name ?? fn;
            _displayToFileName[display] = fn;
            displayNames.Add(display);
        }
        AvailableLayouts = new ObservableCollection<string>(displayNames);

        // English text
        if (!string.IsNullOrEmpty(CurrentLayoutName))
            SwitchLayout(CurrentLayoutName);
    }

    public Task InitializeAsync()
    {
        // T-7.1: English text → English text
        _displayToFileName.Clear();
        var fileNames    = _layoutService.GetAvailableLayouts();
        var displayNames = new List<string>();
        foreach (var fn in fileNames)
        {
            var l = _layoutService.TryLoad(fn);
            var display = l?.Name ?? fn;
            _displayToFileName[display] = fn;
            displayNames.Add(display);
        }
        AvailableLayouts = new ObservableCollection<string>(displayNames);

        var defaultName = _configService.Current.DefaultLayout;
        if (!fileNames.Contains(defaultName) && fileNames.Count > 0)
        {
            var fallback = fileNames[0];
            System.Diagnostics.Debug.WriteLine($"English text '{defaultName}'English text '{fallback}'(English text.");
            defaultName = fallback;
            _configService.Update(c => c.DefaultLayout = fallback, "DefaultLayout");
        }
        SwitchLayout(defaultName);

        // L3: English text
        if (_configService.Current.SwitchScanEnabled)
            Keyboard.StartScan();

        return Task.CompletedTask;
    }

    // T-7.1: English text → English text
    partial void OnCurrentLayoutNameChanged(string value)
    {
        if (_isSwitching || string.IsNullOrEmpty(value)) return;
        SwitchLayout(value);
    }

    [RelayCommand]
    public void SwitchLayout(string name)
    {
        _isSwitching = true;
        try
        {
            // English text → English text)
            var fileName = _displayToFileName.TryGetValue(name, out var fn) ? fn : name;

            // T-6.7: English text
            var layout = _layoutService.TryLoad(fileName, ex =>
            {
                App.LogError(ex);

                // English text
                var fallbackDisplay = AvailableLayouts.FirstOrDefault(l => l != name);
                if (fallbackDisplay is not null
                    && _displayToFileName.TryGetValue(fallbackDisplay, out var fbFile))
                {
                    var fb = _layoutService.TryLoad(fbFile);
                    if (fb is not null)
                    {
                        Keyboard.LoadLayout(fb);
                        CurrentLayoutName = fb.Name;
                    }
                }

                WpfApp.Current.Dispatcher.BeginInvoke(() =>
                    WpfMsgBox.Show(
                        $"English text '{name}'English text.\n{ex.Message}\n\nEnglish text.",
                        "English text",
                        WpfMsgBoxButton.OK,
                        WpfMsgBoxImage.Warning));
            });

            if (layout is null) return;
            Keyboard.LoadLayout(layout);
            CurrentLayoutName = layout.Name;

            // English text "ko"English text
            _autoCompleteService.ResetState();
        }
        finally
        {
            _isSwitching = false;
        }
    }

    // T-9.5: English text
    [RelayCommand]
    private void DismissUpdate()
    {
        UpdateVersion = null;
        UpdateUrl = null;
        UpdateInstallerUrl = null;
        UpdateStatusMessage = null;
    }

    [RelayCommand]
    private void OpenReleasePage()
    {
        if (!string.IsNullOrEmpty(UpdateUrl))
            Process.Start(new ProcessStartInfo(UpdateUrl) { UseShellExecute = true });
    }

    /// T-9.5: English text
    [RelayCommand]
    private async Task DownloadAndInstallUpdate()
    {
        if (string.IsNullOrEmpty(UpdateInstallerUrl) || string.IsNullOrEmpty(UpdateVersion))
            return;

        try
        {
            // English text
            if (PathResolver.IsPortable)
            {
                WpfMsgBox.Show(
                    "English text.\nEnglish text.",
                    "English text",
                    WpfMsgBoxButton.OK,
                    WpfMsgBoxImage.Information);
                OpenReleasePage();
                return;
            }

            IsDownloading = true;
            DownloadProgress = 0;
            UpdateStatusMessage = $"English text {UpdateVersion} English text...";

            var downloadService = App.Services.GetRequiredService<DownloadService>();
            var tempDir = Path.GetTempPath();
            var installerFileName = $"LArtKey-Setup-{UpdateVersion}.exe";
            var installerPath = Path.Combine(tempDir, installerFileName);

            var progress = new Progress<double>(p => DownloadProgress = p);

            await downloadService.DownloadAsync(
                UpdateInstallerUrl,
                installerPath,
                progress);

            IsDownloading = false;
            UpdateStatusMessage = "English text...";

            // English text
            IsInstalling = true;
            var installerService = App.Services.GetRequiredService<InstallerService>();
            // English text.
            await installerService.RunInstallerAsync(
                installerPath,
                autoRestart: true,
                requestElevation: false);
        }
        catch (Exception ex)
        {
            IsDownloading = false;
            IsInstalling = false;
            UpdateStatusMessage = "English text";

            WpfMsgBox.Show(
                $"English text:\n{ex.Message}\n\nGitHub English text.",
                "English text",
                WpfMsgBoxButton.OK,
                WpfMsgBoxImage.Error);

            OpenReleasePage();
        }
    }

    [RelayCommand]
    private void CancelDownload()
    {
        IsDownloading = false;
        DownloadProgress = 0;
        UpdateStatusMessage = "English text.";
    }

    [RelayCommand]
    private void ToggleEmojiPanel() => Emoji.IsVisible = !Emoji.IsVisible;

    [RelayCommand]
    private void ToggleClipboardPanel() => Clipboard.IsVisible = !Clipboard.IsVisible;

    [RelayCommand]
    private void SendLanguageToggle()
    {
        _inputService.SendKeyPress(VirtualKeyCode.VK_HANGUL);
        _liveRegion.Announce("OS IME English text");
    }

    [RelayCommand]
    private void SendOsk()
    {
        // English text.
        if (!_oskLauncher.TryLaunch())
        {
            // English text.
            _inputService.SendCombo([VirtualKeyCode.VK_LWIN, VirtualKeyCode.VK_LCONTROL, VirtualKeyCode.VK_O]);
        }

        _liveRegion.Announce("English text");
    }

    // ── AI English text ───────────────────────────────────────────

    /// AI English text
    public bool AiEnabled => _configService.Current.AiEnabled;

    /// AI English text
    [ObservableProperty]
    private bool isAiProcessing;

    private CancellationTokenSource? _aiCts;

    /// <summary>
    /// AI English text: Ctrl+C → API → Ctrl+V
    /// </summary>
    /// <summary>
    /// English text.
    /// </summary>
    private void TryFocusLastExternalTarget()
    {
        var fg = Win32.GetForegroundWindow();
        Win32.GetWindowThreadProcessId(fg, out var pid);
        if (pid != (uint)Environment.ProcessId) return;

        var target = _profileService.LastExternalForegroundHwnd;
        if (target == IntPtr.Zero || !Win32.IsWindow(target)) return;

        Win32.SetForegroundWindow(target);
    }

    [RelayCommand]
    private async Task ExecuteAi()
    {
        await ExecuteAiCoreAsync("");
    }

    private async Task ExecuteAiCoreAsync(string prompt)
    {
        if (IsAiProcessing) return;
        IsAiProcessing = true;
        _aiCts = new CancellationTokenSource();
        var ct = _aiCts.Token;
        string? originalClipboard = null;
        try
        {
            // English text → English text
            await Task.Yield();
            TryFocusLastExternalTarget();
            await Task.Delay(80, ct);

            await WpfApp.Current.Dispatcher.InvokeAsync(() =>
                originalClipboard = ClipboardHelper.GetTextWithRetry());

            _inputService.SendCombo([VirtualKeyCode.VK_CONTROL, VirtualKeyCode.VK_C]);
            await Task.Delay(220, ct);

            string? selectedText = null;
            await WpfApp.Current.Dispatcher.InvokeAsync(() =>
                selectedText = ClipboardHelper.GetTextWithRetry());

            if (string.IsNullOrWhiteSpace(selectedText))
            { _liveRegion.Announce("English text"); return; }

            _liveRegion.Announce("AI English text...");
            var result = await _aiService.ProcessTextAsync(selectedText, prompt, ct);

            await WpfApp.Current.Dispatcher.InvokeAsync(() =>
                ClipboardHelper.SetTextWithRetry(result));
            await Task.Delay(50, ct);
            _inputService.SendCombo([VirtualKeyCode.VK_CONTROL, VirtualKeyCode.VK_V]);
            await Task.Delay(100, ct);
            _liveRegion.Announce("AI English text");
        }
        catch (AiServiceException ex) { _liveRegion.Announce($"AI English text: {ex.Message}"); }
        catch (OperationCanceledException) { _liveRegion.Announce("AI English text"); }
        catch (Exception ex) { _liveRegion.Announce("AI English text"); Debug.WriteLine($"[AI] {ex}"); }
        finally
        {
            if (originalClipboard is not null)
            {
                try { await Task.Delay(200);
                    await WpfApp.Current.Dispatcher.InvokeAsync(() =>
                        ClipboardHelper.SetTextWithRetry(originalClipboard));
                } catch { }
            }
            IsAiProcessing = false; _aiCts?.Dispose(); _aiCts = null;
        }
    }

    [RelayCommand] private void CancelAi() => _aiCts?.Cancel();

    private void OnSpecialActionRequested(KeyAction action)
    {
        if (action is AiAction aiAction)
        {
            _ = ExecuteAiCoreAsync(aiAction.Prompt);
        }
    }

    // ── English text ─────────────────────────────────────────

    [ObservableProperty] private ObservableCollection<HeaderButtonVm> headerButtonsLeft = [];
    [ObservableProperty] private ObservableCollection<HeaderButtonVm> headerButtonsRight = [];

    public void RebuildHeaderButtons()
    {
        var configs = _configService.Current.HeaderButtons;
        if (configs.Count == 0)
        {
            configs = HeaderButtonConfig.CreateDefaults();
            _configService.Current.HeaderButtons = configs;
            _configService.Save();
        }
        var left  = new ObservableCollection<HeaderButtonVm>();
        var right = new ObservableCollection<HeaderButtonVm>();
        foreach (var cfg in configs)
        {
            if (!cfg.Visible) continue;
            var vm = CreateHeaderButtonVm(cfg);
            if (vm is null) continue;
            if (cfg.Position == "Left") left.Add(vm); else right.Add(vm);
        }
        HeaderButtonsLeft  = left;
        HeaderButtonsRight = right;
    }

    private HeaderButtonVm? CreateHeaderButtonVm(HeaderButtonConfig config)
    {
        if (config.Kind == HeaderButtonKind.Custom)
        {
            return CreateCustomHeaderButtonVm(config);
        }

        return config.Id switch
        {
            HeaderButtonConfig.IdClipboard => CreateBuiltInHeaderButtonVm(config, ToggleClipboardPanelCommand),
            HeaderButtonConfig.IdEmoji => CreateBuiltInHeaderButtonVm(config, ToggleEmojiPanelCommand),
            HeaderButtonConfig.IdAutoComplete => CreateBuiltInHeaderButtonVm(config, command: null, isToggle: true),
            HeaderButtonConfig.IdOsIme => CreateBuiltInHeaderButtonVm(config, SendLanguageToggleCommand, width: 40, fontSize: 10),
            HeaderButtonConfig.IdOsk => CreateBuiltInHeaderButtonVm(config, SendOskCommand),
            HeaderButtonConfig.IdSettings => CreateBuiltInHeaderButtonVm(config, Settings.OpenSettingsCommand),
            HeaderButtonConfig.IdAi => _configService.Current.AiEnabled
                ? CreateBuiltInHeaderButtonVm(config, ExecuteAiCommand)
                : null,
            _ => null
        };
    }

    private HeaderButtonVm CreateBuiltInHeaderButtonVm(
        HeaderButtonConfig config,
        System.Windows.Input.ICommand? command,
        bool isToggle = false,
        double width = 32,
        double fontSize = 14)
    {
        return new HeaderButtonVm
        {
            Id = config.Id,
            Icon = HeaderButtonConfig.GetBuiltInIconText(config.Id),
            Label = HeaderButtonConfig.GetDisplayName(config.Id),
            ToolTip = HeaderButtonConfig.GetBuiltInTooltip(config.Id),
            AccessibleName = HeaderButtonConfig.GetBuiltInAccessibleName(config.Id),
            AccessibleHelp = BuildHeaderActionHelp(config.CustomAction, config.Id),
            Command = command,
            IsToggle = isToggle,
            Width = width,
            FontSize = fontSize
        };
    }

    private HeaderButtonVm? CreateCustomHeaderButtonVm(HeaderButtonConfig config)
    {
        if (config.CustomAction is null)
            return null;

        return new HeaderButtonVm
        {
            Id = config.Id,
            Icon = string.IsNullOrWhiteSpace(config.IconText) ? "English text" : config.IconText,
            Label = HeaderButtonConfig.GetDisplayName(config.Id),
            ToolTip = string.IsNullOrWhiteSpace(config.Tooltip) ? "English text" : config.Tooltip,
            AccessibleName = string.IsNullOrWhiteSpace(config.AccessibleName) ? config.Tooltip : config.AccessibleName,
            AccessibleHelp = BuildHeaderActionHelp(config.CustomAction, null),
            Command = new RelayCommand(() => ExecuteHeaderButtonAction(config.CustomAction)),
            Width = 32,
            FontSize = 12
        };
    }

    private void ExecuteHeaderButtonAction(KeyAction? action)
    {
        if (action is null)
            return;

        if (action is ToggleInputModeAction)
        {
            _autoCompleteService.ToggleInputMode();
            return;
        }

        _inputService.HandleAction(action);
    }

    private static string BuildHeaderActionHelp(KeyAction? action, string? builtInId)
    {
        if (builtInId is not null)
        {
            return builtInId switch
            {
                HeaderButtonConfig.IdClipboard => "English text.",
                HeaderButtonConfig.IdEmoji => "English text.",
                HeaderButtonConfig.IdAutoComplete => "English text.",
                HeaderButtonConfig.IdOsIme => "OS IMEEnglish text.",
                HeaderButtonConfig.IdOsk => "English text.",
                HeaderButtonConfig.IdSettings => "English text.",
                HeaderButtonConfig.IdAi => "English text.",
                _ => ""
            };
        }

        return action switch
        {
            SendKeyAction sendKey => $"{sendKey.Vk} English text.",
            SendComboAction sendCombo => $"{string.Join(" + ", sendCombo.Keys)} English text.",
            ToggleStickyAction sticky => $"{sticky.Vk} English text.",
            SwitchLayoutAction switchLayout => $"{switchLayout.Name} English text.",
            RunAppAction runApp => $"{runApp.Path} English text.",
            BoilerplateAction => "English text.",
            ShellCommandAction => "English text.",
            VolumeControlAction volume => $"English text {volume.Direction} English text.",
            ClipboardPasteAction => "English text.",
            ToggleInputModeAction => "LArtKey English text.",
            ToggleFunctionLayerAction => "Fn English text.",
            AiAction => "English text.",
            _ => ""
        };
    }

    // T-5.4: English text
    private void OnForegroundAppChanged(string processName)
    {
        WpfApp.Current.Dispatcher.Invoke(() =>
        {
            var config = _configService.Current;
            if (!config.AutoProfileSwitch) return;
            if (config.Profiles.TryGetValue(processName, out var layoutName))
            {
                try { SwitchLayout(layoutName); }
                catch { }
            }
        });
    }
}

/// English text VM
public class HeaderButtonVm : ObservableObject
{
    public string Id { get; init; } = "";
    public string Icon { get; init; } = "";
    public string Label { get; init; } = "";
    public string ToolTip { get; init; } = "";
    public string AccessibleName { get; init; } = "";
    public string AccessibleHelp { get; init; } = "";
    public System.Windows.Input.ICommand? Command { get; init; }
    public bool IsToggle { get; init; }
    public double Width { get; init; } = 32;
    public double FontSize { get; init; } = 14;
}
