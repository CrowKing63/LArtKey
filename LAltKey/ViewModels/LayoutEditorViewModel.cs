using System.Collections.ObjectModel;
using LAltKey.Models;
using LAltKey.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

using System.Collections.Specialized;
using System.ComponentModel;
using System.Text.Json;
using System.Windows.Threading;

namespace LAltKey.ViewModels;

// ── text VM ──────────────────────────────────────────────────

public partial class EditableKeySlotVm : ObservableObject
{
    public const double DefaultHeightRatio = 1.0;
    public const double CompactHeightRatio = 2.0 / 3.0;

    // text style_key text.
    public const string SoftAccentStyleKey = "soft_accent";

    [ObservableProperty] private string  editLabel       = "";
    [ObservableProperty] private string? editShiftLabel;
    [ObservableProperty] private double  editWidth       = 1.0;
    [ObservableProperty] private double  editHeight      = DefaultHeightRatio;
    [ObservableProperty] private double  editGapBefore   = 0.0;
    [ObservableProperty] private KeyAction? editAction;
    [ObservableProperty] private string  editStyleKey    = "";
    [ObservableProperty] private bool    useSoftAccentStyle;
    [ObservableProperty] private bool    isSelected      = false;

    [ObservableProperty] private string? englishLabel;
    [ObservableProperty] private string? englishShiftLabel;
    [ObservableProperty] private KeyAction? functionAction;
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(FunctionPreviewLabel))]
    private string? functionLabel;
    [ObservableProperty] private string? functionShiftLabel;
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(FunctionPreviewLabel))]
    private string? functionEnglishLabel;
    [ObservableProperty] private string? functionEnglishShiftLabel;

    /// <summary>
    /// text.
    /// </summary>
    public bool SupportsAccentStyle => true;

    /// <summary>
    /// Fn layer.
    /// </summary>
    public bool CanEditFunctionOverrides => EditAction is not ToggleFunctionLayerAction;

    /// text
    public KeySlot ToKeySlot() =>
        new(EditLabel, EditShiftLabel, EditAction, EditWidth, EditHeight,
            UseSoftAccentStyle ? SoftAccentStyleKey : "", EditGapBefore, EnglishLabel, EnglishShiftLabel,
            FunctionAction, FunctionLabel, FunctionShiftLabel, FunctionEnglishLabel, FunctionEnglishShiftLabel);

    public string? FunctionPreviewLabel => FunctionLabel ?? FunctionEnglishLabel;

    partial void OnEditActionChanged(KeyAction? value)
    {
        // Fn layer.
        if (value is ToggleFunctionLayerAction)
        {
            FunctionAction = null;
            FunctionLabel = null;
            FunctionShiftLabel = null;
            FunctionEnglishLabel = null;
            FunctionEnglishShiftLabel = null;
        }

        OnPropertyChanged(nameof(SupportsAccentStyle));
        OnPropertyChanged(nameof(CanEditFunctionOverrides));
    }

    partial void OnEditStyleKeyChanged(string value)
    {
        var next = string.Equals(value, SoftAccentStyleKey, StringComparison.Ordinal);
        if (UseSoftAccentStyle != next)
            UseSoftAccentStyle = next;
    }

    partial void OnUseSoftAccentStyleChanged(bool value)
    {
        var next = value ? SoftAccentStyleKey : "";
        if (EditStyleKey != next)
            EditStyleKey = next;
    }
}

// ── text VM ────────────────────────────────────────────────────

public partial class EditableKeyRowVm : ObservableObject
{
    [ObservableProperty] private int sharedRowIndex;
    [ObservableProperty] private double heightRatio = EditableKeySlotVm.DefaultHeightRatio;

    [ObservableProperty]
    private ObservableCollection<EditableKeySlotVm> keys = [];

    /// <summary>
    /// text.
    /// </summary>
    public string HeightPresetLabel =>
        Math.Abs(HeightRatio - EditableKeySlotVm.CompactHeightRatio) < 0.001 ? "Compact" : "Default";

    public void ApplyHeight(double heightRatio)
    {
        HeightRatio = heightRatio;
        foreach (var key in Keys)
            key.EditHeight = heightRatio;
        OnPropertyChanged(nameof(HeightPresetLabel));
    }

    public KeyRow ToKeyRow() => new(Keys.Select(k => k.ToKeySlot()).ToList());
}

// ── text VM ────────────────────────────────────────────────────────

public partial class EditableKeyColumnVm : ObservableObject
{
    [ObservableProperty] private double gap = 0;

    [ObservableProperty]
    private ObservableCollection<EditableKeyRowVm> rows = [];

    public KeyColumn ToKeyColumn() => new(Gap, Rows.Select(r => r.ToKeyRow()).ToList());
}

internal sealed record LayoutEditorSnapshot(string CurrentFileName, LayoutConfig Layout);

// ── LayoutEditorViewModel ───────────────────────────────────────────────────

public partial class LayoutEditorViewModel : ObservableObject
{
    private readonly ILayoutRepository _layoutRepository;
    private readonly ConfigService _configService;
    private readonly DispatcherTimer _changeCheckpointTimer;
    private readonly Stack<LayoutEditorSnapshot> _undoStack = [];
    private readonly JsonSerializerOptions _snapshotJsonOptions = new() { WriteIndented = false };
    private LayoutEditorSnapshot? _savedSnapshot;
    private LayoutEditorSnapshot? _trackingSnapshot;
    private LayoutEditorSnapshot? _pendingUndoSnapshot;
    private ObservableCollection<ObservableString>? _actionBuilderComboCollection;
    private ObservableCollection<ObservableString>? _functionActionBuilderComboCollection;
    private bool _isRestoringSnapshot;
    private bool _isLoadingActionBuilder;
    private bool _isLoadingFunctionActionBuilder;

