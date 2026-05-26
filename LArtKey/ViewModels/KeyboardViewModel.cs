using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Threading;
using LArtKey.Models;
using LArtKey.Services;
using LArtKey.Services.InputLanguage;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace LArtKey.ViewModels;

/// <summary>
/// [English text] English text.
/// </summary>
public partial class KeyRowVm : ObservableObject
{
    public KeyRowVm(int sharedRowIndex, double heightRatio, IReadOnlyList<KeySlotVm> keys)
    {
        SharedRowIndex = sharedRowIndex;
        HeightRatio = heightRatio;
        Keys = keys;
    }

    public int SharedRowIndex { get; }
    public double HeightRatio { get; }
    public IReadOnlyList<KeySlotVm> Keys { get; }

    [ObservableProperty]
    private double pixelHeight;
}

/// <summary>
/// [English text] English text.
/// </summary>
public record KeyColumnVm(double Gap, IReadOnlyList<KeyRowVm> Rows);

/// <summary>
/// [English text] English text.
/// </summary>
public class KeySlotVm(KeySlot slot, AutoCompleteService autoComplete) : ObservableObject
{
    public KeySlot Slot { get; } = slot;
    // English text.
    public string StyleKey => Slot.StyleKey;
    public bool HasSoftAccentStyle => string.Equals(StyleKey, EditableKeySlotVm.SoftAccentStyleKey, StringComparison.Ordinal);
    public double Width  { get; } = slot.Width;
    public double Height { get; } = slot.Height;

    private bool _isSticky;
    private bool _isLocked;
    private bool _isA11yFocused;
    private FunctionLayerState _functionLayerState;
    private string? _autoCompleteComposeStateLabel;
    private bool _showShiftLabels;
    private bool _isCapsLockOn;
    public bool IsSticky { get => _isSticky; set => SetProperty(ref _isSticky, value); }
    public bool IsLocked { get => _isLocked; set => SetProperty(ref _isLocked, value); }
    public bool IsA11yFocused { get => _isA11yFocused; set => SetProperty(ref _isA11yFocused, value); }
    public bool IsFunctionOneShot => _functionLayerState == FunctionLayerState.OneShot;
    public bool IsFunctionLocked => _functionLayerState == FunctionLayerState.Locked;
    public bool HasFunctionLayerAccent => IsFunctionLayerToggle || Slot.FunctionAction is not null;

    public VirtualKeyCode? StickyVk =>
        Slot.Action is ToggleStickyAction ta &&
        Enum.TryParse<VirtualKeyCode>(ta.Vk, ignoreCase: true, out var vk)
            ? vk : null;

    public bool IsInputModeToggle => Slot.Action is ToggleInputModeAction;
    public bool IsFunctionLayerToggle => Slot.Action is ToggleFunctionLayerAction;

    private InputSubmode _activeSubmode = InputSubmode.QuietEnglish;
    public InputSubmode ActiveSubmode
    {
        get => _activeSubmode;
        set
        {
            if (SetProperty(ref _activeSubmode, value))
            {
                RefreshDisplay();
            }
        }
    }

    /// <summary>
    /// English text.
    /// </summary>
    public string GetLabel(InputSubmode submode)
    {
        if (IsInputModeToggle)
            return _autoCompleteComposeStateLabel ?? "English text";

        if (submode == InputSubmode.QuietEnglish && Slot.EnglishLabel is { Length: > 0 } eng)
        {
            string baseLabel = ShouldUppercaseEnglishLabel()
                ? (Slot.EnglishShiftLabel ?? eng.ToUpperInvariant())
                : eng;
            return baseLabel;
        }

        return _showShiftLabels && Slot.ShiftLabel is { Length: > 0 } s
            ? s
            : Slot.Label;
    }

    public bool GetIsDimmed(InputSubmode submode) => false;

    public string DisplayLabel { get; private set; } = "";
    public string SubLabelText { get; private set; } = "";
    public bool IsDimmed { get; private set; }

    public void RefreshDisplay()
    {
        DisplayLabel = GetLabel(_activeSubmode);
        SubLabelText = GetSubLabel();
        ApplyFunctionLayerDisplay();
        IsDimmed = GetIsDimmed(_activeSubmode);
        OnPropertyChanged(nameof(DisplayLabel));
        OnPropertyChanged(nameof(SubLabelText));
        OnPropertyChanged(nameof(IsDimmed));
        OnPropertyChanged(nameof(AccessibleName));
        OnPropertyChanged(nameof(AccessibleHelp));
        OnPropertyChanged(nameof(AutomationId));
    }

