using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;
using LArtKey.Models;
using LArtKey.Services;
using WpfApp = System.Windows.Application;
using WpfMsgBox = System.Windows.MessageBox;
using WpfMsgBoxButton = System.Windows.MessageBoxButton;
using WpfMsgBoxImage = System.Windows.MessageBoxImage;
using WpfMsgBoxResult = System.Windows.MessageBoxResult;
using WpfDialog = Microsoft.Win32;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace LArtKey.ViewModels;

/// <summary>
/// [English text] English text.
/// [English text] English text.
/// </summary>
public partial class SettingsViewModel : ObservableObject
{
    private readonly ConfigService        _configService;
    private readonly ThemeService         _themeService;
    private readonly LayoutService        _layoutService;
    private readonly HotkeyService        _hotkeyService;
    private readonly StartupService       _startupService;
    private readonly SoundService         _soundService;
    private readonly UpdateService        _updateService;
    private readonly DownloadService      _downloadService;
    private readonly InstallerService     _installerService;
    private readonly AiService            _aiService;

    private CancellationTokenSource?      _downloadCts;

    private LArtKey.Views.SettingsWindow? _settingsWindow;
    private LArtKey.Views.SwitchScanSettingsWindow? _switchScanSettingsWindow;
    private LArtKey.Views.FocusA11ySettingsWindow? _focusA11ySettingsWindow;

    // ── English text) ───────────────────────
    // English text.

    [ObservableProperty] private string themeMode      = "system";
    [ObservableProperty] private bool   alwaysOnTop    = true;
    [ObservableProperty] private bool   activeOpacityEnabled;
    [ObservableProperty] private double opacityActive  = 1.0;
    [ObservableProperty] private bool   idleOpacityEnabled;
    [ObservableProperty] private double opacityIdle    = 1.0;
    [ObservableProperty] private int    fadeDelaySec   = 5;
    [ObservableProperty] private bool   dwellEnabled   = false;
    [ObservableProperty] private int    dwellTimeMs    = 800;
    [ObservableProperty] private string selectedLayout = "";
    [ObservableProperty] private string globalHotkey   = "Ctrl+Alt+K";
    [ObservableProperty] private int    windowScale    = 100;
    [ObservableProperty] private bool   skipCloseConfirm = false;

    // T-8.1: English text
    [ObservableProperty] private bool runOnStartup;

    // T-8.2: English text
    [ObservableProperty] private bool soundEnabled;
    [ObservableProperty] private string soundFilePath = "";



    // T-10: English text
    [ObservableProperty]
    private bool keyRepeatEnabled;
    [ObservableProperty]
    private int keyRepeatDelayMs;
    [ObservableProperty]
    private int keyRepeatIntervalMs;

    // L1: English text
    [ObservableProperty]
    private int keyFontScalePercent = 100;

    // L1: English text
    [ObservableProperty]
    private bool keyboardA11yNavigationEnabled;
    [ObservableProperty]
    private KeyboardA11yNavigationScope keyboardA11yNavigationScope = KeyboardA11yNavigationScope.KeysOnly;
    [ObservableProperty]
    private bool keyboardA11yAnnounceFocus;
    [ObservableProperty]
    private string keyboardA11yExitKey = "VK_ESCAPE";

    // L2: English text
    [ObservableProperty]
    private bool ttsEnabled;
    [ObservableProperty]
    private bool ttsOnHover;
    [ObservableProperty]
    private int ttsRate;

    // L2: English text
    [ObservableProperty]
    private bool reducedMotionEnabled;

    // L3: English text
    [ObservableProperty]
    private bool switchScanEnabled;
    [ObservableProperty]
    private int switchScanIntervalMs;
    [ObservableProperty]
    private bool switchScanTwoSwitch;
    [ObservableProperty]
    private SwitchScanMode switchScanMode = SwitchScanMode.Linear;
    [ObservableProperty]
    private int switchScanInitialDelayMs;
    [ObservableProperty]
    private int switchScanSelectPauseMs;
    [ObservableProperty]
    private int switchScanCyclesBeforePause;
    [ObservableProperty]
    private bool switchScanWrapEnabled;
    [ObservableProperty]
    private string switchScanNextKey = "VK_TAB";
    [ObservableProperty]
    private string switchScanSelectKey = "VK_RETURN";
    [ObservableProperty]
    private string switchScanSecondarySelectKey = "VK_SPACE";
    [ObservableProperty]
    private string switchScanPreviousKey = "";
    [ObservableProperty]
    private string switchScanPauseKey = "";
    [ObservableProperty]
    private bool switchScanIncludeSuggestions;
    [ObservableProperty]
    private SwitchScanSuggestionPriority switchScanSuggestionPriority = SwitchScanSuggestionPriority.BeforeKeyboard;
    public Array SwitchScanModes => Enum.GetValues(typeof(SwitchScanMode));
    public Array SwitchScanSuggestionPriorities => Enum.GetValues(typeof(SwitchScanSuggestionPriority));
    public Array KeyboardA11yNavigationScopes => Enum.GetValues(typeof(KeyboardA11yNavigationScope));
    public Array SwitchScanAnnounceModes => Enum.GetValues(typeof(SwitchScanAnnounceMode));
    [ObservableProperty]
    private SwitchScanAnnounceMode switchScanAnnounceMode = SwitchScanAnnounceMode.SelectionOnly;

    // T-9.5: English text
    [ObservableProperty] private string currentVersion = "";

    // ── AI English text ──────────────────────────────────────────────
    [ObservableProperty] private bool aiEnabled;
    [ObservableProperty] private string aiEndpoint = "";
    [ObservableProperty] private string aiApiKey = "";
    [ObservableProperty] private string aiModel = "";
    [ObservableProperty] private string aiDefaultPrompt = "";
    [ObservableProperty] private int aiTimeoutSeconds;

    // ── English text ─────────────────────────────────────────────────
    [ObservableProperty]
    private ObservableCollection<HeaderButtonConfig> headerButtons = [];

    public IEnumerable<HeaderButtonConfig> BuiltInHeaderButtons =>
        HeaderButtons.Where(button => button.Kind == HeaderButtonKind.BuiltIn);

    public IEnumerable<HeaderButtonConfig> CustomHeaderButtons =>
        HeaderButtons.Where(button => button.Kind == HeaderButtonKind.Custom);