    // ── VK → text) ─────────────────────
    private static readonly Dictionary<string, (string Label, string? ShiftLabel, string? EnglishLabel)> VkLabelMap
        = new(StringComparer.OrdinalIgnoreCase)
    {
        ["VK_Q"] = ("ㅂ", "ㅃ", "q"), ["VK_W"] = ("ㅈ", "ㅉ", "w"),
        ["VK_E"] = ("ㄷ", "ㄸ", "e"), ["VK_R"] = ("ㄱ", "ㄲ", "r"),
        ["VK_T"] = ("ㅅ", "ㅆ", "t"), ["VK_Y"] = ("ㅛ", null, "y"),
        ["VK_U"] = ("ㅕ", null, "u"), ["VK_I"] = ("ㅑ", null, "i"),
        ["VK_O"] = ("ㅐ", "ㅒ", "o"), ["VK_P"] = ("ㅔ", "ㅖ", "p"),
        ["VK_A"] = ("ㅁ", null, "a"), ["VK_S"] = ("ㄴ", null, "s"),
        ["VK_D"] = ("ㅇ", null, "d"), ["VK_F"] = ("ㄹ", null, "f"),
        ["VK_G"] = ("ㅎ", null, "g"), ["VK_H"] = ("ㅗ", null, "h"),
        ["VK_J"] = ("ㅓ", null, "j"), ["VK_K"] = ("ㅏ", null, "k"),
        ["VK_L"] = ("ㅣ", null, "l"),
        ["VK_Z"] = ("ㅋ", null, "z"), ["VK_X"] = ("ㅌ", null, "x"),
        ["VK_C"] = ("ㅊ", null, "c"), ["VK_V"] = ("ㅍ", null, "v"),
        ["VK_B"] = ("ㅠ", null, "b"), ["VK_N"] = ("ㅜ", null, "n"),
        ["VK_M"] = ("ㅡ", null, "m"),
    };