    private void ApplyFunctionLayerDisplay()
    {
        if (_functionLayerState == FunctionLayerState.Inactive || IsFunctionLayerToggle || IsInputModeToggle)
            return;

        if (_showShiftLabels && Slot.FunctionShiftLabel is { Length: > 0 } fnShift)
        {
            DisplayLabel = fnShift;
        }
        else if (Slot.FunctionLabel is { Length: > 0 } fnLabel)
        {
            DisplayLabel = fnLabel;
        }

        if (Slot.FunctionEnglishLabel is { Length: > 0 } fnSubEnglish)
        {
            SubLabelText = ShouldUppercaseEnglishLabel()
                ? (Slot.FunctionEnglishShiftLabel ?? fnSubEnglish.ToUpperInvariant())
                : fnSubEnglish;
        }
        else if (Slot.FunctionLabel is { Length: > 0 } fnSubLabel)
        {
            SubLabelText = _showShiftLabels && Slot.FunctionShiftLabel is { Length: > 0 } fnSubShift
                ? fnSubShift
                : fnSubLabel;
        }
    }

    public void SetModifierDisplayState(bool showShiftLabels, bool isCapsLockOn)
    {
        if (_showShiftLabels == showShiftLabels && _isCapsLockOn == isCapsLockOn)
            return;

        _showShiftLabels = showShiftLabels;
        _isCapsLockOn = isCapsLockOn;
        RefreshDisplay();
    }

    public void SetComposeStateLabel(string? label)
    {
        if (_autoCompleteComposeStateLabel != label)
        {
            _autoCompleteComposeStateLabel = label;
            if (IsInputModeToggle)
                RefreshDisplay();
        }
    }

    public void SetFunctionLayerState(FunctionLayerState state)
    {
        if (_functionLayerState == state)
            return;

        _functionLayerState = state;
        OnPropertyChanged(nameof(IsFunctionOneShot));
        OnPropertyChanged(nameof(IsFunctionLocked));
        RefreshDisplay();
    }

    public string GetSubLabel()
    {
        if (IsInputModeToggle)
            return "";

        if (Slot.EnglishLabel is { Length: > 0 } eng)
        {
            if (_activeSubmode == InputSubmode.QuietEnglish)
            {
                return ShouldUppercaseEnglishLabel()
                    ? (Slot.EnglishShiftLabel ?? eng.ToUpperInvariant())
                    : eng;
            }
            else if (_activeSubmode == InputSubmode.QuietEnglish)
            {
                return _showShiftLabels && Slot.ShiftLabel is { Length: > 0 } s
                    ? s
                    : Slot.Label;
            }
        }
        return "";
    }

    // English text.
    public string GetSubLabel(bool _) => GetSubLabel();

    /// <summary>
    /// Caps LockEnglish text.
    /// </summary>
    private bool ShouldUppercaseEnglishLabel()
    {
        return _showShiftLabels || (_isCapsLockOn && HasAlphabeticEnglishLabel());
    }

    /// <summary>
    /// English text.
    /// </summary>
    private bool HasAlphabeticEnglishLabel()
    {
        string? label = Slot.EnglishLabel;
        if (string.IsNullOrWhiteSpace(label) || label.Length != 1)
            return false;

        return char.IsLetter(label[0]) && label[0] < 128;
    }

    // ── Accessibility ────────────────────────────────────────────────────────

    public string AccessibleName => ComputeAccessibleName();
    public string AccessibleHelp => ComputeAccessibleHelp();
    public string AutomationId   => (_functionLayerState != FunctionLayerState.Inactive && Slot.FunctionAction is not null && !IsFunctionLayerToggle
        ? Slot.FunctionAction
        : Slot.Action)?.GetType().Name ?? "UnknownAction";

    private string ComputeAccessibleName()
    {
        if (IsFunctionLayerToggle)
        {
            return _functionLayerState switch
            {
                FunctionLayerState.OneShot => "Fn English text",
                FunctionLayerState.Locked => "Fn English text",
                _ => "Fn English text"
            };
        }

        if (_functionLayerState != FunctionLayerState.Inactive && !string.IsNullOrWhiteSpace(DisplayLabel))
            return $"{DisplayLabel} English text";

        if (Slot.Action is ToggleInputModeAction)
        {
            return "English input mode";
        }

        var label = _showShiftLabels && Slot.ShiftLabel is { Length: > 0 } shifted
            ? shifted
            : Slot.Label;
        if (!string.IsNullOrWhiteSpace(label))
        {
            return $"{label} key";
        }
        return ResolveFunctionKeyName(Slot);
    }

    private string ComputeAccessibleHelp()
    {
        if (IsFunctionLayerToggle)
        {
            return _functionLayerState switch
            {
                FunctionLayerState.OneShot => "Fn English text",
                FunctionLayerState.Locked => "Fn English text",
                _ => "Fn English text"
            };
        }

        if (IsSticky) return "English text";
        if (IsLocked) return "English text";
        return "";
    }

