using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;
using LArtKey.Models;
using LArtKey.Services;
using LArtKey.ViewModels;
using Microsoft.Extensions.DependencyInjection;

namespace LArtKey.Controls;

/// <summary>
/// [English text] English text.
/// [English text] English text.
/// </summary>
public class KeyButton : System.Windows.Controls.Button
{
    private enum RepeatPolicy
    {
        Disabled,
        HeldVirtualKey,
        LegacyRepeat
    }

    static KeyButton()
    {
        DefaultStyleKeyProperty.OverrideMetadata(
            typeof(KeyButton),
            new FrameworkPropertyMetadata(typeof(KeyButton)));
    }

    // ── Dependency Properties (WPF UIEnglish text) ────────────────────────

    // English text.
    public static readonly DependencyProperty SlotProperty =
        DependencyProperty.Register(
            nameof(Slot), typeof(KeySlotVm), typeof(KeyButton),
            new PropertyMetadata(null, OnSlotChanged));

    // Shift English text.
    public static readonly DependencyProperty ShowUpperCaseProperty =
        DependencyProperty.Register(
            nameof(ShowUpperCase), typeof(bool), typeof(KeyButton),
            new PropertyMetadata(false, OnShowUpperCaseChanged));

    // English text.
    public static readonly DependencyProperty SubLabelProperty =
        DependencyProperty.Register(
            nameof(SubLabel), typeof(string), typeof(KeyButton),
            new PropertyMetadata(""));

    // English text.
    public static readonly DependencyProperty DisplayLabelProperty =
        DependencyProperty.Register(
            nameof(DisplayLabel), typeof(string), typeof(KeyButton),
            new PropertyMetadata("", OnDisplayLabelChanged));

    // English text.
    public static readonly DependencyProperty IsDimmedProperty =
        DependencyProperty.Register(
            nameof(IsDimmed), typeof(bool), typeof(KeyButton),
            new PropertyMetadata(false, OnIsDimmedChanged));

    // Shift/Ctrl English text 'English text'English text.
    public static readonly DependencyProperty IsStickyProperty =
        DependencyProperty.Register(
            nameof(IsSticky), typeof(bool), typeof(KeyButton),
            new PropertyMetadata(false));

    // Caps Lock English text 'English text'English text.
    public static readonly DependencyProperty IsLockedProperty =
        DependencyProperty.Register(
            nameof(IsLocked), typeof(bool), typeof(KeyButton),
            new PropertyMetadata(false));

    public static readonly DependencyProperty IsFunctionOneShotProperty =
        DependencyProperty.Register(
            nameof(IsFunctionOneShot), typeof(bool), typeof(KeyButton),
            new PropertyMetadata(false));

    public static readonly DependencyProperty IsFunctionLockedProperty =
        DependencyProperty.Register(
            nameof(IsFunctionLocked), typeof(bool), typeof(KeyButton),
            new PropertyMetadata(false));

    public static readonly DependencyProperty HasFunctionLayerAccentProperty =
        DependencyProperty.Register(
            nameof(HasFunctionLayerAccent), typeof(bool), typeof(KeyButton),
            new PropertyMetadata(false));

    // English text.
    public static readonly DependencyProperty KeyUnitProperty =
        DependencyProperty.Register(
            nameof(KeyUnit), typeof(double), typeof(KeyButton),
            new PropertyMetadata(48.0, OnKeyUnitChanged));

    // [English text] English text.
    public static readonly DependencyProperty DwellEnabledProperty =
        DependencyProperty.Register(
            nameof(DwellEnabled), typeof(bool), typeof(KeyButton),
            new PropertyMetadata(false));

    // English text.
    public static readonly DependencyProperty DwellTimeProperty =
        DependencyProperty.Register(
            nameof(DwellTime), typeof(int), typeof(KeyButton),
            new PropertyMetadata(800));

    // English text (0.0 ~ 1.0). English text.
    public static readonly DependencyProperty DwellProgressProperty =
        DependencyProperty.Register(
            nameof(DwellProgress), typeof(double), typeof(KeyButton),
            new PropertyMetadata(0.0));

    // English text.
    public static readonly DependencyProperty KeyRepeatEnabledProperty =
        DependencyProperty.Register(
            nameof(KeyRepeatEnabled), typeof(bool), typeof(KeyButton),
            new PropertyMetadata(false));