    /// <summary>
    /// English text.
    /// </summary>
    public int CustomHeaderButtonCount =>
        HeaderButtons.Count(button => button.Kind == HeaderButtonKind.Custom);

    /// <summary>
    /// English text.
    /// </summary>
    public bool CanAddMoreCustomHeaderButtons =>
        CustomHeaderButtonCount < HeaderButtonConfig.MaxCustomButtonCount;

    /// <summary>
    /// English text.
    /// </summary>
    public string HeaderButtonLimitSummary =>
        $"English text {HeaderButtonConfig.MaxCustomButtonCount}English text {HeaderButtonConfig.MaxVisibleButtonsLeft}English text.";

    // T-5.12: English text
    public bool IsRunningAsAdmin { get; } = System.Security.Principal.WindowsIdentity.GetCurrent() is { } identity
        && new System.Security.Principal.WindowsPrincipal(identity).IsInRole(System.Security.Principal.WindowsBuiltInRole.Administrator);

    // T-9.5: English text
    [ObservableProperty] private bool isCheckingUpdate;
    [ObservableProperty] private bool isDownloading;
    [ObservableProperty] private double downloadProgress;
    [ObservableProperty] private bool isInstalling;
    [ObservableProperty] private string updateStatusMessage = "";
    [ObservableProperty] private bool hasUpdateAvailable;
    [ObservableProperty] private string latestVersion = "";
    [ObservableProperty] private string updateInstallerUrl = "";
    [ObservableProperty] private string updateReleaseUrl = "";

    // T-8.5: English text
    [ObservableProperty]
    private ObservableCollection<ProfileEntry> profiles = [];

    [ObservableProperty]
    private ObservableCollection<string> availableLayouts = [];

     // English text (Theme)
     public bool ThemeIsSystem { get => ThemeMode == "system"; set { if (value) ThemeMode = "system"; } }
     public bool ThemeIsLight  { get => ThemeMode == "Light";  set { if (value) ThemeMode = "Light";  } }
     public bool ThemeIsDark   { get => ThemeMode == "Dark";   set { if (value) ThemeMode = "Dark";   } }
     public bool ThemeIsHighContrast { get => ThemeMode == "HighContrast"; set { if (value) ThemeMode = "HighContrast"; } }
     public double IdleOpacityMaximum => ActiveOpacityEnabled ? Math.Clamp(OpacityActive, 0.1, 1.0) : 1.0;
     public bool CanUseIdleOpacity => ActiveOpacityEnabled;
     public bool CanEditIdleOpacity => ActiveOpacityEnabled && IdleOpacityEnabled;

    // ── English text ──────────────────────────────────────────────────────────────

    public SettingsViewModel(
        ConfigService        configService,
        ThemeService         themeService,
        LayoutService        layoutService,
        HotkeyService        hotkeyService,
        StartupService       startupService,
        SoundService         soundService,
        UpdateService        updateService,
        DownloadService      downloadService,
        InstallerService     installerService,
        AiService            aiService)
    {
        _configService  = configService;
        _themeService   = themeService;
        _layoutService  = layoutService;
        _hotkeyService  = hotkeyService;
        _startupService = startupService;
        _soundService   = soundService;
        _updateService  = updateService;
        _downloadService = downloadService;
        _installerService = installerService;
        _aiService      = aiService;
        _configService.ConfigChanged += OnConfigChanged;

        // T-9.5: English text
        var asmVersion = Assembly.GetExecutingAssembly().GetName().Version;
        CurrentVersion = asmVersion?.ToString(3) ?? "0.1.0";

        _layoutService.LayoutsChanged += OnLayoutsChanged;
        LoadFromConfig();
    }

    private void OnConfigChanged(string? propertyName)
    {
        if (propertyName is not nameof(AppConfig.AiDefaultPrompt)
            and not nameof(AppConfig.AskBeforeHideToTray)
            and not nameof(AppConfig.Profiles)
            and not nameof(AppConfig.HeaderButtons))
        {
            return;
        }

        // English text.
        LoadFromConfig();
    }

    private void OnLayoutsChanged()
    {
        AvailableLayouts = new ObservableCollection<string>(
            _layoutService.GetAvailableLayouts());
    }

    /// <summary>
    /// English text.
    /// </summary>
    private void ClampIdleOpacityIfNeeded(bool saveIfChanged)
    {
        var maxIdleOpacity = IdleOpacityMaximum;
        if (OpacityIdle <= maxIdleOpacity + 0.0001)
        {
            return;
        }

        OpacityIdle = maxIdleOpacity;

        if (saveIfChanged && !_isLoading)
        {
            _configService.Update(c => c.OpacityIdle = maxIdleOpacity, nameof(AppConfig.OpacityIdle));
        }
    }

    private bool _isLoading;