    private static string ResolveFunctionKeyName(KeySlot slot)
    {
        if (slot.Action is SendKeyAction { Vk: var vkStr }
            && Enum.TryParse<VirtualKeyCode>(vkStr, ignoreCase: true, out var vk))
        {
            return vk switch
            {
                VirtualKeyCode.VK_SHIFT   => "English text",
                VirtualKeyCode.VK_LSHIFT  => "English text",
                VirtualKeyCode.VK_RSHIFT  => "English text",
                VirtualKeyCode.VK_CONTROL => "English text",
                VirtualKeyCode.VK_LCONTROL => "English text",
                VirtualKeyCode.VK_RCONTROL => "English text",
                VirtualKeyCode.VK_MENU    => "English text",
                VirtualKeyCode.VK_LMENU   => "English text",
                VirtualKeyCode.VK_RMENU   => "English text",
                VirtualKeyCode.VK_RETURN  => "English text",
                VirtualKeyCode.VK_SPACE   => "English text",
                VirtualKeyCode.VK_TAB     => "English text",
                VirtualKeyCode.VK_BACK    => "English text",
                VirtualKeyCode.VK_DELETE  => "English text",
                VirtualKeyCode.VK_INSERT  => "English text",
                VirtualKeyCode.VK_HOME    => "English text",
                VirtualKeyCode.VK_END     => "English text",
                VirtualKeyCode.VK_LEFT    => "English text",
                VirtualKeyCode.VK_RIGHT   => "English text",
                VirtualKeyCode.VK_UP      => "English text",
                VirtualKeyCode.VK_DOWN    => "English text",
                VirtualKeyCode.VK_PRIOR   => "English text",
                VirtualKeyCode.VK_NEXT    => "English text",
                VirtualKeyCode.VK_ESCAPE  => "English text",
                VirtualKeyCode.VK_CAPITAL => "English text",
                VirtualKeyCode.VK_F1 => "F1 English text", VirtualKeyCode.VK_F2 => "F2 English text",
                VirtualKeyCode.VK_F3 => "F3 English text", VirtualKeyCode.VK_F4 => "F4 English text",
                VirtualKeyCode.VK_F5 => "F5 English text", VirtualKeyCode.VK_F6 => "F6 English text",
                VirtualKeyCode.VK_F7 => "F7 English text", VirtualKeyCode.VK_F8 => "F8 English text",
                VirtualKeyCode.VK_F9 => "F9 English text", VirtualKeyCode.VK_F10 => "F10 English text",
                VirtualKeyCode.VK_F11 => "F11 English text", VirtualKeyCode.VK_F12 => "F12 English text",
                VirtualKeyCode.VK_HANGUL => "English text",
                VirtualKeyCode.VK_HANJA  => "English text",
                _ => slot.Label,
            };
        }

        if (slot.Action is ToggleStickyAction { Vk: var stickyVk })
        {
            return $"{stickyVk} English text";
        }

        if (slot.Action is SwitchLayoutAction { Name: var layoutName })
        {
            return $"{layoutName} English text";
        }

        return slot.Label;
    }
}

/// <summary>
/// [English text] LArtKeyEnglish text.
/// [English text] English text.
/// </summary>
public partial class KeyboardViewModel : ObservableObject
{
    private readonly InputService _inputService;
    private readonly SoundService _soundService;
    private readonly AutoCompleteService _autoComplete;
    private readonly ConfigService _configService;
    private readonly LiveRegionService _liveRegion;
    private readonly AccessibilityService _accessibilityService;
    private readonly SuggestionBarViewModel _suggestionBar;
    private readonly List<KeySlotVm> _a11yNavigableSlots = [];
    private int _a11yFocusIndex = -1;
    private A11yFocusOwner _a11yFocusOwner = A11yFocusOwner.None;

    // [English text] English text.
    public A11yFocusOwner A11yFocusOwner
    {
        get => _a11yFocusOwner;
        private set => SetProperty(ref _a11yFocusOwner, value);
    }

    private readonly DispatcherTimer _capsLockTimer;

    // L3: English text
    private DispatcherTimer? _scanTimer;
    private readonly List<ScanTargetVm> _scanTargets = [];
    private int _scanFocusIndex = -1;
    private bool _isRowSelectionPhase = true;
    private int _selectedRowIndex = -1;

    [ObservableProperty]
    private ObservableCollection<KeyColumnVm> columns = [];

    // English text.)
    [ObservableProperty] private double maxRowUnits = 15.0; // English text
    [ObservableProperty] private double maxRowCount = 14.0; // English text
    [ObservableProperty] private double rowCount    = 5.0;  // English text)
    public double KeyRowHeight => KeyUnit + 4.0;
    public double TotalRowUnits => Math.Max(1.0, GetSharedRowHeightMap().Values.Sum());

    /// English text.
    public double SuggestionChipHeight
    {
        get
        {
            double scaledFontSize = 13.0 * _configService.Current.KeyFontScalePercent / 100.0;
            double fontAwareMinHeight = scaledFontSize + 10.0;
            return Math.Max(fontAwareMinHeight, KeyUnit * 0.62);
        }
    }

    /// English text 100% English text.
    public double SuggestionBarHeight => SuggestionChipHeight + 6.0;

    partial void OnColumnsChanged(ObservableCollection<KeyColumnVm> value)
    {
        RecalculateLayoutMetrics();
    }

    partial void OnKeyUnitChanged(double value)
    {
        UpdateRowPixelHeights();
        OnPropertyChanged(nameof(KeyRowHeight));
        OnPropertyChanged(nameof(TotalRowUnits));
        OnPropertyChanged(nameof(SuggestionChipHeight));
        OnPropertyChanged(nameof(SuggestionBarHeight));
    }