    // English text.
    public static readonly DependencyProperty KeyRepeatDelayMsProperty =
        DependencyProperty.Register(
            nameof(KeyRepeatDelayMs), typeof(int), typeof(KeyButton),
            new PropertyMetadata(300));

    // English text.
    public static readonly DependencyProperty KeyRepeatIntervalMsProperty =
        DependencyProperty.Register(
            nameof(KeyRepeatIntervalMs), typeof(int), typeof(KeyButton),
            new PropertyMetadata(50));

    // [English text] English text.
    public static readonly DependencyProperty KeyboardA11yNavigationEnabledProperty =
        DependencyProperty.Register(
            nameof(KeyboardA11yNavigationEnabled), typeof(bool), typeof(KeyButton),
            new PropertyMetadata(false, OnKeyboardA11yNavigationEnabledChanged));

    // English text.
    public static readonly DependencyProperty IsA11yFocusedProperty =
        DependencyProperty.Register(
            nameof(IsA11yFocused), typeof(bool), typeof(KeyButton),
            new PropertyMetadata(false));

    // [English text][L2] English text.
    public static readonly DependencyProperty ReducedMotionEnabledProperty =
        DependencyProperty.Register(
            nameof(ReducedMotionEnabled), typeof(bool), typeof(KeyButton),
            new PropertyMetadata(false));

    // [English text][L2] English text.
    public static readonly DependencyProperty TtsOnHoverProperty =
        DependencyProperty.Register(
            nameof(TtsOnHover), typeof(bool), typeof(KeyButton),
            new PropertyMetadata(false));

    // ── Properties ──────────────────────────────────────────────────────────

    public KeySlotVm? Slot
    {
        get => (KeySlotVm?)GetValue(SlotProperty);
        set => SetValue(SlotProperty, value);
    }

    public bool ShowUpperCase
    {
        get => (bool)GetValue(ShowUpperCaseProperty);
        set => SetValue(ShowUpperCaseProperty, value);
    }

    public string SubLabel
    {
        get => (string)GetValue(SubLabelProperty);
        set => SetValue(SubLabelProperty, value);
    }

    public string DisplayLabel
    {
        get => (string)GetValue(DisplayLabelProperty);
        set => SetValue(DisplayLabelProperty, value);
    }

    public bool IsDimmed
    {
        get => (bool)GetValue(IsDimmedProperty);
        set => SetValue(IsDimmedProperty, value);
    }

    public bool IsSticky
    {
        get => (bool)GetValue(IsStickyProperty);
        set => SetValue(IsStickyProperty, value);
    }

    public bool IsLocked
    {
        get => (bool)GetValue(IsLockedProperty);
        set => SetValue(IsLockedProperty, value);
    }

    public bool IsFunctionOneShot
    {
        get => (bool)GetValue(IsFunctionOneShotProperty);
        set => SetValue(IsFunctionOneShotProperty, value);
    }

    public bool IsFunctionLocked
    {
        get => (bool)GetValue(IsFunctionLockedProperty);
        set => SetValue(IsFunctionLockedProperty, value);
    }

    public bool HasFunctionLayerAccent
    {
        get => (bool)GetValue(HasFunctionLayerAccentProperty);
        set => SetValue(HasFunctionLayerAccentProperty, value);
    }

    public double KeyUnit
    {
        get => (double)GetValue(KeyUnitProperty);
        set => SetValue(KeyUnitProperty, value);
    }

    public bool DwellEnabled
    {
        get => (bool)GetValue(DwellEnabledProperty);
        set => SetValue(DwellEnabledProperty, value);
    }

    public int DwellTime
    {
        get => (int)GetValue(DwellTimeProperty);
        set => SetValue(DwellTimeProperty, value);
    }

    public double DwellProgress
    {
        get => (double)GetValue(DwellProgressProperty);
        private set => SetValue(DwellProgressProperty, value);
    }

    public bool KeyRepeatEnabled
    {
        get => (bool)GetValue(KeyRepeatEnabledProperty);
        set => SetValue(KeyRepeatEnabledProperty, value);
    }

    public int KeyRepeatDelayMs
    {
        get => (int)GetValue(KeyRepeatDelayMsProperty);
        set => SetValue(KeyRepeatDelayMsProperty, value);
    }