    private void LoadFromConfig()
    {
        _isLoading = true;
        try
        {
            var c = _configService.Current;
            ThemeMode      = c.Theme;
            AlwaysOnTop    = c.AlwaysOnTop;
            ActiveOpacityEnabled = c.ActiveOpacityEnabled;
            OpacityActive  = c.OpacityActive;
            IdleOpacityEnabled = c.IdleOpacityEnabled;
            OpacityIdle    = c.OpacityIdle;
            FadeDelaySec   = c.FadeDelayMs / 1000;
            DwellEnabled   = c.DwellEnabled;
            DwellTimeMs    = c.DwellTimeMs;
            SelectedLayout = c.DefaultLayout;
            GlobalHotkey   = c.GlobalHotkey;
            WindowScale    = c.Window.Scale;
            SkipCloseConfirm = !c.AskBeforeHideToTray;

            // T-8.1: English text
            RunOnStartup = _startupService.IsEnabled;

            // T-8.2: English text
            SoundEnabled = c.SoundEnabled;
            SoundFilePath = c.SoundFilePath ?? "";



             // T-10: English text
             KeyRepeatEnabled = c.KeyRepeatEnabled;
             KeyRepeatDelayMs = c.KeyRepeatDelayMs;
             KeyRepeatIntervalMs = c.KeyRepeatIntervalMs;

             // L1: English text
             KeyFontScalePercent = c.KeyFontScalePercent;

             // L1: English text
             KeyboardA11yNavigationEnabled = c.KeyboardA11yNavigationEnabled;
             KeyboardA11yNavigationScope = c.KeyboardA11yNavigationScope;
             KeyboardA11yAnnounceFocus = c.KeyboardA11yAnnounceFocus;
             KeyboardA11yExitKey = c.KeyboardA11yExitKey;

             // L2/L3 English text
             TtsEnabled = c.TtsEnabled;
             TtsOnHover = c.TtsOnHover;
             TtsRate = c.TtsRate;
             ReducedMotionEnabled = c.ReducedMotionEnabled;
             SwitchScanEnabled = c.SwitchScanEnabled;
             SwitchScanIntervalMs = c.SwitchScanIntervalMs;
             SwitchScanTwoSwitch = c.SwitchScanTwoSwitch;
             SwitchScanMode = c.SwitchScanMode;
             SwitchScanInitialDelayMs = c.SwitchScanInitialDelayMs;
             SwitchScanSelectPauseMs = c.SwitchScanSelectPauseMs;
             SwitchScanCyclesBeforePause = c.SwitchScanCyclesBeforePause;
             SwitchScanWrapEnabled = c.SwitchScanWrapEnabled;
             SwitchScanNextKey = c.SwitchScanNextKey;
             SwitchScanSelectKey = c.SwitchScanSelectKey;
             SwitchScanSecondarySelectKey = c.SwitchScanSecondarySelectKey;
             SwitchScanPreviousKey = c.SwitchScanPreviousKey;
             SwitchScanPauseKey = c.SwitchScanPauseKey;
             SwitchScanIncludeSuggestions = c.SwitchScanIncludeSuggestions;
             SwitchScanSuggestionPriority = c.SwitchScanSuggestionPriority;
             SwitchScanAnnounceMode = c.SwitchScanAnnounceMode;

             // AI English text
             AiEnabled = c.AiEnabled;
             AiEndpoint = c.AiEndpoint;
             AiApiKey = SecureStorage.Decrypt(c.AiApiKeyEncrypted);
             AiModel = c.AiModel;
             AiDefaultPrompt = c.AiDefaultPrompt;
             AiTimeoutSeconds = c.AiTimeoutSeconds <= 0 ? 30 : c.AiTimeoutSeconds;

             // English text
             HeaderButtons = new ObservableCollection<HeaderButtonConfig>(
                 c.HeaderButtons.Select(CloneHeaderButtonConfig));
             RaiseHeaderButtonCollectionChanged();

            // T-8.5: English text
            Profiles = new ObservableCollection<ProfileEntry>(
                c.Profiles.Select(p => new ProfileEntry(p.Key, p.Value)));
            foreach (var p in Profiles) SubscribeToProfileChanges(p);

            AvailableLayouts = new ObservableCollection<string>(
                _layoutService.GetAvailableLayouts());

            ClampIdleOpacityIfNeeded(saveIfChanged: false);
        }
        finally
        {
            _isLoading = false;
        }

        // T-8.2: _isLoading English text _isLoading=true English text)
        _soundService.Configure(SoundEnabled, string.IsNullOrEmpty(SoundFilePath) ? null : SoundFilePath);
    }

    // ── English text ───────────────────────────────────────────────

    partial void OnThemeModeChanged(string value)
    {
        if (_isLoading) return;
        _themeService.Apply(value);
        _configService.Update(c => c.Theme = value);
        OnPropertyChanged(nameof(ThemeIsSystem));
        OnPropertyChanged(nameof(ThemeIsLight));
        OnPropertyChanged(nameof(ThemeIsDark));
    }

    partial void OnAlwaysOnTopChanged(bool value)
    {
        if (_isLoading) return;
        _configService.Update(c => c.AlwaysOnTop = value);
        if (WpfApp.Current.MainWindow is not null)
            WpfApp.Current.MainWindow.Topmost = value;
    }

    partial void OnActiveOpacityEnabledChanged(bool value)
    {
        if (_isLoading) return;
        OnPropertyChanged(nameof(IdleOpacityMaximum));
        OnPropertyChanged(nameof(CanUseIdleOpacity));
        OnPropertyChanged(nameof(CanEditIdleOpacity));

        if (!value && IdleOpacityEnabled)
        {
            IdleOpacityEnabled = false;
        }

        ClampIdleOpacityIfNeeded(saveIfChanged: false);
        _configService.Update(c => c.ActiveOpacityEnabled = value, nameof(AppConfig.ActiveOpacityEnabled));
    }

    partial void OnOpacityActiveChanged(double value)
    {
        if (_isLoading) return;
        OnPropertyChanged(nameof(IdleOpacityMaximum));
        OnPropertyChanged(nameof(CanUseIdleOpacity));
        OnPropertyChanged(nameof(CanEditIdleOpacity));
        ClampIdleOpacityIfNeeded(saveIfChanged: true);
        _configService.Update(c => c.OpacityActive = value, nameof(AppConfig.OpacityActive));
    }

    partial void OnIdleOpacityEnabledChanged(bool value)
    {
        if (_isLoading) return;
        OnPropertyChanged(nameof(CanEditIdleOpacity));
        _configService.Update(c => c.IdleOpacityEnabled = value, nameof(AppConfig.IdleOpacityEnabled));
    }

    partial void OnOpacityIdleChanged(double value)
    {
        if (_isLoading) return;
        var clamped = Math.Min(value, IdleOpacityMaximum);
        if (Math.Abs(clamped - value) > 0.0001)
        {
            OpacityIdle = clamped;
            return;
        }

        _configService.Update(c => c.OpacityIdle = clamped, nameof(AppConfig.OpacityIdle));
    }

    partial void OnFadeDelaySecChanged(int value)
    {
        if (_isLoading) return;
        _configService.Update(c => c.FadeDelayMs = value * 1000, nameof(AppConfig.FadeDelayMs));
    }

    partial void OnDwellEnabledChanged(bool value)
    {
        if (_isLoading) return;
        _configService.Update(c => c.DwellEnabled = value);
    }

    partial void OnDwellTimeMsChanged(int value)
    {
        if (_isLoading) return;
        _configService.Update(c => c.DwellTimeMs = value);
    }

    partial void OnSkipCloseConfirmChanged(bool value)
    {
        if (_isLoading) return;
        _configService.Update(c => c.AskBeforeHideToTray = !value, nameof(AppConfig.AskBeforeHideToTray));
    }