    /// English text.
    /// - MaxRowUnits = Σ(English text) + Σ(English text)
    /// - MaxRowCount = Σ(English text)
    /// - RowCount    = max(English text)
    private void RecalculateLayoutMetrics()
    {
        if (Columns.Count == 0 || Columns.All(c => c.Rows.Count == 0))
        {
            MaxRowUnits = 15.0;
            MaxRowCount = 14.0;
            RowCount    = 5.0;
            UpdateRowPixelHeights();
            return;
        }

        double totalUnits = 0;
        double totalKeys  = 0;
        double maxRows    = 0;
        bool first = true;

        foreach (var col in Columns)
        {
            if (!first) totalUnits += col.Gap;
            first = false;

            if (col.Rows.Count == 0) continue;

            double colUnits = col.Rows.Max(r =>
                r.Keys.Sum(k => k.Width) + r.Keys.Sum(k => k.Slot.GapBefore));
            int    colKeys  = col.Rows.Max(r => r.Keys.Count);

            totalUnits += colUnits;
            totalKeys  += colKeys;
            maxRows     = Math.Max(maxRows, col.Rows.Count);
        }

        MaxRowUnits = Math.Max(1, totalUnits);
        MaxRowCount = Math.Max(1, totalKeys);
        RowCount    = Math.Max(1, maxRows);
        UpdateRowPixelHeights();
        OnPropertyChanged(nameof(TotalRowUnits));
    }

    [ObservableProperty]
    private bool showUpperCase;

    [ObservableProperty]
    private bool showElevatedWarning;

    /// <summary>
    /// English text.
    /// </summary>
    [ObservableProperty]
    private double keyUnit = 48.0;

    [ObservableProperty]
    private string modeAnnouncement = "";

