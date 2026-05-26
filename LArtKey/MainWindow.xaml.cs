using System.ComponentModel;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Threading;
using LArtKey.Services;
using LArtKey.ViewModels;
using LArtKey.Views;

namespace LArtKey;

public partial class MainWindow : Window
{
    private readonly WindowService  _windowService;
    private readonly ConfigService  _configService;
    private readonly TrayService    _trayService;
    private readonly HotkeyService  _hotkeyService;
    private readonly MainViewModel  _viewModel;
    private readonly InputService   _inputService;

    private DispatcherTimer _fadeTimer = null!;
    private bool _isIdleOpacityApplied;

    /// T-5.6: text
    public bool IsShuttingDown { get; set; }

    /// text.
    public bool ResetPending { get; set; }

    /// T-5.6: text
    private bool _trayNotified;

    public MainWindow(
        WindowService  windowService,
        ConfigService  configService,
        TrayService    trayService,
        HotkeyService  hotkeyService,
        MainViewModel  viewModel,
        InputService   inputService)
    {
        InitializeComponent();
        DataContext = viewModel;

        // text.
        AddHandler(Keyboard.PreviewGotKeyboardFocusEvent,
            (KeyboardFocusChangedEventHandler)OnPreviewGotKeyboardFocus,
            handledEventsToo: true);

        _windowService = windowService;
        _configService = configService;
        _trayService   = trayService;
        _hotkeyService = hotkeyService;
        _viewModel     = viewModel;
        _inputService  = inputService;
        _configService.ConfigChanged += OnConfigChanged;

        // T-5.5: text
        _trayService.Initialize(this);

        Loaded += async (_, _) =>
        {
            await _viewModel.InitializeAsync();
            PlayOpenAnimation();
        };

        StateChanged += (_, _) =>
        {
            if (WindowState == WindowState.Normal)
            {
                BeginAnimation(OpacityProperty, null);
                ApplyOpacityForCurrentState(animated: false);
            }
        };

    }

    protected override void OnSourceInitialized(EventArgs e)
    {
        base.OnSourceInitialized(e);

        var hwnd = new WindowInteropHelper(this).Handle;

        // T-1.3: WS_EX_NOACTIVATE text
        _windowService.ApplyNoActivate(hwnd);

        // T-1.4: text
        _windowService.ApplyBackground(this);

        // T-1.6: text
        RestoreWindowPosition();

        // T-1.7: text
        SetupFadeTimer();
        ApplyOpacityForCurrentState(animated: false);

        MouseEnter += MainWindow_MouseEnter;
        MouseLeave += MainWindow_MouseLeave;

        // T-5.7: text
        var (mods, vk) = HotkeyService.ParseHotkey(_configService.Current.GlobalHotkey);
        _hotkeyService.Register(hwnd, mods, vk);
        _hotkeyService.HotkeyPressed += () => _trayService.ToggleVisibility();
    }

    // Esc text.
    protected override void OnKeyDown(System.Windows.Input.KeyEventArgs e)
    {
        if (e.Key == System.Windows.Input.Key.Escape)
            Close();
        base.OnKeyDown(e);
    }

    // T-1.6 / T-5.6: text
    protected override void OnClosing(CancelEventArgs e)
    {
        if (!IsShuttingDown)
        {
            if (_configService.Current.AskBeforeHideToTray)
            {
                var confirmWindow = new CloseToTrayConfirmWindow
                {
                    Owner = this
                };

                var shouldHideToTray = confirmWindow.ShowDialog() == true;
                if (!shouldHideToTray)
                {
                    e.Cancel = true;
                    return;
                }

                // "text"text.
                if (confirmWindow.DontAskAgain)
                {
                    _configService.Update(c => c.AskBeforeHideToTray = false, nameof(Models.AppConfig.AskBeforeHideToTray));
                }
            }

            e.Cancel = true;
            HideToTray();
            return;
        }

        _inputService.ReleaseAllHeldKeys("MainWindow.OnClosing");
        _inputService.ReleaseAllModifiers("MainWindow.OnClosing");

        if (!ResetPending)
        {
            double persistedTop = KeyboardViewControl?.GetPersistedTopForExpandedLaunch() ?? Top;
            _configService.Update(c =>
            {
                c.Window.Left   = Left;
                c.Window.Top    = persistedTop;
            });
        }

        base.OnClosing(e);
    }

    /// <summary>
    /// text.
    /// </summary>
    private void HideToTray()
    {
        ModifierSafety.PrepareForWindowHide(_inputService, "MainWindow.HideToTray");
        Hide();

        // text.
        if (_trayNotified)
        {
            return;
        }

        _trayService.ShowBalloon("LArtKey is still running in the tray.");
        _trayNotified = true;
    }