    public int KeyRepeatIntervalMs
    {
        get => (int)GetValue(KeyRepeatIntervalMsProperty);
        set => SetValue(KeyRepeatIntervalMsProperty, value);
    }

    public bool KeyboardA11yNavigationEnabled
    {
        get => (bool)GetValue(KeyboardA11yNavigationEnabledProperty);
        set => SetValue(KeyboardA11yNavigationEnabledProperty, value);
    }

    public bool IsA11yFocused
    {
        get => (bool)GetValue(IsA11yFocusedProperty);
        set => SetValue(IsA11yFocusedProperty, value);
    }

    public bool ReducedMotionEnabled
    {
        get => (bool)GetValue(ReducedMotionEnabledProperty);
        set => SetValue(ReducedMotionEnabledProperty, value);
    }

    public bool TtsOnHover
    {
        get => (bool)GetValue(TtsOnHoverProperty);
        set => SetValue(TtsOnHoverProperty, value);
    }

    // ── English text ─────────────────────────────────────────────────────

    private DispatcherTimer? _dwellTimer;
    private DateTime         _dwellStart;

    // T-10: English text
    private DispatcherTimer? _repeatDelayTimer;
    private DispatcherTimer? _repeatTimer;
    private bool _isRepeating;
    private bool _isHoldPrimed;
    private VirtualKeyCode? _primedHeldKey;
    private VirtualKeyCode? _activeHeldKey;
    private bool _suppressNextClick;
    private InputService? _inputModeResetInputService;

    public KeyButton()
    {
        Loaded += OnLoaded;
        Unloaded += OnUnloaded;
    }

    internal static string DescribeRepeatPolicyForTests(LArtKey.Services.InputMode inputMode, KeyAction? action)
        => ClassifyRepeatPolicy(inputMode, action).ToString();

    internal static bool ShouldCancelCompositionOnRepeatStartForTests(LArtKey.Services.InputMode inputMode, KeyAction? action)
        => ShouldCancelCompositionOnRepeatStart(ClassifyRepeatPolicy(inputMode, action), TryGetVirtualKey(action));

    internal static VirtualKeyCode? GetHoldableVirtualKeyForTests(LArtKey.Services.InputMode inputMode, KeyAction? action)
    {
        var policy = ClassifyRepeatPolicy(inputMode, action);
        var vk = TryGetVirtualKey(action);
        return policy == RepeatPolicy.Disabled ? null : vk;
    }

    internal void SuppressNextClickForTests() => _suppressNextClick = true;

    internal void ResetTransientGestureStateForTests() => ResetTransientGestureState("test");

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        if (_inputModeResetInputService is not null)
            return;

