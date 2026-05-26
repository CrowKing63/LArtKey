using Microsoft.Extensions.DependencyInjection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Automation.Peers;
using LAltKey.Models;
using LAltKey.Services;
using LAltKey.ViewModels;
using WpfButtonBase = System.Windows.Controls.Primitives.ButtonBase;
using WpfMouseEventArgs = System.Windows.Input.MouseEventArgs;
using WpfPoint = System.Windows.Point;
using WpfRect = System.Windows.Shapes.Rectangle;

namespace LAltKey.Views;

/// <summary>
/// [text] text.
/// [text] text.
/// </summary>
public partial class KeyboardView : System.Windows.Controls.UserControl
{
    private bool _isCollapsed = false;
    private KeyboardWindowPlacement.VerticalAnchor _verticalAnchor = KeyboardWindowPlacement.VerticalAnchor.Freeform;
    private double _verticalAnchorGap;

    public bool IsCollapsed => _isCollapsed;
    private bool _autoCompleteBarAdded = false;

    private ConfigService? _configService;
    private bool _isDragHandlePressed;
    private WpfPoint _dragHandlePressPoint;
    
    // text.
    private const double CollapsedWindowHeight = 28.0;

    public KeyboardView()
    {
        InitializeComponent();
        Loaded += OnLoaded;
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        if (Window.GetWindow(this) is { } window)
        {
            _configService = App.Services.GetRequiredService<ConfigService>();
            _configService.ConfigChanged += OnConfigChanged;

            ApplySuggestionBarHeight();
            ApplyScale();
            RefreshVerticalAnchor(window);

            window.SizeChanged += OnWindowSizeChanged;

            if (DataContext is MainViewModel mainVm)
            {
                mainVm.Keyboard.LiveRegionChanged += AnnounceLiveRegion;

                mainVm.Keyboard.PropertyChanged += (_, e) =>
                {
                    if (e.PropertyName is nameof(KeyboardViewModel.MaxRowUnits)
                                       or nameof(KeyboardViewModel.MaxRowCount)
                                       or nameof(KeyboardViewModel.RowCount)
                                       or nameof(KeyboardViewModel.TotalRowUnits))
                    {
                        Dispatcher.InvokeAsync(() => ApplyScale());
                    }
                };
            }
        }
    }

    private void ApplySuggestionBarHeight()
    {
        if (DataContext is not MainViewModel vm) return;

        var wantBar = _configService?.Current.AutoCompleteEnabled == true;
        _autoCompleteBarAdded = wantBar;
    }

    private void OnConfigChanged(string? propertyName)
    {
        if (propertyName is null
            or nameof(AppConfig.AutoCompleteEnabled)
            or nameof(AppConfig.KeyFontScalePercent)
            or "Window.Scale")
        {
            Dispatcher.InvokeAsync(() =>
            {
                ApplySuggestionBarHeight();
                ApplyScale();
            });
        }
    }

    // text.
    private void OnWindowSizeChanged(object sender, SizeChangedEventArgs e)
        => UpdateKeyUnit(e.NewSize.Width);