    partial void OnSelectedLayoutChanged(string value)
    {
        if (_isLoading) return;
        _configService.Update(c => c.DefaultLayout = value, "DefaultLayout");
    }

    partial void OnGlobalHotkeyChanged(string value)
    {
        if (_isLoading) return;
        _configService.Update(c => c.GlobalHotkey = value);
        if (WpfApp.Current.MainWindow is MainWindow mw)
        {
            var hwnd = new System.Windows.Interop.WindowInteropHelper(mw).Handle;
            if (hwnd != IntPtr.Zero)
                _hotkeyService.Reregister(hwnd, value);
        }
    }

    partial void OnWindowScaleChanged(int value)
    {
        if (_isLoading) return;
        var clamped = Math.Clamp(value, 60, 200);
        if (clamped != value) { WindowScale = clamped; return; }
        _configService.Update(c => c.Window.Scale = clamped, "Window.Scale");
    }

    // ── T-8.1: English text ────────────────────────────────────────────────────

    partial void OnRunOnStartupChanged(bool value)
    {
        if (_isLoading) return;
        if (value) _startupService.Enable();
        else        _startupService.Disable();
        _configService.Update(c => c.RunOnStartup = value);
    }

    // ── T-8.2: English text ──────────────────────────────────────────────

    partial void OnSoundEnabledChanged(bool value)
    {
        if (_isLoading) return;
        _soundService.Configure(value, string.IsNullOrEmpty(SoundFilePath) ? null : SoundFilePath);
        _configService.Update(c => c.SoundEnabled = value);
    }

    partial void OnSoundFilePathChanged(string value)
    {
        if (_isLoading) return;
        _soundService.Configure(SoundEnabled, string.IsNullOrEmpty(value) ? null : value);
        _configService.Update(c => c.SoundFilePath = string.IsNullOrEmpty(value) ? null : value);
    }

    /// <summary>
    /// English text.
    /// </summary>
    [RelayCommand]
    private void BrowseSoundFile()
    {
        var dlg = new WpfDialog.OpenFileDialog { Filter = "WAV English text|*.wav|English text|*.*" };
        if (dlg.ShowDialog() == true)
        {
            SoundFilePath = dlg.FileName;
            _soundService.Configure(SoundEnabled, SoundFilePath);
            _configService.Update(c => c.SoundFilePath = SoundFilePath);
        }
    }

    [RelayCommand]
    private void ResetSoundFile()
    {
        SoundFilePath = "";
        _soundService.Configure(SoundEnabled, null);
        _configService.Update(c => c.SoundFilePath = null);
    }

    // T-10: English text
    partial void OnKeyRepeatEnabledChanged(bool value)
    {
        if (_isLoading) return;
        _configService.Update(c => c.KeyRepeatEnabled = value);
    }

    partial void OnKeyRepeatDelayMsChanged(int value)
    {
        if (_isLoading) return;
        _configService.Update(c => c.KeyRepeatDelayMs = value);
    }

     partial void OnKeyRepeatIntervalMsChanged(int value)
     {
         if (_isLoading) return;
         _configService.Update(c => c.KeyRepeatIntervalMs = value);
     }

     // L1: English text
     partial void OnKeyFontScalePercentChanged(int value)
     {
         if (_isLoading) return;
         var clamped = Math.Clamp(value, 80, 220);
         if (clamped != value) { KeyFontScalePercent = clamped; return; }
         _configService.Update(c => c.KeyFontScalePercent = clamped, "KeyFontScalePercent");
     }

     // L1: English text
     partial void OnKeyboardA11yNavigationEnabledChanged(bool value)
     {
         if (_isLoading) return;
         _configService.Update(c => c.KeyboardA11yNavigationEnabled = value, "KeyboardA11yNavigationEnabled");
     }

     partial void OnKeyboardA11yNavigationScopeChanged(KeyboardA11yNavigationScope value)
     {
         if (_isLoading) return;
         _configService.Update(c => c.KeyboardA11yNavigationScope = value, "KeyboardA11yNavigationScope");
     }

     partial void OnKeyboardA11yAnnounceFocusChanged(bool value)
     {
         if (_isLoading) return;
         _configService.Update(c => c.KeyboardA11yAnnounceFocus = value, "KeyboardA11yAnnounceFocus");
     }

     partial void OnKeyboardA11yExitKeyChanged(string value)
     {
         if (_isLoading) return;
         string normalized = string.IsNullOrWhiteSpace(value) ? "VK_ESCAPE" : value.Trim().ToUpperInvariant();
         if (normalized != value) { KeyboardA11yExitKey = normalized; return; }
         _configService.Update(c => c.KeyboardA11yExitKey = normalized, "KeyboardA11yExitKey");
     }

     // L2: English text
     partial void OnTtsEnabledChanged(bool value)
     {
         if (_isLoading) return;
         _configService.Update(c => c.TtsEnabled = value);
     }

     partial void OnTtsOnHoverChanged(bool value)
     {
         if (_isLoading) return;
         _configService.Update(c => c.TtsOnHover = value);
     }

     partial void OnTtsRateChanged(int value)
     {
         if (_isLoading) return;
         var clamped = Math.Clamp(value, -5, 5);
         if (clamped != value) { TtsRate = clamped; return; }
         _configService.Update(c => c.TtsRate = clamped);
     }

     // L2: English text
     partial void OnReducedMotionEnabledChanged(bool value)
     {
         if (_isLoading) return;
         _configService.Update(c => c.ReducedMotionEnabled = value);
     }

     // L3: English text
     partial void OnSwitchScanEnabledChanged(bool value)
     {
         if (_isLoading) return;
         _configService.Update(c => c.SwitchScanEnabled = value);
     }

     partial void OnSwitchScanIntervalMsChanged(int value)
     {
         if (_isLoading) return;
         var clamped = Math.Clamp(value, 200, 3000);
         if (clamped != value) { SwitchScanIntervalMs = clamped; return; }
         _configService.Update(c => c.SwitchScanIntervalMs = clamped);
     }

     partial void OnSwitchScanTwoSwitchChanged(bool value)
     {
         if (_isLoading) return;
         _configService.Update(c => c.SwitchScanTwoSwitch = value);
     }

     partial void OnSwitchScanModeChanged(SwitchScanMode value)
     {
         if (_isLoading) return;
         _configService.Update(c => c.SwitchScanMode = value);
     }