        _inputModeResetInputService = App.Services?.GetService<InputService>();
        if (_inputModeResetInputService is not null)
            _inputModeResetInputService.InputModeGestureResetRequested += OnInputModeGestureResetRequested;
    }

    private void OnUnloaded(object sender, RoutedEventArgs e)
    {
        if (_inputModeResetInputService is not null)
            _inputModeResetInputService.InputModeGestureResetRequested -= OnInputModeGestureResetRequested;

        _inputModeResetInputService = null;
    }

    private void OnInputModeGestureResetRequested()
    {
        ResetTransientGestureState("input-mode-change");
    }

    protected override void OnMouseEnter(System.Windows.Input.MouseEventArgs e)
    {
        base.OnMouseEnter(e);

        // L2: English text)
        if (TtsOnHover && !string.IsNullOrWhiteSpace(DisplayLabel))
        {
            try
            {
                App.Services?.GetService<AccessibilityService>()?.SpeakLabel(DisplayLabel);
            }
            catch
            {
                // TTS English text
            }
        }

        System.Diagnostics.Debug.WriteLine($"[KeyButton] OnMouseEnter - DwellEnabled={DwellEnabled}, Slot={Slot?.Slot.Label}");
        if (!DwellEnabled) return;

        _dwellStart  = DateTime.UtcNow;
        _dwellTimer  = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(16) };
        _dwellTimer.Tick += DwellTick;
        _dwellTimer.Start();
        System.Diagnostics.Debug.WriteLine($"[KeyButton] Dwell timer started");
    }

    protected override void OnMouseLeave(System.Windows.Input.MouseEventArgs e)
    {
        base.OnMouseLeave(e);
        CancelDwell();
        if (!IsMouseCaptured)
            CancelRepeat();
    }

    protected override void OnPreviewMouseLeftButtonDown(System.Windows.Input.MouseButtonEventArgs e)
    {
        if (DwellEnabled)
        {
            e.Handled = false;
            return;
        }

        if (CanStartRepeat())
        {
            e.Handled = true;
            CancelRepeat();
            _isRepeating = false;
            // English text.
            _suppressNextClick = true;
            StartHoldGestureIfNeeded();
            ExecuteKeyPress();
            FinalizePressDispatch();

            _repeatDelayTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(Math.Max(1, KeyRepeatDelayMs)) };
            _repeatDelayTimer.Tick += RepeatDelayTick;
            _repeatDelayTimer.Start();
        }
        else
        {
            e.Handled = false;
        }
    }

    protected override void OnPreviewMouseLeftButtonUp(System.Windows.Input.MouseButtonEventArgs e)
    {
        if (DwellEnabled)
        {
            e.Handled = false;
            return;
        }

        if (CanStartRepeat())
        {
            e.Handled = true;
            CancelRepeat();
            ReleaseHeldKey("mouse-up");
        }
        else
        {
            e.Handled = false;
        }
    }

    protected override void OnClick()
    {
        if (_suppressNextClick)
        {
            _suppressNextClick = false;
            return;
        }

        base.OnClick();
    }

    protected override void OnLostMouseCapture(System.Windows.Input.MouseEventArgs e)
    {
        base.OnLostMouseCapture(e);
        CancelRepeat();
        ReleaseHeldKey("lost-mouse-capture");
    }

    protected override void OnMouseMove(System.Windows.Input.MouseEventArgs e)
    {
        base.OnMouseMove(e);

        // English text "English text"English text.
        if (!IsMouseCaptured)
            return;

        if (_activeHeldKey is null)
            return;

        if (e.LeftButton != MouseButtonState.Pressed || !IsPointerWithinButton(e))
        {
            CancelRepeat();
            ReleaseHeldKey("pointer-left-button");
        }
    }

    private void RepeatDelayTick(object? sender, EventArgs e)
    {
        if (_repeatDelayTimer is not null)
        {
            _repeatDelayTimer.Tick -= RepeatDelayTick;
            _repeatDelayTimer.Stop();
        }
        _repeatDelayTimer = null;

        var action = ResolveRepeatAction();
        var policy = GetRepeatPolicy(action);
        if (policy == RepeatPolicy.Disabled)
            return;

        // English text.
        if (ShouldCancelCompositionOnRepeatStart(policy, TryGetVirtualKey(action)))
        {
            App.Services?.GetService<AutoCompleteService>()?.CancelComposition();
        }

        System.Diagnostics.Debug.WriteLine($"[KeyButton] Repeat started");
        _isRepeating = true;

        _repeatTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(Math.Max(1, KeyRepeatIntervalMs)) };
        _repeatTimer.Tick += RepeatTick;
        _repeatTimer.Start();

        if (_activeHeldKey is not null)
            CaptureMouse();
    }

    private void RepeatTick(object? sender, EventArgs e)
    {
        if (_isRepeating)
        {
            if (_activeHeldKey is { } heldVk)
            {
                App.Services?.GetService<InputService>()?.PulseHeldKey(heldVk);
                return;
            }

            ExecuteKeyPress();
        }
    }

    /// <summary>
    /// English text 'English text' English text.
    /// </summary>
    private void ExecuteKeyPress()
    {
        if (Command?.CanExecute(CommandParameter) == true)
        {
            Command.Execute(CommandParameter);
        }
    }

    /// <summary>
    /// English text "English text"English text.
    /// </summary>
    private void StartHoldGestureIfNeeded()
    {
        var holdVk = TryGetHoldableVirtualKey();
        if (holdVk is null)
            return;

        if (App.Services?.GetService<InputService>() is not { } inputService)
            return;

        _isHoldPrimed = true;
        _primedHeldKey = holdVk.Value;
        inputService.ArmHeldKeyGesture(holdVk.Value);
    }

    /// <summary>
    /// Command English text.
    /// </summary>
    private void FinalizePressDispatch()
    {
        if (!_isHoldPrimed)
            return;

        _isHoldPrimed = false;

        var holdVk = _primedHeldKey;
        if (holdVk is null)
            return;

        if (App.Services?.GetService<InputService>() is not { } inputService)
            return;

        if (inputService.IsHeldKey(holdVk.Value))
        {
            _activeHeldKey = holdVk;
            _primedHeldKey = null;
            CaptureMouse();
            return;
        }

        inputService.CancelHeldKeyGesture(holdVk.Value);
        _primedHeldKey = null;
    }

    /// <summary>
    /// English text.
    /// </summary>
    private void ReleaseHeldKey(string reason)
    {
        if (App.Services?.GetService<InputService>() is not { } inputService)
        {
            _activeHeldKey = null;
            _isHoldPrimed = false;
            _primedHeldKey = null;
            if (IsMouseCaptured)
                ReleaseMouseCapture();
            return;
        }

        if (_isHoldPrimed)
        {
            var pendingVk = _primedHeldKey;
            _isHoldPrimed = false;
            if (pendingVk is not null)
                inputService.CancelHeldKeyGesture(pendingVk.Value);
            _primedHeldKey = null;
        }

        if (_activeHeldKey is { } vk)
        {
            inputService.EndHeldKey(vk);
            System.Diagnostics.Debug.WriteLine($"[KeyButton] Held key released ({reason}) - {vk}");
            _activeHeldKey = null;
        }

        if (IsMouseCaptured)
            ReleaseMouseCapture();
    }

    private void CancelRepeat()
    {
        _isRepeating = false;
        if (_repeatDelayTimer is not null)
        {
            _repeatDelayTimer.Tick -= RepeatDelayTick;
            _repeatDelayTimer.Stop();
        }
        _repeatDelayTimer = null;
        if (_repeatTimer is not null)
        {
            _repeatTimer.Tick -= RepeatTick;
            _repeatTimer.Stop();
        }
        _repeatTimer = null;
    }

    /// <summary>
    /// English text.
    /// </summary>
    private void ResetTransientGestureState(string reason)
    {
        _suppressNextClick = false;
        CancelRepeat();
        ReleaseHeldKey(reason);
    }

    private bool CanStartRepeat()
    {
        if (!KeyRepeatEnabled)
            return false;

        return GetRepeatPolicy() != RepeatPolicy.Disabled;
    }

    /// <summary>
    /// English text.
    /// </summary>
    private static bool IsUnicodeHeldRepeatKey(VirtualKeyCode vk) => vk is
        // English text
        VirtualKeyCode.VK_BACK or VirtualKeyCode.VK_DELETE or
        // English text
        VirtualKeyCode.VK_LEFT or VirtualKeyCode.VK_RIGHT or
        VirtualKeyCode.VK_UP or VirtualKeyCode.VK_DOWN or
        // English text
        VirtualKeyCode.VK_HOME or VirtualKeyCode.VK_END or
        VirtualKeyCode.VK_PRIOR or VirtualKeyCode.VK_NEXT;

    /// <summary>
    /// English text.
    /// </summary>
    private KeyAction? ResolveRepeatAction()
    {
        if (App.Services?.GetService<InputService>() is { IsFunctionLayerActive: true } inputService
            && Slot?.Slot.Action is not ToggleFunctionLayerAction
            && Slot?.Slot.FunctionAction is not null)
        {
            return Slot.Slot.FunctionAction;
        }

        return Slot?.Slot.Action;
    }

    /// <summary>
    /// English text.
    /// </summary>
    private VirtualKeyCode? TryGetHoldableVirtualKey()
    {
        var action = ResolveRepeatAction();
        return GetHoldableVirtualKey(GetRepeatPolicy(action), action);
    }

    private RepeatPolicy GetRepeatPolicy()
        => GetRepeatPolicy(ResolveRepeatAction());

    private RepeatPolicy GetRepeatPolicy(KeyAction? action)
    {
        if (App.Services?.GetService<InputService>() is not { } inputService)
            return RepeatPolicy.Disabled;

        return ClassifyRepeatPolicy(inputService.Mode, action);
    }

    private static RepeatPolicy ClassifyRepeatPolicy(LArtKey.Services.InputMode inputMode, KeyAction? action)
    {
        var vk = TryGetVirtualKey(action);
        if (vk is null)
            return RepeatPolicy.Disabled;

        if (inputMode == LArtKey.Services.InputMode.Unicode)
            return IsUnicodeHeldRepeatKey(vk.Value) ? RepeatPolicy.HeldVirtualKey : RepeatPolicy.Disabled;

        return RepeatPolicy.LegacyRepeat;
    }

    private static VirtualKeyCode? GetHoldableVirtualKey(RepeatPolicy policy, KeyAction? action)
    {
        var vk = TryGetVirtualKey(action);
        if (vk is null)
            return null;

        return policy == RepeatPolicy.Disabled ? null : vk;
    }

    private static bool ShouldCancelCompositionOnRepeatStart(RepeatPolicy policy, VirtualKeyCode? vk)
        => policy == RepeatPolicy.HeldVirtualKey && vk == VirtualKeyCode.VK_BACK;

    private static VirtualKeyCode? TryGetVirtualKey(KeyAction? action)
    {
        if (action is not SendKeyAction { Vk: var vkText })
            return null;

        return Enum.TryParse<VirtualKeyCode>(vkText, ignoreCase: true, out var vk)
            ? vk
            : null;
    }

    private void DwellTick(object? sender, EventArgs e)
    {
        var elapsed = (DateTime.UtcNow - _dwellStart).TotalMilliseconds;
        DwellProgress = elapsed / DwellTime; // 0.0 ~ 1.0

        if (elapsed >= 100) // 100msEnglish text
            System.Diagnostics.Debug.WriteLine($"[KeyButton] Dwell progress: {DwellProgress:P0}");

        if (elapsed >= DwellTime)
        {
            System.Diagnostics.Debug.WriteLine($"[KeyButton] DWELL CLICK - Slot={Slot?.Slot.Label}");
            CancelDwell();
            
            // Command English text)
            if (Command?.CanExecute(CommandParameter) == true)
            {
                System.Diagnostics.Debug.WriteLine($"[KeyButton] Executing command...");
                Command.Execute(CommandParameter);
            }
        }
    }

    private void CancelDwell()
    {
        _dwellTimer?.Stop();
        _dwellTimer  = null;
        DwellProgress = 0;
        CancelRepeat();
        ReleaseHeldKey("dwell-cancel");
    }

    /// <summary>
    /// English text.
    /// </summary>
    private bool IsPointerWithinButton(System.Windows.Input.MouseEventArgs e)
    {
        var point = e.GetPosition(this);
        return point.X >= 0
            && point.Y >= 0
            && point.X <= ActualWidth
            && point.Y <= ActualHeight;
    }

    // ── English text ────────────────────────────────────────────────────────────

    private static void OnSlotChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not KeyButton kb || e.NewValue is not KeySlotVm slot) return;
        kb.UpdateSize();
        kb.UpdateLabel();
        // English text: style_keyEnglish text.
        ToolTipService.SetToolTip(kb, null);
    }

    private static void OnShowUpperCaseChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is KeyButton kb)
            kb.UpdateLabel();
    }

    private void UpdateLabel()
    {
        if (Slot is null) return;
        Slot.RefreshDisplay();
        // XAML English text
        SetCurrentValue(DisplayLabelProperty, Slot.DisplayLabel);
        SetCurrentValue(SubLabelProperty, Slot.GetSubLabel(ShowUpperCase));
    }

    private static void OnDisplayLabelChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is KeyButton kb)
            kb.Content = e.NewValue as string;
    }

    private static void OnIsDimmedChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is KeyButton kb)
            kb.IsEnabled = !(bool)e.NewValue;
    }

    private static void OnKeyUnitChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is KeyButton kb)
            kb.UpdateSize();
    }

    private static void OnKeyboardA11yNavigationEnabledChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is KeyButton kb)
        {
            var enabled = (bool)e.NewValue;
            kb.Focusable = enabled;
            kb.IsTabStop = enabled;
        }
    }

    /// <summary>
    /// English text.
    /// </summary>
    private void UpdateSize()
    {
        if (Slot is null) return;
        Width  = Slot.Width  * KeyUnit;
        Height = Slot.Height * KeyUnit;
    }
}