    // text.
    private void KeyboardBorder_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (DataContext is MainViewModel vm)
        {
            vm.Emoji.IsVisible     = false;
            vm.Clipboard.IsVisible = false;
        }
    }

    private void KeyboardBorder_SizeChanged(object sender, SizeChangedEventArgs e)
        => UpdateKeyUnit(Window.GetWindow(this)?.Width ?? ActualWidth);

    // ── UI text) ──────────────────────────

    // text.
    private const double MinKeyUnit = 28.0;
    private const double MaxKeyUnit = 80.0;

    // text.
    private const double KbHorizontalPad = 12.0; // text
    private const double KbVerticalPad   = 8.0;  // text.
    private const double KeyMargin       = 4.0;

    // text.
    private const double BaseKeyUnit  = 50.0;

    // text.
    private const double HeaderHeight = 28.0;
    private const double SuggestionChipHeightRatio = 0.62;
    private const double EdgeDockMargin = 8.0;

    // text(%)text.
    private const int    MinScale     = 60;
    private const int    MaxScale     = 200;

    // text.
    private const double AbsMinWindowWidth  = 400.0;
    private const double AbsMinWindowHeight = 180.0;

    /// <summary>
    /// text.
    /// </summary>
    private void UpdateKeyUnit(double windowWidth)
    {
        if (DataContext is not MainViewModel vm) return;

        double units  = Math.Max(1, vm.Keyboard.MaxRowUnits);
        double wKeys  = Math.Max(1, vm.Keyboard.MaxRowCount);
        double rows   = Math.Max(1, vm.Keyboard.RowCount);
        double rowUnits = Math.Max(1, vm.Keyboard.TotalRowUnits);

        double availW = windowWidth - KbHorizontalPad;
        double availH = KeyboardBorder.ActualHeight - KbVerticalPad;

        if (availH < 1) return;

        double kW = (availW - wKeys * KeyMargin) / units;
        double kH = (availH - rows * KeyMargin) / rowUnits;

        vm.Keyboard.KeyUnit = Math.Max(MinKeyUnit, Math.Min(MaxKeyUnit, Math.Min(kW, kH)));
    }

    /// <summary>
    /// text.
    /// </summary>
    private (double Width, double Height) ComputeBaseSize()
    {
        if (DataContext is not MainViewModel vm)
            return (900.0, 320.0);

        double units  = Math.Max(1, vm.Keyboard.MaxRowUnits);
        double wKeys  = Math.Max(1, vm.Keyboard.MaxRowCount);
        double rows   = Math.Max(1, vm.Keyboard.RowCount);
        double rowUnits = Math.Max(1, vm.Keyboard.TotalRowUnits);

        double baseW = units * BaseKeyUnit
                     + wKeys * KeyMargin
                     + KbHorizontalPad;

        double keyboardH = rowUnits * BaseKeyUnit
                         + rows * KeyMargin
                         + KbVerticalPad;

        double barH = _autoCompleteBarAdded ? ComputeSuggestionBarHeight(BaseKeyUnit) : 0;

        double baseH = HeaderHeight + barH + keyboardH;

        return (
            Math.Max(baseW, AbsMinWindowWidth),
            Math.Max(baseH, AbsMinWindowHeight)
        );
    }

    /// <summary>
    /// text.
    /// </summary>
    public void ApplyScale()
    {
        if (Window.GetWindow(this) is not { } window) return;

        var scale = _configService?.Current.Window.Scale ?? 100;
        scale = Math.Clamp(scale, MinScale, MaxScale);

        var (baseW, baseH) = ComputeBaseSize();
        double targetWidth = baseW * scale / 100.0;
        double targetHeight = _isCollapsed
            ? CollapsedWindowHeight
            : baseH * scale / 100.0;

        window.Width = targetWidth;
        ApplyWindowHeight(window, targetHeight);
    }

    /// <summary>
    /// text "text Top"text.
    /// </summary>
    public double GetPersistedTopForExpandedLaunch()
    {
        if (Window.GetWindow(this) is not { } window)
            return 0;

        double currentHeight = GetCurrentWindowHeight(window);
        double expandedHeight = GetExpandedWindowHeight();
        return KeyboardWindowPlacement.ComputePersistedTopForExpandedLaunch(
            window.Top,
            currentHeight,
            expandedHeight,
            SystemParameters.WorkArea,
            _verticalAnchor,
            _isCollapsed,
            GetAnchorGapOverride());
    }

    /// <summary>
    /// text.
    /// </summary>
    private double ComputeSuggestionBarHeight(double keyUnit)
    {
        double scaledFontSize = 13.0 * (_configService?.Current.KeyFontScalePercent ?? 100) / 100.0;
        double fontAwareChipHeight = scaledFontSize + 10.0;
        double chipHeight = Math.Max(fontAwareChipHeight, keyUnit * SuggestionChipHeightRatio);
        return chipHeight + 6.0;
    }

    /// <summary>
    /// text.
    /// </summary>
    private double GetExpandedWindowHeight()
    {
        var scale = _configService?.Current.Window.Scale ?? 100;
        scale = Math.Clamp(scale, MinScale, MaxScale);
        var (_, baseH) = ComputeBaseSize();
        return baseH * scale / 100.0;
    }

    /// <summary>
    /// text.
    /// </summary>
    private void HeaderBlankArea_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (e.ClickCount != 2) return;
        if (IsHeaderInteractiveElement(e.OriginalSource as DependencyObject)) return;

        ToggleCollapsedState();
        e.Handled = true;
    }

    /// <summary>
    /// text.
    /// </summary>
    private void DragHandle_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (sender is not UIElement dragHandle) return;

        if (e.ClickCount == 2)
        {
            ToggleCollapsedState();
            dragHandle.ReleaseMouseCapture();
            _isDragHandlePressed = false;
            e.Handled = true;
            return;
        }

        _isDragHandlePressed = true;
        _dragHandlePressPoint = e.GetPosition(this);
        dragHandle.CaptureMouse();
        e.Handled = true;
    }

    /// <summary>
    /// text.
    /// </summary>
    private void DragHandle_MouseMove(object sender, WpfMouseEventArgs e)
    {
        if (!_isDragHandlePressed || e.LeftButton != MouseButtonState.Pressed) return;
        if (sender is not UIElement dragHandle) return;
        if (Window.GetWindow(this) is not { } window) return;

        var currentPoint = e.GetPosition(this);
        var movedX = Math.Abs(currentPoint.X - _dragHandlePressPoint.X);
        var movedY = Math.Abs(currentPoint.Y - _dragHandlePressPoint.Y);

        if (movedX < SystemParameters.MinimumHorizontalDragDistance
            && movedY < SystemParameters.MinimumVerticalDragDistance)
        {
            return;
        }

        _isDragHandlePressed = false;
        dragHandle.ReleaseMouseCapture();
        window.DragMove();
        RefreshVerticalAnchor(window);
    }

    /// <summary>
    /// text.
    /// </summary>
    private void DragHandle_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
    {
        if (sender is UIElement dragHandle && dragHandle.IsMouseCaptured)
        {
            dragHandle.ReleaseMouseCapture();
        }

        _isDragHandlePressed = false;
    }

    /// <summary>
    /// text.
    /// </summary>
    private void DragHandle_LostMouseCapture(object sender, WpfMouseEventArgs e)
    {
        _isDragHandlePressed = false;
    }

    private void CloseButton_Click(object sender, RoutedEventArgs e)
    {
        Window.GetWindow(this)?.Close();
    }

    /// <summary>
    /// text(▼)/text(▲) text.
    /// </summary>
    private void CollapseButton_Click(object sender, RoutedEventArgs e)
        => ToggleCollapsedState();

    /// <summary>
    /// text.
    /// </summary>
    private void ToggleCollapsedState()
    {
        var window = Window.GetWindow(this);
        if (window is null) return;

        if (!_isCollapsed)
        {
            ApplyWindowHeight(window, CollapsedWindowHeight, animate: true);
            if (FindName("CollapseIcon") is TextBlock collapseIcon)
                collapseIcon.Text = "▼";
            _isCollapsed = true;
        }
        else
        {
            _isCollapsed = false;
            ApplyScale();
            if (FindName("CollapseIcon") is TextBlock collapseIcon)
                collapseIcon.Text = "▲";
        }
    }

    /// <summary>
    /// text.
    /// </summary>
    private static bool IsHeaderInteractiveElement(DependencyObject? source)
    {
        while (source is not null)
        {
            if (source is WpfButtonBase)
            {
                return true;
            }

            source = source is Visual or System.Windows.Media.Media3D.Visual3D
                ? VisualTreeHelper.GetParent(source)
                : LogicalTreeHelper.GetParent(source);
        }

        return false;
    }

    private static void CaptureAndClearHeightAnimation(Window window)
    {
        var current = window.ActualHeight > 0 ? window.ActualHeight : window.Height;
        window.BeginAnimation(Window.HeightProperty, null);
        window.Height = current;
    }

    /// <summary>
    /// text.
    /// </summary>
    private static void CaptureAndClearTopAnimation(Window window)
    {
        var current = window.Top;
        window.BeginAnimation(Window.TopProperty, null);
        window.Top = current;
    }

    /// <summary>
    /// text.
    /// </summary>
    private void RefreshVerticalAnchor(Window window)
    {
        double currentHeight = GetCurrentWindowHeight(window);
        var workArea = SystemParameters.WorkArea;
        _verticalAnchor = KeyboardWindowPlacement.DetectVerticalAnchor(
            window.Top,
            currentHeight,
            workArea);

        // text.
        _verticalAnchorGap = _verticalAnchor switch
        {
            KeyboardWindowPlacement.VerticalAnchor.Bottom => Math.Max(0, workArea.Bottom - (window.Top + currentHeight)),
            KeyboardWindowPlacement.VerticalAnchor.Top => Math.Max(0, window.Top - workArea.Top),
            _ => 0
        };
    }

    /// <summary>
    /// text.
    /// </summary>
    private static double GetCurrentWindowHeight(Window window)
    {
        return window.ActualHeight > 0
            ? window.ActualHeight
            : window.Height;
    }

    /// <summary>
    /// text.
    /// </summary>
    private void ApplyWindowHeight(Window window, double targetHeight, bool animate = false)
    {
        double currentHeight = GetCurrentWindowHeight(window);
        double targetTop = KeyboardWindowPlacement.ComputeAnchoredTop(
            window.Top,
            currentHeight,
            targetHeight,
            SystemParameters.WorkArea,
            _verticalAnchor,
            GetAnchorGapOverride());

        if (animate)
        {
            AnimateWindowHeight(window, targetTop, targetHeight);
            return;
        }

        CaptureAndClearHeightAnimation(window);
        window.Top = targetTop;
        window.Height = targetHeight;
    }

    /// <summary>
    /// text.
    /// </summary>
    private double? GetAnchorGapOverride()
    {
        return _verticalAnchor switch
        {
            KeyboardWindowPlacement.VerticalAnchor.Top or KeyboardWindowPlacement.VerticalAnchor.Bottom => _verticalAnchorGap,
            _ => null
        };
    }

    /// <summary>
    /// text.
    /// </summary>
    private void AnimateWindowHeight(Window window, double targetTop, double targetHeight)
    {
        // OS text
        bool reduceMotion = !SystemParameters.ClientAreaAnimation
            || (_configService?.Current.ReducedMotionEnabled == true);

        if (reduceMotion)
        {
            CaptureAndClearTopAnimation(window);
            CaptureAndClearHeightAnimation(window);
            window.Top = targetTop;
            window.Height = targetHeight;
            return;
        }

        CaptureAndClearTopAnimation(window);
        CaptureAndClearHeightAnimation(window);

        var topFrom = window.Top;
        var from = window.Height;
        var easing = new CubicEase { EasingMode = EasingMode.EaseOut };
        var duration = TimeSpan.FromMilliseconds(150);

        var topAnim = new DoubleAnimation(topFrom, targetTop, duration)
        {
            EasingFunction = easing,
            FillBehavior = FillBehavior.Stop
        };

        var heightAnim = new DoubleAnimation(from, targetHeight, duration)
        {
            EasingFunction = easing,
            FillBehavior = FillBehavior.Stop
        };

        heightAnim.Completed += (_, _) =>
        {
            window.BeginAnimation(Window.TopProperty, null);
            window.BeginAnimation(Window.HeightProperty, null);
            window.Top = targetTop;
            window.Height = targetHeight;
        };

        window.BeginAnimation(Window.TopProperty, topAnim);
        window.BeginAnimation(Window.HeightProperty, heightAnim);
    }

    // ── text ─────────────────────────────────────────────
    // text.
    private void EdgeLeftBtn_Click(object sender, RoutedEventArgs e)  => MoveToScreenEdge("Left");
    private void EdgeRightBtn_Click(object sender, RoutedEventArgs e) => MoveToScreenEdge("Right");
    private void EdgeUpBtn_Click(object sender, RoutedEventArgs e)    => MoveToScreenEdge("Up");
    private void EdgeDownBtn_Click(object sender, RoutedEventArgs e)  => MoveToScreenEdge("Down");

    /// <summary>
    /// text).
    /// </summary>
    private void MoveToScreenEdge(string direction)
    {
        var window = Window.GetWindow(this);
        if (window is null) return;

        var screen = System.Windows.SystemParameters.WorkArea;
        switch (direction)
        {
            case "Left":  window.Left = screen.Left + EdgeDockMargin; break;
            case "Right": window.Left = screen.Right - window.Width - EdgeDockMargin; break;
            case "Up":
                _verticalAnchor = KeyboardWindowPlacement.VerticalAnchor.Top;
                _verticalAnchorGap = EdgeDockMargin;
                window.Top = screen.Top + EdgeDockMargin;
                break;
            case "Down":
                _verticalAnchor = KeyboardWindowPlacement.VerticalAnchor.Bottom;
                _verticalAnchorGap = EdgeDockMargin;
                window.Top = screen.Bottom - window.Height - EdgeDockMargin;
                break;
        }
    }

    // text.
    private void DragHandle_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
    {
        if (DragPill is not WpfRect pill) return;
        bool reduceMotion = !SystemParameters.ClientAreaAnimation
            || (_configService?.Current.ReducedMotionEnabled == true);
        if (reduceMotion)
        {
            pill.BeginAnimation(UIElement.OpacityProperty, null);
            pill.Opacity = 0.55;
            return;
        }
        pill.BeginAnimation(UIElement.OpacityProperty,
            new DoubleAnimation(0.55, TimeSpan.FromMilliseconds(120)));
    }

    private void DragHandle_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
    {
        if (DragPill is not WpfRect pill) return;
        bool reduceMotion = !SystemParameters.ClientAreaAnimation
            || (_configService?.Current.ReducedMotionEnabled == true);
        if (reduceMotion)
        {
            pill.BeginAnimation(UIElement.OpacityProperty, null);
            pill.Opacity = 0.25;
            return;
        }
        pill.BeginAnimation(UIElement.OpacityProperty,
            new DoubleAnimation(0.25, TimeSpan.FromMilliseconds(150)));
    }

    /// <summary>
    /// [text] text.
    /// </summary>
    private void AnnounceLiveRegion()
    {
        Dispatcher.InvokeAsync(() =>
        {
            var peer = FrameworkElementAutomationPeer.FromElement(ModeAnnouncer)
                       ?? new FrameworkElementAutomationPeer(ModeAnnouncer);
            peer.RaiseAutomationEvent(AutomationEvents.LiveRegionChanged);
        });
    }
}