     partial void OnSwitchScanInitialDelayMsChanged(int value)
     {
         if (_isLoading) return;
         var clamped = Math.Clamp(value, 0, 5000);
         if (clamped != value) { SwitchScanInitialDelayMs = clamped; return; }
         _configService.Update(c => c.SwitchScanInitialDelayMs = clamped);
     }

     partial void OnSwitchScanSelectPauseMsChanged(int value)
     {
         if (_isLoading) return;
         var clamped = Math.Clamp(value, 0, 5000);
         if (clamped != value) { SwitchScanSelectPauseMs = clamped; return; }
         _configService.Update(c => c.SwitchScanSelectPauseMs = clamped);
     }

     partial void OnSwitchScanCyclesBeforePauseChanged(int value)
     {
         if (_isLoading) return;
         var clamped = Math.Clamp(value, 0, 20);
         if (clamped != value) { SwitchScanCyclesBeforePause = clamped; return; }
         _configService.Update(c => c.SwitchScanCyclesBeforePause = clamped);
     }

     partial void OnSwitchScanWrapEnabledChanged(bool value)
     {
         if (_isLoading) return;
         _configService.Update(c => c.SwitchScanWrapEnabled = value);
     }

     partial void OnSwitchScanNextKeyChanged(string value)
     {
         if (_isLoading) return;
         _configService.Update(c => c.SwitchScanNextKey = value?.Trim() ?? "");
     }

     partial void OnSwitchScanSelectKeyChanged(string value)
     {
         if (_isLoading) return;
         _configService.Update(c => c.SwitchScanSelectKey = value?.Trim() ?? "");
     }

     partial void OnSwitchScanSecondarySelectKeyChanged(string value)
     {
         if (_isLoading) return;
         _configService.Update(c => c.SwitchScanSecondarySelectKey = value?.Trim() ?? "");
     }

     partial void OnSwitchScanPreviousKeyChanged(string value)
     {
         if (_isLoading) return;
         _configService.Update(c => c.SwitchScanPreviousKey = value?.Trim() ?? "");
     }

     partial void OnSwitchScanPauseKeyChanged(string value)
     {
         if (_isLoading) return;
         _configService.Update(c => c.SwitchScanPauseKey = value?.Trim() ?? "");
     }

     partial void OnSwitchScanIncludeSuggestionsChanged(bool value)
     {
         if (_isLoading) return;
         _configService.Update(c => c.SwitchScanIncludeSuggestions = value);
     }

     partial void OnSwitchScanSuggestionPriorityChanged(SwitchScanSuggestionPriority value)
     {
         if (_isLoading) return;
         _configService.Update(c => c.SwitchScanSuggestionPriority = value);
     }

     partial void OnSwitchScanAnnounceModeChanged(SwitchScanAnnounceMode value)
     {
         if (_isLoading) return;
         _configService.Update(c => c.SwitchScanAnnounceMode = value);
     }

    // ── T-8.5: English text ────────────────────────────────────────

    [RelayCommand]
    private void AddProfile()
    {
        var entry = new ProfileEntry("", "");
        SubscribeToProfileChanges(entry);
        Profiles.Add(entry);
        // English text)
        SaveProfiles();
    }

    [RelayCommand]
    private void RemoveProfile(ProfileEntry entry)
    {
        UnsubscribeFromProfileChanges(entry);
        Profiles.Remove(entry);
        SaveProfiles();
    }

    private void SubscribeToProfileChanges(ProfileEntry entry)
    {
        entry.PropertyChanged += OnProfileEntryPropertyChanged;
    }

    private void UnsubscribeFromProfileChanges(ProfileEntry entry)
    {
        entry.PropertyChanged -= OnProfileEntryPropertyChanged;
    }