    public KeyboardViewModel(InputService inputService, SoundService soundService,
        AutoCompleteService autoComplete, ConfigService configService, LiveRegionService liveRegion,
        AccessibilityService accessibilityService, SuggestionBarViewModel suggestionBar)
    {
        _inputService = inputService;
        _soundService = soundService;
        _autoComplete = autoComplete;
        _configService = configService;
        _liveRegion = liveRegion;
        _accessibilityService = accessibilityService;
        _suggestionBar = suggestionBar;
        _inputService.StickyStateChanged += UpdateModifierState;
        _inputService.ElevatedAppDetected += OnElevatedAppDetected;
        _configService.ConfigChanged += OnConfigChanged;
        _suggestionBar.ScanTargetsChanged += OnSuggestionScanTargetsChanged;

        _autoComplete.SubmodeChanged += OnSubmodeChanged;

        _liveRegion.Announced += msg =>
        {
            ModeAnnouncement = msg;
            RaiseLiveRegionChanged();
        };

        _capsLockTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(100) };
        _capsLockTimer.Tick += OnTimerTick;
        _capsLockTimer.Start();
    }

    public event Action? LiveRegionChanged;

    private void RaiseLiveRegionChanged()
    {
        LiveRegionChanged?.Invoke();
    }

    /// <summary>
    /// English text.
    /// </summary>
    public void LoadLayout(LayoutConfig layout)
    {
        if (layout.Columns is { Count: > 0 })
        {
            var sharedRowHeights = BuildSharedRowHeights(layout);
            Columns = new ObservableCollection<KeyColumnVm>(
                layout.Columns.Select(col => new KeyColumnVm(
                    col.Gap,
                    col.Rows?.Select((r, rowIndex) => new KeyRowVm(
                        rowIndex,
                        sharedRowHeights.TryGetValue(rowIndex, out var rowHeight) ? rowHeight : 1.0,
                        r.Keys.Select(k => new KeySlotVm(k, _autoComplete)).ToList()
                    )).ToList() ?? []
                ))
            );
        }
        else
        {
            Columns = [];
        }

        UpdateRowPixelHeights();

        _autoComplete.ResetState();
        _inputService.ResetFunctionLayer();
        RefreshKeyLabels(_autoComplete.ActiveSubmode);
        ResetA11yNavigationState();

        // L3: English text.
        if (_configService.Current.SwitchScanEnabled)
            StartScan();
    }

    private void UpdateRowPixelHeights()
    {
        var sharedRowHeights = GetSharedRowHeightMap();
        foreach (var row in Columns.SelectMany(column => column.Rows))
        {
            var sharedHeight = sharedRowHeights.TryGetValue(row.SharedRowIndex, out var heightRatio)
                ? heightRatio
                : row.HeightRatio;
            row.PixelHeight = sharedHeight * KeyUnit + 4.0;
        }
    }

    private Dictionary<int, double> GetSharedRowHeightMap() =>
        Columns.SelectMany(column => column.Rows)
            .GroupBy(row => row.SharedRowIndex)
            .ToDictionary(group => group.Key, group => group.Min(row => row.HeightRatio));

    private static Dictionary<int, double> BuildSharedRowHeights(LayoutConfig layout)
    {
        var result = new Dictionary<int, double>();
        if (layout.Columns is null)
            return result;

        foreach (var column in layout.Columns)
        {
            if (column.Rows is null)
                continue;

            for (int rowIndex = 0; rowIndex < column.Rows.Count; rowIndex++)
            {
                var row = column.Rows[rowIndex];
                var heightRatio = row.Keys.Count > 0
                    ? NormalizeHeight(row.Keys[0].Height)
                    : EditableKeySlotVm.DefaultHeightRatio;

                if (!result.TryGetValue(rowIndex, out var existing)
                    || heightRatio < existing)
                {
                    result[rowIndex] = heightRatio;
                }
            }
        }

        return result;
    }

    private static double NormalizeHeight(double heightRatio) =>
        Math.Abs(heightRatio - EditableKeySlotVm.CompactHeightRatio) < 0.001
            ? EditableKeySlotVm.CompactHeightRatio
            : EditableKeySlotVm.DefaultHeightRatio;

    public event Action? KeyTapped;

    /// <summary>
    /// English text.
    /// </summary>
    [RelayCommand]
    private void KeyPressed(KeySlot slot)
    {
        _soundService.Play();

        KeyAction? effectiveAction = ResolveEffectiveAction(slot);
        bool isFunctionToggleKey = slot.Action is ToggleFunctionLayerAction;
        bool shouldConsumeFunctionLayer = false;

        if (effectiveAction is ToggleInputModeAction)
        {
            _autoComplete.ToggleInputMode();
            UpdateModifierState();
            KeyTapped?.Invoke();
            return;
        }

        if (_inputService.IsFunctionLayerActive
            && !isFunctionToggleKey
            && slot.FunctionAction is not null)
        {
            if (effectiveAction is not null)
                _inputService.HandleAction(effectiveAction);

            FinalizeFunctionLayerKeypress(slot, effectiveAction is not null);
            return;
        }

        if (_inputService.IsForegroundOwnWindow())
        {
            _autoComplete.CancelComposition();

            // English text.
            if (FocusTracker.LastFocused is { IsVisible: true } tb && !tb.IsKeyboardFocused)
                System.Windows.Input.Keyboard.Focus(tb);

            if (IsSeparatorKey(slot))
            {
                if (effectiveAction is not null)
                {
                    _inputService.HandleAction(effectiveAction);
                    shouldConsumeFunctionLayer = !isFunctionToggleKey;
                }
            }
            else if (effectiveAction is not null)
            {
                _inputService.HandleAction(effectiveAction);
                shouldConsumeFunctionLayer = !isFunctionToggleKey;
            }

            FinalizeFunctionLayerKeypress(slot, shouldConsumeFunctionLayer);
            return;
        }

        if (IsSeparatorKey(slot))
        {
            _autoComplete.OnSeparator();
            if (effectiveAction is not null)
            {
                _inputService.HandleAction(effectiveAction);
                shouldConsumeFunctionLayer = !isFunctionToggleKey;
            }
            FinalizeFunctionLayerKeypress(slot, shouldConsumeFunctionLayer);
            return;
        }

        var ctx = new KeyContext(
            ShowUpperCase,
            _inputService.HasActiveModifiers,
            _inputService.HasActiveModifiersExcludingShift,
            _inputService.Mode,
            _inputService.TrackedOnScreenLength);

        bool handled = _autoComplete.OnKey(slot, ctx);
        if (!handled && effectiveAction is not null)
        {
            _inputService.HandleAction(effectiveAction);
            handled = true;
        }

        shouldConsumeFunctionLayer = handled && !isFunctionToggleKey;
        FinalizeFunctionLayerKeypress(slot, shouldConsumeFunctionLayer);

    }

    private KeyAction? ResolveEffectiveAction(KeySlot slot)
    {
        if (_inputService.IsFunctionLayerActive
            && slot.Action is not ToggleFunctionLayerAction
            && slot.FunctionAction is not null)
        {
            return slot.FunctionAction;
        }

        return slot.Action;
    }

    private void FinalizeFunctionLayerKeypress(KeySlot slot, bool consumeFunctionLayer)
    {
        if (consumeFunctionLayer)
            _inputService.ConsumeFunctionLayerAfterAction();

        UpdateModifierState();
        KeyTapped?.Invoke();

        var slotVm = EnumerateSlotVms().FirstOrDefault(vm => vm.Slot == slot);
        _accessibilityService.SpeakLabel(slotVm?.DisplayLabel ?? slot.Label);
    }

    public void MoveA11yFocus(bool reverse)
    {
        if (!_configService.Current.KeyboardA11yNavigationEnabled)
            return;

        // English text.
        if (A11yFocusOwner == A11yFocusOwner.SwitchScan)
            StopScan();
        A11yFocusOwner = A11yFocusOwner.KeyboardNavigation;

        RebuildA11yNavigableSlots();
        if (_a11yNavigableSlots.Count == 0)
            return;

        int count = _a11yNavigableSlots.Count;
        int nextIndex;

        if (_a11yFocusIndex < 0 || _a11yFocusIndex >= count)
        {
            nextIndex = reverse ? count - 1 : 0;
        }
        else
        {
            nextIndex = reverse
                ? (_a11yFocusIndex - 1 + count) % count
                : (_a11yFocusIndex + 1) % count;
        }

        SetA11yFocus(nextIndex);

        // English text.
        if (_configService.Current.KeyboardA11yAnnounceFocus)
            AnnounceFocusedTarget();
    }

    public void ActivateA11yFocused()
    {
        if (!_configService.Current.KeyboardA11yNavigationEnabled)
            return;

        RebuildA11yNavigableSlots();
        if (_a11yNavigableSlots.Count == 0)
            return;

        if (_a11yFocusIndex < 0 || _a11yFocusIndex >= _a11yNavigableSlots.Count)
        {
            SetA11yFocus(0);
            return;
        }

        var focused = _a11yNavigableSlots[_a11yFocusIndex];
        KeyPressed(focused.Slot);
    }

    /// <summary>
    /// English text)
    /// </summary>
    public void ClearA11yFocus()
    {
        ResetA11yNavigationState();
    }

    private void RebuildA11yNavigableSlots()
    {
        _a11yNavigableSlots.Clear();
        foreach (var vm in EnumerateSlotVms())
            _a11yNavigableSlots.Add(vm);
    }

    private IEnumerable<KeySlotVm> EnumerateSlotVms()
    {
        foreach (var col in Columns)
        foreach (var row in col.Rows)
        foreach (var slotVm in row.Keys)
            yield return slotVm;
    }

    private void SetA11yFocus(int nextIndex)
    {
        if (_a11yFocusIndex >= 0 && _a11yFocusIndex < _a11yNavigableSlots.Count)
            _a11yNavigableSlots[_a11yFocusIndex].IsA11yFocused = false;

        _a11yFocusIndex = nextIndex;

        if (_a11yFocusIndex >= 0 && _a11yFocusIndex < _a11yNavigableSlots.Count)
            _a11yNavigableSlots[_a11yFocusIndex].IsA11yFocused = true;
    }

    private void ResetA11yNavigationState()
    {
        foreach (var vm in EnumerateSlotVms())
            vm.IsA11yFocused = false;

        _a11yNavigableSlots.Clear();
        _a11yFocusIndex = -1;
        A11yFocusOwner = A11yFocusOwner.None;
    }

    private void OnConfigChanged(string? propertyName)
    {
        if (propertyName is null or nameof(AppConfig.KeyFontScalePercent))
        {
            OnPropertyChanged(nameof(SuggestionChipHeight));
            OnPropertyChanged(nameof(SuggestionBarHeight));
        }

        if (propertyName is null or nameof(AppConfig.KeyboardA11yNavigationEnabled))
        {
            if (!_configService.Current.KeyboardA11yNavigationEnabled)
                ResetA11yNavigationState();
        }

        // L3: English text
        if (propertyName is null
            or nameof(AppConfig.SwitchScanEnabled)
            or nameof(AppConfig.SwitchScanIntervalMs)
            or nameof(AppConfig.SwitchScanMode)
            or nameof(AppConfig.SwitchScanInitialDelayMs)
            or nameof(AppConfig.SwitchScanSelectPauseMs)
            or nameof(AppConfig.SwitchScanCyclesBeforePause)
            or nameof(AppConfig.SwitchScanWrapEnabled)
            or nameof(AppConfig.SwitchScanIncludeSuggestions)
            or nameof(AppConfig.SwitchScanSuggestionPriority))
        {
            if (_configService.Current.SwitchScanEnabled)
                StartScan();
            else
                StopScan();
        }
    }

    // ── L3: English text ───────────────────────────────────────────

    /// <summary>
    /// English text.
    /// </summary>
    public void StartScan()
    {
        StopScan();
        A11yFocusOwner = A11yFocusOwner.SwitchScan;
        RebuildScanTargets();
        if (_scanTargets.Count == 0)
            return;

        int interval = Math.Clamp(_configService.Current.SwitchScanIntervalMs, 200, 3000);
        _scanTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(interval) };
        _scanTimer.Tick += ScanTick;
        if (_configService.Current.SwitchScanMode != SwitchScanMode.Manual)
            _scanTimer.Start();
        _isRowSelectionPhase = true;
        _selectedRowIndex = -1;

        // English text
        SetScanFocus(0);
        AnnounceScanMove(_scanTargets[0].AccessibleName);
    }

    /// <summary>
    /// English text.
    /// </summary>
    public void StopScan()
    {
        _scanTimer?.Stop();
        _scanTimer = null;

        foreach (var vm in EnumerateSlotVms())
            vm.IsA11yFocused = false;
        foreach (var vm in _suggestionBar.ScanTargets)
            vm.SetScanFocused(false);
        _scanTargets.Clear();
        _scanFocusIndex = -1;
        _a11yFocusIndex = -1;
        if (A11yFocusOwner == A11yFocusOwner.SwitchScan)
            A11yFocusOwner = A11yFocusOwner.None;
    }

    private void ScanTick(object? sender, EventArgs e)
    {
        if (_scanTargets.Count == 0)
            return;
        AdvanceScan();
    }

    /// <summary>
    /// English text "English text"English text.
    /// </summary>
    public void SelectScanTarget()
    {
        if (_scanFocusIndex >= 0 && _scanFocusIndex < _scanTargets.Count)
        {
            var target = _scanTargets[_scanFocusIndex];

            if (_configService.Current.SwitchScanMode == SwitchScanMode.RowColumn && _isRowSelectionPhase)
            {
                _selectedRowIndex = _scanFocusIndex;
                _isRowSelectionPhase = false;
                RebuildScanTargets();
                if (_scanTargets.Count > 0)
                {
                    SetScanFocus(0);
                    AnnounceScanMove(_scanTargets[0].AccessibleName);
                }
                return;
            }

            AnnounceScanSelection(target.AccessibleName);
            target.Activate();
            if (_configService.Current.SwitchScanMode == SwitchScanMode.RowColumn)
            {
                _isRowSelectionPhase = true;
                _selectedRowIndex = -1;
                RebuildScanTargets();
                if (_scanTargets.Count > 0)
                    SetScanFocus(0);
            }
        }
    }

    /// <summary>
    /// 2English text "English text" English text.
    /// </summary>
    public void AdvanceScan()
    {
        if (_scanTargets.Count == 0)
            return;
        int next = GetNextScanIndex(reverse: false);
        SetScanFocus(next);
        var current = _scanTargets[next];
        AnnounceScanMove(current.AccessibleName);
    }

    /// <summary>
    /// English text.
    /// </summary>
    public void ReverseScan()
    {
        if (_scanTargets.Count == 0)
            return;
        int next = GetNextScanIndex(reverse: true);
        SetScanFocus(next);
        var current = _scanTargets[next];
        AnnounceScanMove(current.AccessibleName);
    }

    /// <summary>
    /// English text.
    /// </summary>
    public void ToggleScanPaused()
    {
        if (_scanTimer is null) return;
        if (_scanTimer.IsEnabled) _scanTimer.Stop();
        else if (_configService.Current.SwitchScanMode != SwitchScanMode.Manual) _scanTimer.Start();
    }

    private int GetNextScanIndex(bool reverse)
    {
        int count = _scanTargets.Count;
        if (count == 0) return -1;

        int current = _scanFocusIndex;
        if (current < 0 || current >= count)
            current = reverse ? count - 1 : 0;
        else
            current = reverse ? current - 1 : current + 1;

        bool wrap = _configService.Current.SwitchScanWrapEnabled;
        if (wrap)
            return (current + count) % count;

        return Math.Clamp(current, 0, count - 1);
    }

    private void SetScanFocus(int index)
    {
        if (_scanFocusIndex >= 0 && _scanFocusIndex < _scanTargets.Count)
            _scanTargets[_scanFocusIndex].SetScanFocused(false);

        _scanFocusIndex = index;
        _a11yFocusIndex = -1;

        if (_scanFocusIndex >= 0 && _scanFocusIndex < _scanTargets.Count)
            _scanTargets[_scanFocusIndex].SetScanFocused(true);
    }

    private void RebuildScanTargets()
    {
        _scanTargets.Clear();
        var config = _configService.Current;
        var keyboardTargets = BuildKeyboardScanTargets(config.SwitchScanMode);

        var suggestionTargets = config.SwitchScanIncludeSuggestions
            ? _suggestionBar.ScanTargets.ToList()
            : [];

        if (config.SwitchScanSuggestionPriority == SwitchScanSuggestionPriority.BeforeKeyboard)
        {
            _scanTargets.AddRange(suggestionTargets);
            _scanTargets.AddRange(keyboardTargets);
        }
        else
        {
            _scanTargets.AddRange(keyboardTargets);
            _scanTargets.AddRange(suggestionTargets);
        }
    }

    private List<ScanTargetVm> BuildKeyboardScanTargets(SwitchScanMode mode)
    {
        if (mode == SwitchScanMode.RowColumn)
            return _isRowSelectionPhase ? BuildRowTargets() : BuildKeyTargetsInSelectedRow();

        return EnumerateSlotVms()
            .Select(vm => new ScanTargetVm
            {
                DisplayText = vm.DisplayLabel,
                Kind = "KeyboardKey",
                AccessibleName = vm.AccessibleName,
                Activate = () => KeyPressed(vm.Slot),
                SetScanFocused = isFocused => vm.IsA11yFocused = isFocused
            }).ToList();
    }

    private List<ScanTargetVm> BuildRowTargets()
    {
        var targets = new List<ScanTargetVm>();
        int rowIndex = 0;
        foreach (var row in Columns.SelectMany(c => c.Rows))
        {
            int capturedRow = rowIndex;
            string label = $"English text {capturedRow + 1}";
            targets.Add(new ScanTargetVm
            {
                DisplayText = label,
                Kind = "KeyboardRow",
                AccessibleName = label,
                Activate = () => { },
                SetScanFocused = isFocused =>
                {
                    foreach (var key in row.Keys)
                        key.IsA11yFocused = isFocused;
                }
            });
            rowIndex++;
        }
        return targets;
    }

    private List<ScanTargetVm> BuildKeyTargetsInSelectedRow()
    {
        var rows = Columns.SelectMany(c => c.Rows).ToList();
        if (_selectedRowIndex < 0 || _selectedRowIndex >= rows.Count)
            return [];

        var row = rows[_selectedRowIndex];
        return row.Keys.Select(vm => new ScanTargetVm
        {
            DisplayText = vm.DisplayLabel,
            Kind = "KeyboardKey",
            AccessibleName = vm.AccessibleName,
            Activate = () => KeyPressed(vm.Slot),
            SetScanFocused = isFocused => vm.IsA11yFocused = isFocused
        }).ToList();
    }

    private void OnSuggestionScanTargetsChanged()
    {
        if (A11yFocusOwner != A11yFocusOwner.SwitchScan)
            return;

        RebuildScanTargets();
        if (_scanTargets.Count == 0)
        {
            _scanFocusIndex = -1;
            return;
        }

        if (_scanFocusIndex >= _scanTargets.Count || _scanFocusIndex < 0)
            SetScanFocus(0);
    }

    private void AnnounceFocusedTarget()
    {
        if (_a11yFocusIndex < 0 || _a11yFocusIndex >= _a11yNavigableSlots.Count)
            return;
        _liveRegion.Announce(_a11yNavigableSlots[_a11yFocusIndex].AccessibleName);
    }

    private void AnnounceScanMove(string name)
    {
        if (_configService.Current.SwitchScanAnnounceMode != SwitchScanAnnounceMode.EveryMove)
            return;
        _liveRegion.Announce(name);
    }

    private void AnnounceScanSelection(string name)
    {
        if (_configService.Current.SwitchScanAnnounceMode == SwitchScanAnnounceMode.Off)
            return;
        _liveRegion.Announce($"English text: {name}");
    }

    private static bool IsSeparatorKey(KeySlot slot) => slot.Action switch
    {
        SendKeyAction { Vk: "VK_SPACE" }  => true,
        _ => false,
    };

    private void OnSubmodeChanged(InputSubmode submode)
    {
        RefreshKeyLabels(submode);

        _liveRegion.Announce(submode == InputSubmode.QuietEnglish
            ? "English text"
            : "English text");
    }

    private void RefreshKeyLabels(InputSubmode submode)
    {
        foreach (var col in Columns)
            foreach (var row in col.Rows)
                foreach (var keyVm in row.Keys)
                {
                    keyVm.ActiveSubmode = submode;
                    keyVm.SetComposeStateLabel(_autoComplete.ComposeStateLabel);
                    keyVm.SetFunctionLayerState(_inputService.FunctionLayerState);
                }
    }

    private void OnTimerTick(object? sender, EventArgs e)
    {
        UpdateModifierState();
    }

    private void UpdateModifierState()
    {
        ShowUpperCase =
            _inputService.StickyKeys.Contains(VirtualKeyCode.VK_SHIFT) ||
            _inputService.LockedKeys.Contains(VirtualKeyCode.VK_SHIFT) ||
            _inputService.StickyKeys.Contains(VirtualKeyCode.VK_LSHIFT) ||
            _inputService.LockedKeys.Contains(VirtualKeyCode.VK_LSHIFT) ||
            _inputService.IsCapsLockOn;

        bool showShiftLabels =
            _inputService.StickyKeys.Contains(VirtualKeyCode.VK_SHIFT) ||
            _inputService.LockedKeys.Contains(VirtualKeyCode.VK_SHIFT) ||
            _inputService.StickyKeys.Contains(VirtualKeyCode.VK_LSHIFT) ||
            _inputService.LockedKeys.Contains(VirtualKeyCode.VK_LSHIFT);

        foreach (var col in Columns)
        foreach (var row in col.Rows)
        foreach (var slotVm in row.Keys)
        {
            if (slotVm.StickyVk is { } vk)
            {
                slotVm.IsSticky = _inputService.StickyKeys.Contains(vk);
                slotVm.IsLocked = _inputService.LockedKeys.Contains(vk);
            }

            if (slotVm.Slot.Action is SendKeyAction { Vk: "VK_CAPITAL" })
            {
                slotVm.IsLocked = _inputService.IsCapsLockOn;
            }

            slotVm.SetFunctionLayerState(_inputService.FunctionLayerState);
            slotVm.SetModifierDisplayState(showShiftLabels, _inputService.IsCapsLockOn);
        }
    }

    private void OnElevatedAppDetected()
    {
        if (_inputService.Mode == InputMode.VirtualKey)
            return;

        ShowElevatedWarning = true;

        var dismissTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(5) };
        dismissTimer.Tick += (_, _) =>
        {
            ShowElevatedWarning = false;
            dismissTimer.Stop();
        };
        dismissTimer.Start();
    }
}