    // ── text ────────────────────────────────────────────────
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsExistingLayout))]
    [NotifyPropertyChangedFor(nameof(CanDeleteLayout))]
    private string currentFileName = "";

    partial void OnCurrentFileNameChanged(string value)
    {
        OnPropertyChanged(nameof(IsEditingCurrentLayout));
        HandleWorkingCopyMutated();
    }

    partial void OnLayoutNameChanged(string value) => HandleWorkingCopyMutated();

    /// text)
    public bool IsExistingLayout => !string.IsNullOrEmpty(CurrentFileName)
        && _layoutRepository.GetAvailableLayouts().Contains(CurrentFileName);

    /// text
    public bool CanDeleteLayout => IsExistingLayout
        && !string.Equals(CurrentFileName, _layoutRepository.DefaultLayoutName,
            StringComparison.OrdinalIgnoreCase);

    /// <summary>
    /// text.
    /// </summary>
    public string CurrentActiveLayoutName => _configService.Current.DefaultLayout;

    /// <summary>
    /// text.
    /// </summary>
    public bool IsEditingCurrentLayout =>
        !string.IsNullOrWhiteSpace(CurrentFileName)
        && string.Equals(CurrentFileName, CurrentActiveLayoutName, StringComparison.OrdinalIgnoreCase);

    // ── text ────────────────────────────────────────────────────
    [ObservableProperty] private string layoutName = "";

    [ObservableProperty] private ObservableCollection<EditableKeyColumnVm> columns = [];

    // ── text ─────────────────────────────────────────────────────
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasSelectedColumn))]
    private EditableKeyColumnVm? selectedColumn;

    public bool HasSelectedColumn => SelectedColumn is not null;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasSelectedRow))]
    private EditableKeyRowVm? selectedRow;

    public bool HasSelectedRow => SelectedRow is not null;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasSelectedKey))]
    [NotifyPropertyChangedFor(nameof(CanMoveKeyLeft))]
    [NotifyPropertyChangedFor(nameof(CanMoveKeyRight))]
    private EditableKeySlotVm? selectedKey;

    public bool HasSelectedKey => SelectedKey is not null;

    public bool CanMoveKeyLeft
    {
        get
        {
            if (SelectedKey is null) return false;
            var row = FindRowContaining(SelectedKey);
            if (row is null) return false;
            return row.Keys.IndexOf(SelectedKey) > 0;
        }
    }

    public bool CanMoveKeyRight
    {
        get
        {
            if (SelectedKey is null) return false;
            var row = FindRowContaining(SelectedKey);
            if (row is null) return false;
            return row.Keys.IndexOf(SelectedKey) < row.Keys.Count - 1;
        }
    }

    // ── text ─────────────────────────────────────────────
    [ObservableProperty] private string statusMessage = "";

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(UndoCommand))]
    [NotifyCanExecuteChangedFor(nameof(CancelEditsCommand))]
    private bool hasUnsavedChanges;

    public bool CanUndo => _undoStack.Count > 0;

    // ── text ──────────────────────────────────────────
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CanMoveColumnContents))]
    private bool showColumnDeleteDialog = false;

    private EditableKeyColumnVm? _pendingDeleteColumn;

    /// text 'text' text
    public bool CanMoveColumnContents =>
        _pendingDeleteColumn is not null && Columns.IndexOf(_pendingDeleteColumn) > 0;

    // ── text ActionBuilder ────────────────────────────────────────────────
    public ActionBuilderViewModel ActionBuilder { get; } = new();
    public ActionBuilderViewModel FunctionActionBuilder { get; } = new();

    // ── text ────────────────────────────────────
    [ObservableProperty]
    private ObservableCollection<string> availableLayouts = [];

    [ObservableProperty]
    private string selectedLayoutToLoad = "";

    public LayoutEditorViewModel(ILayoutRepository layoutRepository, ConfigService configService)
    {
        _layoutRepository = layoutRepository;
        _configService = configService;
        _changeCheckpointTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromMilliseconds(350)
        };
        _changeCheckpointTimer.Tick += (_, _) => FlushPendingCheckpoint();
        HookActionBuilderEvents();
        HookFunctionActionBuilderEvents();
        RefreshAvailableLayouts();
        RefreshCurrentActiveLayoutInfo();
    }

    private void RefreshAvailableLayouts()
    {
        AvailableLayouts = new ObservableCollection<string>(
            _layoutRepository.GetAvailableLayouts());
        if (AvailableLayouts.Count > 0 && string.IsNullOrEmpty(SelectedLayoutToLoad))
            SelectedLayoutToLoad = AvailableLayouts[0];
        OnPropertyChanged(nameof(IsExistingLayout));
        OnPropertyChanged(nameof(CanDeleteLayout));
    }

    private void RefreshCurrentActiveLayoutInfo()
    {
        _configService.Load();
        OnPropertyChanged(nameof(CurrentActiveLayoutName));
        OnPropertyChanged(nameof(IsEditingCurrentLayout));
    }

    // ── text ──────────────────────────────────────────────────────

    [RelayCommand]
    public void LoadLayout(string fileName)
    {
        var config = _layoutRepository.TryLoad(fileName);
        if (config is null) return;
        ApplySnapshot(new LayoutEditorSnapshot(fileName, CloneLayoutConfig(config)), resetUndoHistory: true);
        StatusMessage  = $"'{config.Name}' loaded";
    }

    [RelayCommand]
    private void LoadSelected()
    {
        if (!string.IsNullOrEmpty(SelectedLayoutToLoad))
            LoadLayout(SelectedLayoutToLoad);
    }

    // ── text ─────────────────────────────────────────────────
    [RelayCommand]
    private void NewLayout()
    {
        ApplySnapshot(new LayoutEditorSnapshot("", new LayoutConfig("New layout", null, [])), resetUndoHistory: true);
        StatusMessage  = "New layout created.";
    }

    // ── SelectedKey text ────────────────────────────
    partial void OnSelectedKeyChanged(EditableKeySlotVm? oldValue, EditableKeySlotVm? newValue)
    {
        if (oldValue is not null) oldValue.IsSelected = false;
        if (newValue is not null) newValue.IsSelected = true;
        LoadActionBuilderFromSelectedKey(newValue);
        LoadFunctionActionBuilderFromSelectedKey(newValue);
        OnPropertyChanged(nameof(CanMoveKeyLeft));
        OnPropertyChanged(nameof(CanMoveKeyRight));
    }

    // ── text ────────────────────────────────────────────────────

    [RelayCommand]
    public void SelectKey(EditableKeySlotVm slot)
    {
        SelectedKey = slot;
    }

    /// text.
    [RelayCommand]
    private void AutoFillBaseLabels()
    {
        if (SelectedKey is null) return;

        var action = SelectedKey.EditAction ?? ActionBuilder.BuildAction();
        StatusMessage = TryApplyLabelsFromVk(
            action,
            applyMapped: mapping =>
            {
                SelectedKey.EditLabel = mapping.Label;
                SelectedKey.EditShiftLabel = mapping.ShiftLabel;
                SelectedKey.EnglishLabel = mapping.EnglishLabel;
                SelectedKey.EnglishShiftLabel = null;
            },
            applyDisplayOnly: displayName =>
            {
                SelectedKey.EditLabel = displayName;
                SelectedKey.EditShiftLabel = null;
                SelectedKey.EnglishLabel = null;
                SelectedKey.EnglishShiftLabel = null;
            },
            "Selected key");
    }

    /// text.
    [RelayCommand]
    private void AutoFillFunctionLabels()
    {
        if (SelectedKey is null) return;
        if (!SelectedKey.CanEditFunctionOverrides)
        {
            StatusMessage = "Fn layer";
            return;
        }

        var action = SelectedKey.FunctionAction ?? FunctionActionBuilder.BuildAction();
        StatusMessage = TryApplyLabelsFromVk(
            action,
            applyMapped: mapping =>
            {
                SelectedKey.FunctionLabel = mapping.Label;
                SelectedKey.FunctionShiftLabel = mapping.ShiftLabel;
                SelectedKey.FunctionEnglishLabel = mapping.EnglishLabel;
                SelectedKey.FunctionEnglishShiftLabel = null;
            },
            applyDisplayOnly: displayName =>
            {
                SelectedKey.FunctionLabel = displayName;
                SelectedKey.FunctionShiftLabel = null;
                SelectedKey.FunctionEnglishLabel = null;
                SelectedKey.FunctionEnglishShiftLabel = null;
            },
            "Fn layer");
    }

    // ── text ─────────────────────────────────────────────────────

    [RelayCommand]
    private void AddColumn()
    {
        var newColumn = new EditableKeyColumnVm { Gap = 0 };
        Columns.Add(newColumn);
        NormalizeSharedRowHeights();
        SelectedColumn = newColumn;
        StatusMessage = "Column added.";
    }

    [RelayCommand]
    private void RequestRemoveColumn(EditableKeyColumnVm column)
    {
        // text
        bool hasContent = column.Rows.Count > 0 && column.Rows.Any(r => r.Keys.Count > 0);
        if (hasContent)
        {
            _pendingDeleteColumn = column;
            ShowColumnDeleteDialog = true;
        }
        else
        {
            // text
            ExecuteRemoveColumn(column);
        }
    }

    [RelayCommand]
    private void ConfirmDeleteColumnAll()
    {
        if (_pendingDeleteColumn is not null)
        {
            ExecuteRemoveColumn(_pendingDeleteColumn);
            _pendingDeleteColumn = null;
        }
        ShowColumnDeleteDialog = false;
    }

    [RelayCommand]
    private void ConfirmDeleteColumnMove()
    {
        if (_pendingDeleteColumn is not null)
        {
            int idx = Columns.IndexOf(_pendingDeleteColumn);
            if (idx > 0)
            {
                var prevColumn = Columns[idx - 1];
                // text
                for (int i = 0; i < _pendingDeleteColumn.Rows.Count; i++)
                {
                    if (i < prevColumn.Rows.Count)
                    {
                        // text
                        foreach (var key in _pendingDeleteColumn.Rows[i].Keys)
                            prevColumn.Rows[i].Keys.Add(key);
                    }
                    else
                    {
                        // text
                        prevColumn.Rows.Add(_pendingDeleteColumn.Rows[i]);
                    }
                }
            }
            // text
            ExecuteRemoveColumn(_pendingDeleteColumn, clearSelection: true);
            NormalizeSharedRowHeights();
            _pendingDeleteColumn = null;
        }
        ShowColumnDeleteDialog = false;
    }

    [RelayCommand]
    private void CancelDeleteColumn()
    {
        _pendingDeleteColumn = null;
        ShowColumnDeleteDialog = false;
    }

    private void ExecuteRemoveColumn(EditableKeyColumnVm column, bool clearSelection = false)
    {
        if (SelectedKey is not null)
        {
            foreach (var row in column.Rows)
            {
                if (row.Keys.Contains(SelectedKey))
                {
                    SelectedKey = null;
                    break;
                }
            }
        }

        if (SelectedRow is not null && column.Rows.Contains(SelectedRow))
            SelectedRow = null;

        if (SelectedColumn == column)
            SelectedColumn = null;

        Columns.Remove(column);
        NormalizeSharedRowHeights();
        StatusMessage = "Column removed.";
    }

    // ── text ─────────────────────────────────────────────────────

    [RelayCommand]
    private void AddRow(EditableKeyColumnVm? targetColumn = null)
    {
        // CommandParametertext
        targetColumn ??= SelectedColumn ?? Columns.FirstOrDefault();
        if (targetColumn is null)
        {
            targetColumn = new EditableKeyColumnVm { Gap = 0 };
            Columns.Add(targetColumn);
        }

        var newRow = new EditableKeyRowVm();
        targetColumn.Rows.Add(newRow);
        NormalizeSharedRowHeights();
        SelectedColumn = targetColumn;
        SelectedRow = newRow;
        StatusMessage = "Row added.";
    }

    [RelayCommand]
    private void RemoveRow(EditableKeyRowVm row)
    {
        if (SelectedKey is not null && row.Keys.Contains(SelectedKey))
            SelectedKey = null;

        if (SelectedRow == row)
            SelectedRow = null;

        foreach (var column in Columns)
        {
            if (column.Rows.Contains(row))
            {
                column.Rows.Remove(row);
                break;
            }
        }

        NormalizeSharedRowHeights();
        StatusMessage = "Row removed.";
    }

    [RelayCommand]
    private void SetRowDefaultHeight(EditableKeyRowVm row)
    {
        ApplySharedHeightToRowBand(row, EditableKeySlotVm.DefaultHeightRatio);
        StatusMessage = "Row height set to default.";
    }

    [RelayCommand]
    private void SetRowCompactHeight(EditableKeyRowVm row)
    {
        ApplySharedHeightToRowBand(row, EditableKeySlotVm.CompactHeightRatio);
        StatusMessage = "Row height set to compact.";
    }

    // ── text ─────────────────────────────────────────────────

    [RelayCommand]
    private void MoveKeyLeft()
    {
        if (SelectedKey is null) return;
        var row = FindRowContaining(SelectedKey);
        if (row is null) return;
        var idx = row.Keys.IndexOf(SelectedKey);
        if (idx <= 0) return;
        row.Keys.Move(idx, idx - 1);
        OnPropertyChanged(nameof(CanMoveKeyLeft));
        OnPropertyChanged(nameof(CanMoveKeyRight));
    }

    [RelayCommand]
    private void MoveKeyRight()
    {
        if (SelectedKey is null) return;
        var row = FindRowContaining(SelectedKey);
        if (row is null) return;
        var idx = row.Keys.IndexOf(SelectedKey);
        if (idx < 0 || idx >= row.Keys.Count - 1) return;
        row.Keys.Move(idx, idx + 1);
        OnPropertyChanged(nameof(CanMoveKeyLeft));
        OnPropertyChanged(nameof(CanMoveKeyRight));
    }

    private EditableKeyRowVm? FindRowContaining(EditableKeySlotVm key)
    {
        foreach (var column in Columns)
            foreach (var row in column.Rows)
                if (row.Keys.Contains(key))
                    return row;
        return null;
    }

    [RelayCommand]
    private void AddKeyToRow(EditableKeyRowVm row)
    {
        row.Keys.Add(new EditableKeySlotVm
        {
            EditLabel = "Key",
            EditHeight = row.HeightRatio
        });
    }

    [RelayCommand]
    private void RemoveKey(EditableKeySlotVm key)
    {
        foreach (var column in Columns)
        {
            foreach (var row in column.Rows)
            {
                if (row.Keys.Contains(key))
                {
                    row.Keys.Remove(key);
                    break;
                }
            }
        }

        if (SelectedKey == key)
            SelectedKey = null;
    }

    // ── text ──────────────────────────────────────
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CanDeletePendingLayout))]
    private bool showDeleteLayoutDialog = false;

    private string? _pendingDeleteLayoutName;

    /// text
    public bool CanDeletePendingLayout =>
        _pendingDeleteLayoutName is not null && !string.Equals(_pendingDeleteLayoutName,
            _layoutRepository.DefaultLayoutName, StringComparison.OrdinalIgnoreCase);

    // ── text ───────────────────────────────────────────────────
    [RelayCommand]
    private void RequestDeleteLayout()
    {
        if (string.IsNullOrWhiteSpace(CurrentFileName)) return;
        _pendingDeleteLayoutName = CurrentFileName;
        ShowDeleteLayoutDialog = true;
    }

    // ── text ───────────────────────────────────────────────────
    [RelayCommand]
    private void ConfirmDeleteLayout()
    {
        if (_pendingDeleteLayoutName is null) return;

        try
        {
            if (_layoutRepository.Delete(_pendingDeleteLayoutName))
            {
                // text.
                ToolsReloadSignalService.NotifyReloadLayouts();
                StatusMessage = $"'{_pendingDeleteLayoutName}' deleted";
                ApplySnapshot(new LayoutEditorSnapshot("", new LayoutConfig("", null, [])), resetUndoHistory: true);
                RefreshAvailableLayouts();
            }
            else
            {
                StatusMessage = "Layout could not be deleted.";
            }
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error: {ex.Message}";
        }

        _pendingDeleteLayoutName = null;
        ShowDeleteLayoutDialog = false;
    }

    // ── text ───────────────────────────────────────────────────
    [RelayCommand]
    private void CancelDeleteLayout()
    {
        _pendingDeleteLayoutName = null;
        ShowDeleteLayoutDialog = false;
    }

    // ── text ────────────────────────────────

    [RelayCommand]
    private void Save()
    {
        if (string.IsNullOrWhiteSpace(CurrentFileName))
        {
            StatusMessage = "Enter a layout file name before saving.";
            return;
        }

        FlushPendingCheckpoint();

        try
        {
            var layoutToSave = BuildLayoutConfig();
            _layoutRepository.Save(CurrentFileName, layoutToSave);
            // text, "Custom shortcut" text.
            ToolsReloadSignalService.NotifyReloadLayouts();
            RefreshAvailableLayouts();
            RefreshCurrentActiveLayoutInfo();
            ResetUndoHistory(new LayoutEditorSnapshot(CurrentFileName, CloneLayoutConfig(layoutToSave)));

            var verification = _layoutRepository.TryLoad(CurrentFileName);
            var accentKeyCount = verification is null ? 0 : CountSoftAccentKeys(verification);
            var currentLayoutHint = IsEditingCurrentLayout
                ? "The main keyboard is using this layout."
                : $"The main keyboard is currently using '{CurrentActiveLayoutName}'.";

            StatusMessage = $"'{CurrentFileName}' saved. Highlighted keys: {accentKeyCount} · {currentLayoutHint}";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error: {ex.Message}";
        }
    }

    [RelayCommand]
    private void SaveAs()
    {
        if (string.IsNullOrWhiteSpace(CurrentFileName))
        {
            StatusMessage = "Enter a layout file name before saving.";
            return;
        }

        Save();
        SelectedLayoutToLoad = CurrentFileName;
    }

    [RelayCommand(CanExecute = nameof(CanUndo))]
    private void Undo()
    {
        FlushPendingCheckpoint();
        if (_undoStack.Count == 0)
            return;

        var snapshot = _undoStack.Pop();
        ApplySnapshot(snapshot, resetUndoHistory: false);
        StatusMessage = "Undo applied.";
        OnPropertyChanged(nameof(CanUndo));
        UndoCommand.NotifyCanExecuteChanged();
    }

    [RelayCommand(CanExecute = nameof(HasUnsavedChanges))]
    private void CancelEdits()
    {
        if (_savedSnapshot is null)
            return;

        _changeCheckpointTimer.Stop();
        _pendingUndoSnapshot = null;
        ApplySnapshot(_savedSnapshot, resetUndoHistory: true);
        StatusMessage = "Edits canceled.";
    }

    // ── text ─────────────────────────────────────────────────────────

    private void HookActionBuilderEvents()
    {
        ActionBuilder.PropertyChanged += OnActionBuilderPropertyChanged;
        RewireActionBuilderComboCollection(_actionBuilderComboCollection, ActionBuilder.SendComboKeysCollection);
    }

    private void HookFunctionActionBuilderEvents()
    {
        FunctionActionBuilder.PropertyChanged += OnFunctionActionBuilderPropertyChanged;
        RewireFunctionActionBuilderComboCollection(_functionActionBuilderComboCollection, FunctionActionBuilder.SendComboKeysCollection);
    }

    private void OnActionBuilderPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(ActionBuilderViewModel.SendComboKeysCollection))
        {
            RewireActionBuilderComboCollection(_actionBuilderComboCollection, ActionBuilder.SendComboKeysCollection);
        }

        if (_isLoadingActionBuilder || SelectedKey is null)
            return;

        SelectedKey.EditAction = CloneAction(ActionBuilder.BuildAction());
        LoadFunctionActionBuilderFromSelectedKey(SelectedKey);
    }

    private void RewireActionBuilderComboCollection(ObservableCollection<ObservableString>? oldCollection,
        ObservableCollection<ObservableString> newCollection)
    {
        if (oldCollection is not null)
        {
            oldCollection.CollectionChanged -= OnActionBuilderComboCollectionChanged;
            foreach (var item in oldCollection)
                item.PropertyChanged -= OnActionBuilderComboItemPropertyChanged;
        }

        newCollection.CollectionChanged -= OnActionBuilderComboCollectionChanged;
        newCollection.CollectionChanged += OnActionBuilderComboCollectionChanged;
        foreach (var item in newCollection)
        {
            item.PropertyChanged -= OnActionBuilderComboItemPropertyChanged;
            item.PropertyChanged += OnActionBuilderComboItemPropertyChanged;
        }

        _actionBuilderComboCollection = newCollection;
    }

    private void OnActionBuilderComboCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (e.OldItems is not null)
        {
            foreach (ObservableString item in e.OldItems)
                item.PropertyChanged -= OnActionBuilderComboItemPropertyChanged;
        }

        if (e.NewItems is not null)
        {
            foreach (ObservableString item in e.NewItems)
            {
                item.PropertyChanged -= OnActionBuilderComboItemPropertyChanged;
                item.PropertyChanged += OnActionBuilderComboItemPropertyChanged;
            }
        }

        OnActionBuilderPropertyChanged(ActionBuilder, new PropertyChangedEventArgs(nameof(ActionBuilderViewModel.SendComboKeysCollection)));
    }

    private void OnActionBuilderComboItemPropertyChanged(object? sender, PropertyChangedEventArgs e) =>
        OnActionBuilderPropertyChanged(ActionBuilder, new PropertyChangedEventArgs(nameof(ActionBuilderViewModel.SendComboKeysCollection)));

    private void OnFunctionActionBuilderPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(ActionBuilderViewModel.SendComboKeysCollection))
        {
            RewireFunctionActionBuilderComboCollection(_functionActionBuilderComboCollection, FunctionActionBuilder.SendComboKeysCollection);
        }

        if (_isLoadingFunctionActionBuilder || SelectedKey is null)
            return;

        SelectedKey.FunctionAction = CloneAction(FunctionActionBuilder.BuildAction());
    }

    private void RewireFunctionActionBuilderComboCollection(ObservableCollection<ObservableString>? oldCollection,
        ObservableCollection<ObservableString> newCollection)
    {
        if (oldCollection is not null)
        {
            oldCollection.CollectionChanged -= OnFunctionActionBuilderComboCollectionChanged;
            foreach (var item in oldCollection)
                item.PropertyChanged -= OnFunctionActionBuilderComboItemPropertyChanged;
        }

        newCollection.CollectionChanged -= OnFunctionActionBuilderComboCollectionChanged;
        newCollection.CollectionChanged += OnFunctionActionBuilderComboCollectionChanged;
        foreach (var item in newCollection)
        {
            item.PropertyChanged -= OnFunctionActionBuilderComboItemPropertyChanged;
            item.PropertyChanged += OnFunctionActionBuilderComboItemPropertyChanged;
        }

        _functionActionBuilderComboCollection = newCollection;
    }

    private void OnFunctionActionBuilderComboCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (e.OldItems is not null)
        {
            foreach (ObservableString item in e.OldItems)
                item.PropertyChanged -= OnFunctionActionBuilderComboItemPropertyChanged;
        }

        if (e.NewItems is not null)
        {
            foreach (ObservableString item in e.NewItems)
            {
                item.PropertyChanged -= OnFunctionActionBuilderComboItemPropertyChanged;
                item.PropertyChanged += OnFunctionActionBuilderComboItemPropertyChanged;
            }
        }

        OnFunctionActionBuilderPropertyChanged(FunctionActionBuilder, new PropertyChangedEventArgs(nameof(ActionBuilderViewModel.SendComboKeysCollection)));
    }

    private void OnFunctionActionBuilderComboItemPropertyChanged(object? sender, PropertyChangedEventArgs e) =>
        OnFunctionActionBuilderPropertyChanged(FunctionActionBuilder, new PropertyChangedEventArgs(nameof(ActionBuilderViewModel.SendComboKeysCollection)));

    private void LoadActionBuilderFromSelectedKey(EditableKeySlotVm? key)
    {
        _isLoadingActionBuilder = true;
        try
        {
            ActionBuilder.LoadFromAction(key?.EditAction);
        }
        finally
        {
            _isLoadingActionBuilder = false;
        }
    }

    private void LoadFunctionActionBuilderFromSelectedKey(EditableKeySlotVm? key)
    {
        _isLoadingFunctionActionBuilder = true;
        try
        {
            FunctionActionBuilder.LoadFromAction(key?.FunctionAction);
        }
        finally
        {
            _isLoadingFunctionActionBuilder = false;
        }
    }

    private string TryApplyLabelsFromVk(
        KeyAction? action,
        Action<(string Label, string? ShiftLabel, string? EnglishLabel)> applyMapped,
        Action<string> applyDisplayOnly,
        string targetLabel)
    {
        string? vk = action switch
        {
            SendKeyAction sendKey => sendKey.Vk,
            _ => null
        };

        if (vk is null)
            return "SendKey text";

        if (VkLabelMap.TryGetValue(vk, out var mapping))
        {
            applyMapped(mapping);
            return $"{targetLabel} label: {mapping.Label} / {mapping.EnglishLabel}";
        }

        if (ActionBuilderViewModel.KeyDisplayNameMap.TryGetValue(vk, out var displayName))
        {
            applyDisplayOnly(displayName);
            return $"{targetLabel} label: {displayName}";
        }

        return $"'{vk}' key";
    }

    private void AttachWorkingCopyObservers()
    {
        Columns.CollectionChanged -= OnColumnsCollectionChanged;
        Columns.CollectionChanged += OnColumnsCollectionChanged;
        foreach (var column in Columns)
            AttachColumnObservers(column);
    }

    private void DetachWorkingCopyObservers()
    {
        Columns.CollectionChanged -= OnColumnsCollectionChanged;
        foreach (var column in Columns)
            DetachColumnObservers(column);
    }

    private void OnColumnsCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (e.OldItems is not null)
        {
            foreach (EditableKeyColumnVm column in e.OldItems)
                DetachColumnObservers(column);
        }

        if (e.NewItems is not null)
        {
            foreach (EditableKeyColumnVm column in e.NewItems)
                AttachColumnObservers(column);
        }

        HandleWorkingCopyMutated();
    }

    private void AttachColumnObservers(EditableKeyColumnVm column)
    {
        column.PropertyChanged -= OnWorkingItemPropertyChanged;
        column.PropertyChanged += OnWorkingItemPropertyChanged;
        column.Rows.CollectionChanged -= OnRowsCollectionChanged;
        column.Rows.CollectionChanged += OnRowsCollectionChanged;
        foreach (var row in column.Rows)
            AttachRowObservers(row);
    }

    private void DetachColumnObservers(EditableKeyColumnVm column)
    {
        column.PropertyChanged -= OnWorkingItemPropertyChanged;
        column.Rows.CollectionChanged -= OnRowsCollectionChanged;
        foreach (var row in column.Rows)
            DetachRowObservers(row);
    }

    private void OnRowsCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (e.OldItems is not null)
        {
            foreach (EditableKeyRowVm row in e.OldItems)
                DetachRowObservers(row);
        }

        if (e.NewItems is not null)
        {
            foreach (EditableKeyRowVm row in e.NewItems)
                AttachRowObservers(row);
        }

        HandleWorkingCopyMutated();
    }

    private void AttachRowObservers(EditableKeyRowVm row)
    {
        row.PropertyChanged -= OnWorkingItemPropertyChanged;
        row.PropertyChanged += OnWorkingItemPropertyChanged;
        row.Keys.CollectionChanged -= OnKeysCollectionChanged;
        row.Keys.CollectionChanged += OnKeysCollectionChanged;
        foreach (var key in row.Keys)
            AttachKeyObservers(key);
    }

    private void DetachRowObservers(EditableKeyRowVm row)
    {
        row.PropertyChanged -= OnWorkingItemPropertyChanged;
        row.Keys.CollectionChanged -= OnKeysCollectionChanged;
        foreach (var key in row.Keys)
            DetachKeyObservers(key);
    }

    private void OnKeysCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (e.OldItems is not null)
        {
            foreach (EditableKeySlotVm key in e.OldItems)
                DetachKeyObservers(key);
        }

        if (e.NewItems is not null)
        {
            foreach (EditableKeySlotVm key in e.NewItems)
                AttachKeyObservers(key);
        }

        HandleWorkingCopyMutated();
    }

    private void AttachKeyObservers(EditableKeySlotVm key)
    {
        key.PropertyChanged -= OnWorkingItemPropertyChanged;
        key.PropertyChanged += OnWorkingItemPropertyChanged;
    }

    private void DetachKeyObservers(EditableKeySlotVm key)
    {
        key.PropertyChanged -= OnWorkingItemPropertyChanged;
    }

    private void OnWorkingItemPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(EditableKeySlotVm.IsSelected))
            return;

        HandleWorkingCopyMutated();
    }

    private void HandleWorkingCopyMutated()
    {
        if (_isRestoringSnapshot || _trackingSnapshot is null)
            return;

        if (_pendingUndoSnapshot is null)
            _pendingUndoSnapshot = _trackingSnapshot;

        _changeCheckpointTimer.Stop();
        _changeCheckpointTimer.Start();
    }

    private void FlushPendingCheckpoint()
    {
        _changeCheckpointTimer.Stop();
        if (_pendingUndoSnapshot is null || _isRestoringSnapshot)
            return;

        var currentSnapshot = CaptureSnapshot();
        if (!SnapshotEquals(currentSnapshot, _pendingUndoSnapshot))
        {
            _undoStack.Push(_pendingUndoSnapshot);
            _trackingSnapshot = currentSnapshot;
        }

        _pendingUndoSnapshot = null;
        UpdateDirtyState();
        OnPropertyChanged(nameof(CanUndo));
        UndoCommand.NotifyCanExecuteChanged();
    }

    private void ApplySnapshot(LayoutEditorSnapshot snapshot, bool resetUndoHistory)
    {
        _isRestoringSnapshot = true;
        _changeCheckpointTimer.Stop();
        _pendingUndoSnapshot = null;

        try
        {
            DetachWorkingCopyObservers();

            CurrentFileName = snapshot.CurrentFileName;
            LayoutName = snapshot.Layout.Name;
            Columns = BuildEditableColumns(snapshot.Layout);
            NormalizeSharedRowHeights();
            SelectedColumn = null;
            SelectedRow = null;
            SelectedKey = null;
            RefreshCurrentActiveLayoutInfo();
            AttachWorkingCopyObservers();

            var clonedSnapshot = CloneSnapshot(snapshot);
            _trackingSnapshot = clonedSnapshot;

            if (resetUndoHistory)
            {
                _savedSnapshot = clonedSnapshot;
                _undoStack.Clear();
            }
        }
        finally
        {
            _isRestoringSnapshot = false;
        }

        UpdateDirtyState();
        OnPropertyChanged(nameof(CanUndo));
        UndoCommand.NotifyCanExecuteChanged();
    }

    private void ResetUndoHistory(LayoutEditorSnapshot snapshot)
    {
        _savedSnapshot = CloneSnapshot(snapshot);
        _trackingSnapshot = CloneSnapshot(snapshot);
        _pendingUndoSnapshot = null;
        _undoStack.Clear();
        UpdateDirtyState();
        OnPropertyChanged(nameof(CanUndo));
        UndoCommand.NotifyCanExecuteChanged();
    }

    private void UpdateDirtyState() =>
        HasUnsavedChanges = _savedSnapshot is not null && !SnapshotEquals(CaptureSnapshot(), _savedSnapshot);

    private LayoutEditorSnapshot CaptureSnapshot() =>
        new(CurrentFileName, CloneLayoutConfig(BuildLayoutConfig()));

    private LayoutEditorSnapshot CloneSnapshot(LayoutEditorSnapshot snapshot) =>
        new(snapshot.CurrentFileName, CloneLayoutConfig(snapshot.Layout));

    private ObservableCollection<EditableKeyColumnVm> BuildEditableColumns(LayoutConfig config)
    {
        if (config.Columns is not { Count: > 0 })
            return [];

        var sharedRowHeights = BuildSharedRowHeights(config);

        return new ObservableCollection<EditableKeyColumnVm>(
            config.Columns.Select(col => new EditableKeyColumnVm
            {
                Gap = col.Gap,
                Rows = new ObservableCollection<EditableKeyRowVm>(
                    col.Rows?.Select((r, rowIndex) => new EditableKeyRowVm
                    {
                        SharedRowIndex = rowIndex,
                        HeightRatio = sharedRowHeights.TryGetValue(rowIndex, out var rowHeight)
                            ? rowHeight
                            : EditableKeySlotVm.DefaultHeightRatio,
                        Keys = new ObservableCollection<EditableKeySlotVm>(
                            r.Keys.Select(k => new EditableKeySlotVm
                            {
                                EditLabel = k.Label,
                                EditShiftLabel = k.ShiftLabel,
                                EditWidth = k.Width,
                                EditHeight = sharedRowHeights.TryGetValue(rowIndex, out var slotHeight)
                                    ? slotHeight
                                    : NormalizeHeight(k.Height),
                                EditGapBefore = k.GapBefore,
                                EditStyleKey = k.StyleKey,
                                UseSoftAccentStyle = string.Equals(k.StyleKey, EditableKeySlotVm.SoftAccentStyleKey, StringComparison.Ordinal),
                                EditAction = CloneAction(k.Action),
                                EnglishLabel = k.EnglishLabel,
                                EnglishShiftLabel = k.EnglishShiftLabel,
                                FunctionAction = CloneAction(k.FunctionAction),
                                FunctionLabel = k.FunctionLabel,
                                FunctionShiftLabel = k.FunctionShiftLabel,
                                FunctionEnglishLabel = k.FunctionEnglishLabel,
                                FunctionEnglishShiftLabel = k.FunctionEnglishShiftLabel,
                            }).ToList())
                    }).ToList() ?? [])
            }).ToList());
    }

    private static LayoutConfig CloneLayoutConfig(LayoutConfig config) =>
        new(config.Name, null,
            config.Columns?.Select(column =>
                new KeyColumn(column.Gap,
                    column.Rows?.Select(row =>
                        new KeyRow(row.Keys.Select(slot =>
                            new KeySlot(slot.Label, slot.ShiftLabel, CloneAction(slot.Action), slot.Width,
                                slot.Height, slot.StyleKey, slot.GapBefore, slot.EnglishLabel, slot.EnglishShiftLabel,
                                CloneAction(slot.FunctionAction), slot.FunctionLabel, slot.FunctionShiftLabel,
                                slot.FunctionEnglishLabel, slot.FunctionEnglishShiftLabel)).ToList()
                        )).ToList() ?? []
                )).ToList() ?? []);

    private static KeyAction? CloneAction(KeyAction? action) => action switch
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
        _ => action
    };

    private bool SnapshotEquals(LayoutEditorSnapshot left, LayoutEditorSnapshot right) =>
        string.Equals(SerializeSnapshot(left), SerializeSnapshot(right), StringComparison.Ordinal);

    private string SerializeSnapshot(LayoutEditorSnapshot snapshot) =>
        JsonSerializer.Serialize(snapshot, _snapshotJsonOptions);

    private LayoutConfig BuildLayoutConfig() =>
        new(LayoutName, null, Columns.Select(c => c.ToKeyColumn()).ToList());

    /// <summary>
    /// text.
    /// </summary>
    private void NormalizeSharedRowHeights()
    {
        for (int columnIndex = 0; columnIndex < Columns.Count; columnIndex++)
        {
            var column = Columns[columnIndex];
            for (int rowIndex = 0; rowIndex < column.Rows.Count; rowIndex++)
                column.Rows[rowIndex].SharedRowIndex = rowIndex;
        }

        foreach (var entry in BuildSharedRowHeightMap())
        {
            foreach (var row in FindRowsBySharedIndex(entry.Key))
                row.ApplyHeight(entry.Value);
        }
    }

    private void ApplySharedHeightToRowBand(EditableKeyRowVm sourceRow, double heightRatio)
    {
        foreach (var row in FindRowsBySharedIndex(sourceRow.SharedRowIndex))
            row.ApplyHeight(heightRatio);
    }

    private IEnumerable<EditableKeyRowVm> FindRowsBySharedIndex(int sharedRowIndex) =>
        Columns.SelectMany(column => column.Rows)
            .Where(row => row.SharedRowIndex == sharedRowIndex);

    private Dictionary<int, double> BuildSharedRowHeightMap()
    {
        var result = new Dictionary<int, double>();

        foreach (var row in Columns.SelectMany(column => column.Rows))
        {
            var normalizedHeight = NormalizeHeight(row.HeightRatio);
            if (!result.TryGetValue(row.SharedRowIndex, out var existing)
                || normalizedHeight < existing)
            {
                result[row.SharedRowIndex] = normalizedHeight;
            }
        }

        return result;
    }

    private static Dictionary<int, double> BuildSharedRowHeights(LayoutConfig config)
    {
        var result = new Dictionary<int, double>();
        if (config.Columns is null)
            return result;

        foreach (var column in config.Columns)
        {
            if (column.Rows is null)
                continue;

            for (int rowIndex = 0; rowIndex < column.Rows.Count; rowIndex++)
            {
                var row = column.Rows[rowIndex];
                var rowHeight = row.Keys.Count > 0
                    ? NormalizeHeight(row.Keys[0].Height)
                    : EditableKeySlotVm.DefaultHeightRatio;

                if (!result.TryGetValue(rowIndex, out var existing)
                    || rowHeight < existing)
                {
                    result[rowIndex] = rowHeight;
                }
            }
        }

        return result;
    }

    private static double NormalizeHeight(double heightRatio) =>
        Math.Abs(heightRatio - EditableKeySlotVm.CompactHeightRatio) < 0.001
            ? EditableKeySlotVm.CompactHeightRatio
            : EditableKeySlotVm.DefaultHeightRatio;

    private static int CountSoftAccentKeys(LayoutConfig config) =>
        config.Columns?
            .SelectMany(column => column.Rows ?? [])
            .SelectMany(row => row.Keys)
            .Count(slot => string.Equals(slot.StyleKey, EditableKeySlotVm.SoftAccentStyleKey, StringComparison.Ordinal))
        ?? 0;
}