    private void OnProfileEntryPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (_isLoading) return;
        SaveProfiles();
    }

    [RelayCommand]
    private void SaveProfiles()
    {
        _configService.Update(c =>
            c.Profiles = Profiles
                .Where(p => !string.IsNullOrWhiteSpace(p.ProcessName))
                .ToDictionary(p => p.ProcessName.ToLower(), p => p.LayoutName));
    }

    // ── AI English text ────────────────────────────────
    partial void OnAiEnabledChanged(bool value)
    {
        if (_isLoading) return;
        // English text ✨ English text.
        _configService.Update(c => c.AiEnabled = value, nameof(AppConfig.AiEnabled));
    }
    partial void OnAiEndpointChanged(string value) { if (_isLoading) return; _configService.Update(c => c.AiEndpoint = value); }
    partial void OnAiApiKeyChanged(string value) { if (_isLoading) return; _configService.Update(c => c.AiApiKeyEncrypted = SecureStorage.Encrypt(value)); }
    partial void OnAiModelChanged(string value) { if (_isLoading) return; _configService.Update(c => c.AiModel = value); }
    partial void OnAiDefaultPromptChanged(string value) { if (_isLoading) return; _configService.Update(c => c.AiDefaultPrompt = value); }
    partial void OnAiTimeoutSecondsChanged(int value)
    {
        if (_isLoading) return;
        var clamped = Math.Clamp(value, 5, 300);
        if (clamped != value) { AiTimeoutSeconds = clamped; return; }
        _configService.Update(c => c.AiTimeoutSeconds = clamped);
    }

    /// <summary>English text.</summary>
    [RelayCommand]
    private async Task TestAiConnection()
    {
        try
        {
            var msg = await _aiService.TestConnectionAsync();
            WpfMsgBox.Show(msg, "AI English text", WpfMsgBoxButton.OK, WpfMsgBoxImage.Information);
        }
        catch (AiServiceException ex)
        {
            WpfMsgBox.Show(ex.Message, "AI English text", WpfMsgBoxButton.OK, WpfMsgBoxImage.Warning);
        }
        catch (Exception ex)
        {
            WpfMsgBox.Show($"English text: {ex.Message}", "AI English text", WpfMsgBoxButton.OK, WpfMsgBoxImage.Error);
        }
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

    private static string GetHeaderButtonDisplayName(HeaderButtonConfig item)
    {
        if (item.Kind == HeaderButtonKind.BuiltIn)
        {
            return HeaderButtonConfig.GetDisplayName(item.Id);
        }

        return string.IsNullOrWhiteSpace(item.Tooltip) ? "English text" : item.Tooltip;
    }

    public string DescribeHeaderButton(HeaderButtonConfig item) => GetHeaderButtonDisplayName(item);

    public string DescribeHeaderButtonAction(HeaderButtonConfig item)
    {
        if (item.Kind == HeaderButtonKind.BuiltIn)
        {
            return HeaderButtonConfig.GetBuiltInTooltip(item.Id);
        }

        return item.CustomAction switch
        {
            SendKeyAction sendKey => sendKey.Vk,
            SendComboAction sendCombo => string.Join(" + ", sendCombo.Keys),
            ToggleStickyAction sticky => $"{sticky.Vk} English text",
            SwitchLayoutAction switchLayout => $"{switchLayout.Name} English text",
            RunAppAction runApp => runApp.Path,
            BoilerplateAction => "English text",
            ShellCommandAction => "English text",
            VolumeControlAction volume => $"English text {volume.Direction}",
            ClipboardPasteAction => "English text",
            ToggleInputModeAction => "English text",
            ToggleFunctionLayerAction => "Fn English text",
            AiAction => "AI English text",
            _ => "English text"
        };
    }

    private void RaiseHeaderButtonCollectionChanged()
    {
        OnPropertyChanged(nameof(BuiltInHeaderButtons));
        OnPropertyChanged(nameof(CustomHeaderButtons));
        OnPropertyChanged(nameof(CustomHeaderButtonCount));
        OnPropertyChanged(nameof(CanAddMoreCustomHeaderButtons));
        OnPropertyChanged(nameof(HeaderButtonLimitSummary));
    }

    /// <summary>
    /// English text.
    /// </summary>
    private bool TryValidateHeaderButtonLayout()
    {
        var leftVisibleCount = HeaderButtonConfig.CountVisibleButtons(HeaderButtons, "Left");
        if (leftVisibleCount > HeaderButtonConfig.MaxVisibleButtonsLeft)
        {
            ShowHeaderButtonSideLimitMessage("English text", HeaderButtonConfig.MaxVisibleButtonsLeft);
            RestoreHeaderButtonsFromConfig();
            return false;
        }

        var rightVisibleCount = HeaderButtonConfig.CountVisibleButtons(HeaderButtons, "Right");
        if (rightVisibleCount > HeaderButtonConfig.MaxVisibleButtonsRight)
        {
            ShowHeaderButtonSideLimitMessage("English text", HeaderButtonConfig.MaxVisibleButtonsRight);
            RestoreHeaderButtonsFromConfig();
            return false;
        }

        return true;
    }

    /// <summary>
    /// English text.
    /// </summary>
    private void RestoreHeaderButtonsFromConfig()
    {
        HeaderButtons = new ObservableCollection<HeaderButtonConfig>(
            _configService.Current.HeaderButtons.Select(CloneHeaderButtonConfig));
        RaiseHeaderButtonCollectionChanged();
    }

    /// <summary>
    /// English text.
    /// </summary>
    private static void ShowHeaderButtonSideLimitMessage(string sideName, int maxCount)
    {
        WpfMsgBox.Show(
            $"English text {sideName}English text {maxCount}English text.\nEnglish text.",
            "English text",
            WpfMsgBoxButton.OK,
            WpfMsgBoxImage.Information);
    }

    /// <summary>
    /// English text.
    /// </summary>
    private bool EnsureCanAddCustomHeaderButton()
    {
        if (CanAddMoreCustomHeaderButtons)
            return true;

        WpfMsgBox.Show(
            $"English text {HeaderButtonConfig.MaxCustomButtonCount}English text.",
            "English text",
            WpfMsgBoxButton.OK,
            WpfMsgBoxImage.Information);
        return false;
    }

    [RelayCommand]
    private void MoveHeaderButtonUp(HeaderButtonConfig item)
    {
        var idx = HeaderButtons.IndexOf(item);
        if (idx > 0)
        {
            HeaderButtons.Move(idx, idx - 1);
            RaiseHeaderButtonCollectionChanged();
            SaveHeaderButtons();
        }
    }

    [RelayCommand]
    private void MoveHeaderButtonDown(HeaderButtonConfig item)
    {
        var idx = HeaderButtons.IndexOf(item);
        if (idx >= 0 && idx < HeaderButtons.Count - 1)
        {
            HeaderButtons.Move(idx, idx + 1);
            RaiseHeaderButtonCollectionChanged();
            SaveHeaderButtons();
        }
    }
    [RelayCommand]
    private void ToggleHeaderButtonPosition(HeaderButtonConfig item)
    {
        if (item is null) return;
        item.Position = string.Equals(item.Position, "Left", StringComparison.OrdinalIgnoreCase) ? "Right" : "Left";

        // HeaderButtonConfigEnglish text.
        HeaderButtons = new ObservableCollection<HeaderButtonConfig>(
            HeaderButtons.Select(CloneHeaderButtonConfig));
        RaiseHeaderButtonCollectionChanged();

        SaveHeaderButtons();
    }

    [RelayCommand]
    private void SaveHeaderButtons()
    {
        if (_isLoading) return;
        if (!TryValidateHeaderButtonLayout()) return;
        _configService.Update(c => c.HeaderButtons = HeaderButtons.Select(CloneHeaderButtonConfig).ToList(), "HeaderButtons");
    }

    [RelayCommand]
    private void AddCustomHeaderButton()
    {
        if (!EnsureCanAddCustomHeaderButton())
            return;

        LaunchTools("header-shortcut", "--header-button-mode create");
    }

    [RelayCommand]
    private void EditCustomHeaderButton(HeaderButtonConfig? item)
    {
        if (item is null || item.Kind != HeaderButtonKind.Custom)
            return;

        LaunchTools("header-shortcut", $"--header-button-id \"{item.Id}\"");
    }

    [RelayCommand]
    private void DuplicateCustomHeaderButton(HeaderButtonConfig? item)
    {
        if (item is null || item.Kind != HeaderButtonKind.Custom)
            return;

        if (!EnsureCanAddCustomHeaderButton())
            return;

        var copy = CloneHeaderButtonConfig(item);
        copy.Id = HeaderButtonConfig.CreateCustomDefault().Id;
        copy.Tooltip = string.IsNullOrWhiteSpace(copy.Tooltip) ? "English text" : $"{copy.Tooltip} English text";
        copy.AccessibleName = string.IsNullOrWhiteSpace(copy.AccessibleName) ? copy.Tooltip : $"{copy.AccessibleName} English text";

        var index = HeaderButtons.IndexOf(item);
        if (index < 0)
            HeaderButtons.Add(copy);
        else
            HeaderButtons.Insert(index + 1, copy);

        RaiseHeaderButtonCollectionChanged();
        SaveHeaderButtons();
    }

    [RelayCommand]
    private void RemoveCustomHeaderButton(HeaderButtonConfig? item)
    {
        if (item is null || item.Kind != HeaderButtonKind.Custom)
            return;

        HeaderButtons.Remove(item);
        RaiseHeaderButtonCollectionChanged();
        SaveHeaderButtons();
    }

    // ── English text) ──────────────────────────────────────────────

    [RelayCommand]
    private void OpenSettings()
    {
        if (_settingsWindow is { } win)
        {
            win.Activate();
            return;
        }

        _settingsWindow = new LArtKey.Views.SettingsWindow(this);
        AuxiliaryWindowPlacement.CenterOnScreen(_settingsWindow);
        ShowAuxiliaryWindow(_settingsWindow);
    }

    internal void OnSettingsWindowClosed() => _settingsWindow = null;

    /// <summary>
    /// [English text] English text.
    /// </summary>
    private static void ShowAuxiliaryWindow(Window window)
    {
        window.Show();

        if (window.WindowState == WindowState.Minimized)
        {
            window.WindowState = WindowState.Normal;
        }

        window.Activate();
        window.Focus();
    }

    /// <summary>
    /// [English text] English text.
    /// </summary>
    [RelayCommand]
    private void OpenSwitchScanSettings()
    {
        if (_switchScanSettingsWindow is { } existing)
        {
            existing.Activate();
            return;
        }

        _switchScanSettingsWindow = new LArtKey.Views.SwitchScanSettingsWindow(this);
        AuxiliaryWindowPlacement.CenterNear(
            _switchScanSettingsWindow,
            _settingsWindow ?? WpfApp.Current.MainWindow);
        _switchScanSettingsWindow.Closed += (_, _) => _switchScanSettingsWindow = null;
        ShowAuxiliaryWindow(_switchScanSettingsWindow);
    }

    /// <summary>
    /// [English text] English text.
    /// </summary>
    [RelayCommand]
    private void OpenFocusA11ySettings()
    {
        if (_focusA11ySettingsWindow is { } existing)
        {
            existing.Activate();
            return;
        }

        _focusA11ySettingsWindow = new LArtKey.Views.FocusA11ySettingsWindow(this);
        AuxiliaryWindowPlacement.CenterNear(
            _focusA11ySettingsWindow,
            _settingsWindow ?? WpfApp.Current.MainWindow);
        _focusA11ySettingsWindow.Closed += (_, _) => _focusA11ySettingsWindow = null;
        ShowAuxiliaryWindow(_focusA11ySettingsWindow);
    }

    // ── T-9.4: English text ──────────────────────────────────────────

    /// <summary>
    /// English text.
    /// </summary>
    [RelayCommand]
    private void OpenLayoutEditor()
    {
        LaunchTools("layout");
    }

    // ── ac-editor 03: English text ─────────────────────────────

    /// <summary>
    /// English text.
    /// </summary>
    [RelayCommand]
    private void OpenUserDictionaryEditor()
    {
        LaunchTools("dictionary");
    }

    /// <summary>
    /// English text.
    /// </summary>
    [RelayCommand]
    private void OpenProfileMappingEditor()
    {
        LaunchTools("profile");
    }

    /// <summary>
    /// AI English text.
    /// </summary>
    [RelayCommand]
    private void OpenAiPromptEditor()
    {
        LaunchTools("ai-prompt");
    }

    /// <summary>
    /// English text.
    /// </summary>
    private static void LaunchTools(string? toolName, string? extraArguments = null)
    {
        var toolsExePath = PathResolver.ToolsExePath;
        if (!File.Exists(toolsExePath))
        {
            WpfMsgBox.Show(
                "English text.\n" +
                "English text.",
                "English text",
                WpfMsgBoxButton.OK,
                WpfMsgBoxImage.Warning);
            return;
        }

        try
        {
            // English text.
            var toolArgument = string.IsNullOrWhiteSpace(toolName) ? "" : $"--tool {toolName}";
            var extraArgument = string.IsNullOrWhiteSpace(extraArguments) ? "" : extraArguments.Trim();
            var dataDirArgument = $"--data-dir \"{PathResolver.DataDir}\"";
            var argumentParts = new[] { toolArgument, extraArgument, dataDirArgument }
                .Where(part => !string.IsNullOrWhiteSpace(part));
            var arguments = string.Join(" ", argumentParts);
            var process = Process.Start(new ProcessStartInfo
            {
                FileName = toolsExePath,
                Arguments = arguments,
                UseShellExecute = true
            });

            // English text.
            if (process is not null)
            {
                _ = Task.Run(() =>
                {
                    if (!process.WaitForExit(1200))
                    {
                        return;
                    }

                    WpfApp.Current.Dispatcher.BeginInvoke(() =>
                    {
                        WpfMsgBox.Show(
                            $"English text.\n\nEnglish text: {toolsExePath}\nEnglish text.",
                            "English text",
                            WpfMsgBoxButton.OK,
                            WpfMsgBoxImage.Error);
                    });
                });
            }
        }
        catch (Exception ex)
        {
            WpfMsgBox.Show(
                $"English text: {ex.Message}",
                "English text",
                WpfMsgBoxButton.OK,
                WpfMsgBoxImage.Error);
        }
    }

    // ── English text ──────────────────────────────────────

    [RelayCommand]
    private void OpenUserSettingsFolder()
    {
        try
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = "explorer.exe",
                Arguments = $"\"{PathResolver.DataDir}\"",
                UseShellExecute = true
            });
        }
        catch (Exception ex)
        {
            WpfMsgBox.Show($"English text: {ex.Message}", "English text", WpfMsgBoxButton.OK, WpfMsgBoxImage.Error);
        }
    }

    // ── T-5.12: English text ──────────────────────────────────────

    [RelayCommand]
    private void ResetWindowLayout()
    {
        _configService.Update(c =>
        {
            c.Window = new WindowConfig();
            c.AutoCompleteEnabled = false;
        });

        var executablePath = Environment.ProcessPath;
        if (string.IsNullOrEmpty(executablePath)) return;

        try
        {
            if (WpfApp.Current.MainWindow is MainWindow mw)
                mw.ResetPending = true;

            Process.Start(new ProcessStartInfo
            {
                FileName = executablePath,
                UseShellExecute = true,
            });
            ShutdownCurrentApp();
        }
        catch (Win32Exception) { }
    }

    [RelayCommand]
    private void RestartAsAdmin()
    {
        var executablePath = Environment.ProcessPath;
        if (string.IsNullOrEmpty(executablePath)) return;

        var psi = new ProcessStartInfo
        {
            FileName = executablePath,
            Verb = "runas",
            UseShellExecute = true,
        };

        try
        {
            Process.Start(psi);
            ShutdownCurrentApp();
        }
        catch (Win32Exception)
        {
            // English text
        }
    }

    [RelayCommand]
    private void RestartAsUser()
    {
        var executablePath = Environment.ProcessPath;
        if (string.IsNullOrEmpty(executablePath)) return;

        var psi = new ProcessStartInfo
        {
            FileName = "explorer.exe",
            Arguments = $"\"{executablePath}\"",
            UseShellExecute = true
        };

        try
        {
            Process.Start(psi);
            ShutdownCurrentApp();
        }
        catch (Win32Exception)
        {
            // explorer start failed: keep current process running
        }
    }

    private static void ShutdownCurrentApp()
    {
        WpfApp.Current.Dispatcher.Invoke(() =>
        {
            if (WpfApp.Current.MainWindow is MainWindow mw)
                mw.IsShuttingDown = true;
            WpfApp.Current.Shutdown();
        });
    }

    // ── T-9.5: English text ───────────────────────────────────

    /// <summary>GitHubEnglish text</summary>
    /// <summary>
    /// English text.
    /// </summary>
    [RelayCommand]
    private async Task CheckForUpdate()
    {
        IsCheckingUpdate = true;
        UpdateStatusMessage = "English text...";
        HasUpdateAvailable = false;

        try
        {
            var (hasUpdate, version, url, installerUrl) = await _updateService.CheckAsync();

            if (string.IsNullOrEmpty(version))
            {
                UpdateStatusMessage = "English text)";
                ShowUpdateMessage("English text.\nEnglish text.");
                return;
            }

            LatestVersion = version;
            UpdateReleaseUrl = url;
            UpdateInstallerUrl = installerUrl;

            if (hasUpdate)
            {
                HasUpdateAvailable = true;
                UpdateStatusMessage = $"English text {version}English text!";
            }
            else
            {
                HasUpdateAvailable = false;
                UpdateStatusMessage = "English text.";
                ShowUpdateMessage($"English text.\nEnglish text: {CurrentVersion}");
            }
        }
        catch (Exception ex)
        {
            UpdateStatusMessage = "English text";
            ShowUpdateMessage($"English text:\n{ex.Message}");
        }
        finally
        {
            IsCheckingUpdate = false;
        }
    }

    /// <summary>English text → English text → English text)</summary>
    [RelayCommand]
    private async Task DownloadAndInstallFromSettings()
    {
        if (string.IsNullOrEmpty(UpdateInstallerUrl))
        {
            ShowUpdateMessage("English text.\nGitHub English text.");
            OpenReleasePage();
            return;
        }

        if (PathResolver.IsPortable)
        {
            ShowUpdateMessage("English text.\nGitHub English text.");
            OpenReleasePage();
            return;
        }

        try
        {
            _downloadCts = new CancellationTokenSource();

            IsDownloading = true;
            DownloadProgress = 0;
            UpdateStatusMessage = $"{LatestVersion} English text...";

            var tempDir = Path.GetTempPath();
            var installerFileName = $"LArtKey-Setup-{LatestVersion}.exe";
            var installerPath = Path.Combine(tempDir, installerFileName);

            var progress = new Progress<double>(p => DownloadProgress = p);

            await _downloadService.DownloadAsync(
                UpdateInstallerUrl,
                installerPath,
                progress,
                _downloadCts.Token);

            IsDownloading = false;
            UpdateStatusMessage = "English text...";

            // English text
            var result = WpfMsgBox.Show(
                $"LArtKey {LatestVersion} English text.\n\nEnglish text.\n\nEnglish text?",
                "English text",
                WpfMsgBoxButton.YesNo,
                WpfMsgBoxImage.Question);

            if (result != WpfMsgBoxResult.Yes)
            {
                UpdateStatusMessage = "English text.";
                try { File.Delete(installerPath); } catch { }
                return;
            }

            // English text
            IsInstalling = true;

            // 1. English text.
            _installerService.StartInstaller(
                installerPath,
                autoRestart: true,
                requestElevation: false);

            // 2. English text)
            if (WpfApp.Current.MainWindow is MainWindow mw)
                mw.IsShuttingDown = true;

            WpfApp.Current.Dispatcher.Invoke(() => WpfApp.Current.Shutdown());
        }
        catch (OperationCanceledException)
        {
            UpdateStatusMessage = "English text";
        }
        catch (Exception ex)
        {
            IsDownloading = false;
            IsInstalling = false;
            UpdateStatusMessage = "English text";

            ShowUpdateMessage($"English text:\n{ex.Message}\n\nGitHubEnglish text.");
            OpenReleasePage();
        }
        finally
        {
            _downloadCts?.Dispose();
            _downloadCts = null;
            IsDownloading = false;
        }
    }

    [RelayCommand]
    private void OpenReleasePage()
    {
        if (!string.IsNullOrEmpty(UpdateReleaseUrl))
            Process.Start(new ProcessStartInfo(UpdateReleaseUrl) { UseShellExecute = true });
    }

    [RelayCommand]
    private void CancelDownload()
    {
        _downloadCts?.Cancel();
        IsDownloading = false;
    }

    private static void ShowUpdateMessage(string message)
    {
        WpfMsgBox.Show(message, "English text", WpfMsgBoxButton.OK, WpfMsgBoxImage.Information);
    }
}

/// T-8.5: English text
public partial class ProfileEntry : ObservableObject
{
    [ObservableProperty] private string processName = "";
    [ObservableProperty] private string layoutName = "";

    public ProfileEntry(string processName, string layoutName)
    {
        ProcessName = processName;
        LayoutName = layoutName;
    }
}