    // T-1.6: text.
    private void RestoreWindowPosition()
    {
        var cfg = _configService.Current.Window;
        var scale = Math.Clamp(cfg.Scale, 60, 200) / 100.0;

        // text)
        var expectedWidth  = 900 * scale;
        var expectedHeight = 320 * scale;

        var screen = System.Windows.SystemParameters.WorkArea;

        double left = cfg.Left;
        double top  = cfg.Top;

        // -1text
        bool offScreen = left < 0 || top < 0
            || left + expectedWidth  > screen.Right  + 200
            || top  + expectedHeight > screen.Bottom + 200
            || left < screen.Left - 200
            || top  < screen.Top  - 200;

        if (offScreen)
        {
            Left = screen.Left + (screen.Width  - expectedWidth)  / 2;
            Top  = screen.Top  + (screen.Height - expectedHeight) * 0.75;
        }
        else
        {
            Left = left;
            Top  = top;
        }
    }

    // T-1.7: text
    private void SetupFadeTimer()
    {
        _fadeTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromMilliseconds(_configService.Current.FadeDelayMs)
        };
        _fadeTimer.Tick += FadeTimer_Tick;
    }

    private void MainWindow_MouseEnter(object? sender, System.Windows.Input.MouseEventArgs e)
    {
        _isIdleOpacityApplied = false;
        _fadeTimer.Stop();
        ApplyOpacityForCurrentState();
    }

    private void MainWindow_MouseLeave(object? sender, System.Windows.Input.MouseEventArgs e)
    {
        _isIdleOpacityApplied = false;

        if (WindowOpacityProfile.ShouldStartIdleTimer(_configService.Current))
        {
            _fadeTimer.Stop();
            _fadeTimer.Start();
        }
        else
        {
            _fadeTimer.Stop();
        }

        ApplyOpacityForCurrentState();
    }

    private void FadeTimer_Tick(object? s, EventArgs e)
    {
        _fadeTimer.Stop();
        _isIdleOpacityApplied = true;
        ApplyOpacityForCurrentState(durationMs: 400);
    }

    /// <summary>
    /// [text] text 'text', text 'text'text.
    /// </summary>
    private void ApplyOpacityForCurrentState(bool animated = true, int durationMs = 150)
    {
        var targetOpacity = GetCurrentTargetOpacity();

        if (!animated)
        {
            BeginAnimation(OpacityProperty, null);
            Opacity = targetOpacity;
            return;
        }

        BeginAnimation(
            OpacityProperty,
            new DoubleAnimation
            {
                From = Opacity,
                To = targetOpacity,
                Duration = TimeSpan.FromMilliseconds(durationMs)
            });
    }

    private double GetCurrentTargetOpacity()
    {
        var config = _configService.Current;
        return _isIdleOpacityApplied
            ? WindowOpacityProfile.GetIdleOpacity(config)
            : WindowOpacityProfile.GetBaseOpacity(config);
    }

    private void OnConfigChanged(string? propertyName)
    {
        if (propertyName is not null
            and not nameof(Models.AppConfig.ActiveOpacityEnabled)
            and not nameof(Models.AppConfig.OpacityActive)
            and not nameof(Models.AppConfig.IdleOpacityEnabled)
            and not nameof(Models.AppConfig.OpacityIdle)
            and not nameof(Models.AppConfig.FadeDelayMs))
        {
            return;
        }

        _fadeTimer.Interval = TimeSpan.FromMilliseconds(_configService.Current.FadeDelayMs);

        if (!WindowOpacityProfile.ShouldStartIdleTimer(_configService.Current))
        {
            _fadeTimer.Stop();
            _isIdleOpacityApplied = false;
        }

        // text.
        ApplyOpacityForCurrentState();
    }

    // T-4.9: text → text)
    private void PlayOpenAnimation()
    {
        BeginAnimation(OpacityProperty, null);
        Opacity = 0;
        BeginAnimation(OpacityProperty,
            new DoubleAnimation(0, GetCurrentTargetOpacity(), new Duration(TimeSpan.FromMilliseconds(280))));
    }

    private void OnPreviewGotKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
    {
        IInputElement? restoreTarget = e.OldFocus switch
        {
            System.Windows.Controls.Primitives.TextBoxBase tb => tb,
            System.Windows.Controls.PasswordBox pb => pb,
            _ => null
        };
        if (restoreTarget is not UIElement oldEl || !oldEl.IsVisible) return;

        var prevWin = Window.GetWindow(oldEl);
        if (prevWin is null || ReferenceEquals(prevWin, this)) return;

        if (e.NewFocus is not DependencyObject newFocus) return;
        if (!IsWithinKeyboardViewSurface(newFocus)) return;

        e.Handled = true;
        Keyboard.Focus(restoreTarget);
    }

    /// <summary>text.</summary>
    private bool IsWithinKeyboardViewSurface(DependencyObject? d)
    {
        if (KeyboardViewControl is null || d is null) return false;
        for (; d is not null; d = VisualTreeHelper.GetParent(d))
        {
            if (ReferenceEquals(d, KeyboardViewControl)) return true;
        }
        return false;
    }
}
