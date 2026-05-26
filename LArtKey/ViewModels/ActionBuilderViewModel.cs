using System.Collections.ObjectModel;
using System.Diagnostics;
using LArtKey.Models;
using LArtKey.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using WpfDialog = Microsoft.Win32;

namespace LArtKey.ViewModels;

/// string TwoWay English text)
public partial class ObservableString : ObservableObject
{
    [ObservableProperty] private string _value;

    public ObservableString(string value) => Value = value;

    public override string ToString() => Value;
}

/// VK English text)
public record VkCodeItem(string VkCode, string DisplayName);

/// T-9.2: English text VM — ActionBuilderView English text
public partial class ActionBuilderViewModel : ObservableObject
{
    // ── VK English text → English text ─────────────────────────────────────────────────
    public static Dictionary<string, string> KeyDisplayNameMap { get; } = new(StringComparer.OrdinalIgnoreCase)
    {
        // English text
        ["VK_A"] = "A", ["VK_B"] = "B", ["VK_C"] = "C", ["VK_D"] = "D",
        ["VK_E"] = "E", ["VK_F"] = "F", ["VK_G"] = "G", ["VK_H"] = "H",
        ["VK_I"] = "I", ["VK_J"] = "J", ["VK_K"] = "K", ["VK_L"] = "L",
        ["VK_M"] = "M", ["VK_N"] = "N", ["VK_O"] = "O", ["VK_P"] = "P",
        ["VK_Q"] = "Q", ["VK_R"] = "R", ["VK_S"] = "S", ["VK_T"] = "T",
        ["VK_U"] = "U", ["VK_V"] = "V", ["VK_W"] = "W", ["VK_X"] = "X",
        ["VK_Y"] = "Y", ["VK_Z"] = "Z",
        // English text
        ["VK_0"] = "0", ["VK_1"] = "1", ["VK_2"] = "2", ["VK_3"] = "3",
        ["VK_4"] = "4", ["VK_5"] = "5", ["VK_6"] = "6", ["VK_7"] = "7",
        ["VK_8"] = "8", ["VK_9"] = "9",
        // English text
        ["VK_F1"] = "F1", ["VK_F2"] = "F2", ["VK_F3"] = "F3", ["VK_F4"] = "F4",
        ["VK_F5"] = "F5", ["VK_F6"] = "F6", ["VK_F7"] = "F7", ["VK_F8"] = "F8",
        ["VK_F9"] = "F9", ["VK_F10"] = "F10", ["VK_F11"] = "F11", ["VK_F12"] = "F12",
        // English text
        ["VK_RETURN"] = "Enter", ["VK_SPACE"] = "Space", ["VK_TAB"] = "Tab",
        ["VK_BACK"] = "Backspace", ["VK_ESCAPE"] = "Esc",
        // English text
        ["VK_SHIFT"] = "Shift", ["VK_CONTROL"] = "Ctrl", ["VK_MENU"] = "Alt",
        ["VK_LSHIFT"] = "LShift", ["VK_RSHIFT"] = "RShift",
        ["VK_LCONTROL"] = "LCtrl", ["VK_RCONTROL"] = "RCtrl",
        ["VK_LMENU"] = "LAlt", ["VK_RMENU"] = "RAlt",
        ["VK_LWIN"] = "Win", ["VK_RWIN"] = "RWin",
        // English text
        ["VK_LEFT"] = "←", ["VK_UP"] = "↑", ["VK_RIGHT"] = "→", ["VK_DOWN"] = "↓",
        // English text
        ["VK_HOME"] = "Home", ["VK_END"] = "End", ["VK_PRIOR"] = "PageUp", ["VK_NEXT"] = "PageDown",
        ["VK_INSERT"] = "Insert", ["VK_DELETE"] = "Delete",
        // English text
        ["VK_HANGUL"] = "English text", ["VK_HANJA"] = "English text", ["VK_CAPITAL"] = "CapsLock",
        // English text
        ["VK_NUMLOCK"] = "NumLock", ["VK_SCROLL"] = "ScrollLock",
        ["VK_OEM_3"] = "`", ["VK_SNAPSHOT"] = "Print", ["VK_OEM_5"] = "\\",
        ["VK_OEM_MINUS"] = "-", ["VK_OEM_PLUS"] = "=", ["VK_OEM_4"] = "[",
        ["VK_OEM_6"] = "]", ["VK_OEM_1"] = ";", ["VK_OEM_7"] = "'",
        ["VK_OEM_PERIOD"] = ".", ["VK_OEM_2"] = "/", ["VK_OEM_COMMA"] = ",",
    };

    // ── English text ─────────────────────────────────────────────────────────
    public static IReadOnlyList<string> ActionTypes { get; } =
    [
        "SendKey", "SendCombo", "ToggleSticky", "SwitchLayout",
        "RunApp", "Boilerplate", "ShellCommand", "VolumeControl", "ClipboardPaste",
        "ToggleInputMode", "ToggleFunctionLayer", "Ai"
    ];

    public static IReadOnlyList<string> ShellTypes  { get; } = ["cmd", "powershell"];
    public static IReadOnlyList<string> VolumeDirections { get; } = ["up", "down", "mute"];

    public static IReadOnlyList<string> CommonVkCodes { get; } =
    [
        "VK_A", "VK_B", "VK_C", "VK_D", "VK_E", "VK_F", "VK_G",
        "VK_H", "VK_I", "VK_J", "VK_K", "VK_L", "VK_M", "VK_N",
        "VK_O", "VK_P", "VK_Q", "VK_R", "VK_S", "VK_T", "VK_U",
        "VK_V", "VK_W", "VK_X", "VK_Y", "VK_Z",
        "VK_OEM_3", "VK_1", "VK_2", "VK_3", "VK_4",
        "VK_5", "VK_6", "VK_7", "VK_8", "VK_9", "VK_0", "VK_OEM_MINUS", "VK_OEM_PLUS",
        "VK_OEM_4", "VK_OEM_6", "VK_OEM_5", "VK_OEM_1", "VK_OEM_7", "VK_OEM_COMMA", "VK_OEM_PERIOD", "VK_OEM_2",
        "VK_F1", "VK_F2", "VK_F3", "VK_F4", "VK_F5", "VK_F6",
        "VK_F7", "VK_F8", "VK_F9", "VK_F10", "VK_F11", "VK_F12",
        "VK_RETURN", "VK_SPACE", "VK_TAB", "VK_BACK", "VK_ESCAPE",
        "VK_SHIFT", "VK_CONTROL", "VK_MENU",
        "VK_LEFT", "VK_UP", "VK_RIGHT", "VK_DOWN",
        "VK_HOME", "VK_END", "VK_PRIOR", "VK_NEXT",
        "VK_INSERT", "VK_DELETE", "VK_NUMLOCK", "VK_SCROLL", "VK_SNAPSHOT",
        "VK_HANGUL", "VK_HANJA", "VK_CAPITAL",
        "VK_LWIN", "VK_RWIN", "VK_LSHIFT", "VK_RSHIFT",
        "VK_LCONTROL", "VK_RCONTROL", "VK_LMENU", "VK_RMENU"
    ];

    public static IReadOnlyList<VkCodeItem> CommonVkCodesDisplay { get; } =
        CommonVkCodes.Select(vk => new VkCodeItem(vk,
            KeyDisplayNameMap.TryGetValue(vk, out var name) ? name : vk.Replace("VK_", "")))
        .ToList();

    // ── English text ───────────────────────────────────────────────────────
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsSendKey))]
    [NotifyPropertyChangedFor(nameof(IsSendCombo))]
    [NotifyPropertyChangedFor(nameof(IsToggleSticky))]
    [NotifyPropertyChangedFor(nameof(IsSwitchLayout))]
    [NotifyPropertyChangedFor(nameof(IsRunApp))]
    [NotifyPropertyChangedFor(nameof(IsBoilerplate))]
    [NotifyPropertyChangedFor(nameof(IsShellCommand))]
    [NotifyPropertyChangedFor(nameof(IsVolumeControl))]
    [NotifyPropertyChangedFor(nameof(IsClipboardPaste))]
    [NotifyPropertyChangedFor(nameof(IsToggleInputMode))]
    [NotifyPropertyChangedFor(nameof(IsToggleFunctionLayer))]
    [NotifyPropertyChangedFor(nameof(IsAi))]
    private string selectedActionType = "SendKey";

    // ── Visibility English text ───────────────────────────────────────────────
    public bool IsSendKey              => SelectedActionType == "SendKey";
    public bool IsSendCombo            => SelectedActionType == "SendCombo";
    public bool IsToggleSticky         => SelectedActionType == "ToggleSticky";
    public bool IsSwitchLayout         => SelectedActionType == "SwitchLayout";
    public bool IsRunApp               => SelectedActionType == "RunApp";
    public bool IsBoilerplate          => SelectedActionType == "Boilerplate";
    public bool IsShellCommand         => SelectedActionType == "ShellCommand";
    public bool IsVolumeControl        => SelectedActionType == "VolumeControl";
    public bool IsClipboardPaste       => SelectedActionType == "ClipboardPaste";
    public bool IsToggleInputMode  => SelectedActionType == "ToggleInputMode";
    public bool IsToggleFunctionLayer  => SelectedActionType == "ToggleFunctionLayer";
    public bool IsAi                   => SelectedActionType == "Ai";

    // ── English text ─────────────────────────────────────────────────────

    // SendKey
    [ObservableProperty] private string sendKeyVk = "VK_A";

    // SendCombo (English text)
    [ObservableProperty] private ObservableCollection<ObservableString> sendComboKeysCollection = 
        [new ObservableString("VK_CONTROL"), new ObservableString("VK_C")];

    // ToggleSticky
    [ObservableProperty] private string toggleStickyVk = "VK_SHIFT";

    // SwitchLayout
    [ObservableProperty] private string switchLayoutName = "";

    // RunApp
    [ObservableProperty] private string appPath = "";
    [ObservableProperty] private string appArgs = "";

    // Boilerplate
    [ObservableProperty] private string boilerplateText = "";

    // ShellCommand
    [ObservableProperty] private string shellCmd = "";
    [ObservableProperty] private string selectedShell = "cmd";

    // VolumeControl
    [ObservableProperty] private string volumeDirection = "up";
    [ObservableProperty] private int    volumeStep      = 5;

    // ClipboardPaste
    [ObservableProperty] private string clipboardText = "";

    // Ai
    [ObservableProperty] private string aiPrompt = "";

    // ── English text ───────────────────────────────────────────────
    public void LoadFromAction(KeyAction? action)
    {
        switch (action)
        {
            case SendKeyAction a:
                SelectedActionType = "SendKey";
                SendKeyVk = a.Vk;
                break;
            case SendComboAction a:
                SelectedActionType = "SendCombo";
                SendComboKeysCollection = new ObservableCollection<ObservableString>(
                    a.Keys.Select(k => new ObservableString(k)));
                break;
            case ToggleStickyAction a:
                SelectedActionType = "ToggleSticky";
                ToggleStickyVk = a.Vk;
                break;
            case SwitchLayoutAction a:
                SelectedActionType = "SwitchLayout";
                SwitchLayoutName = a.Name;
                break;
            case RunAppAction a:
                SelectedActionType = "RunApp";
                AppPath = a.Path;
                AppArgs = a.Args;
                break;
            case BoilerplateAction a:
                SelectedActionType = "Boilerplate";
                BoilerplateText = a.Text;
                break;
            case ShellCommandAction a:
                SelectedActionType = "ShellCommand";
                ShellCmd = a.Command;
                SelectedShell = a.Shell;
                break;
            case VolumeControlAction a:
                SelectedActionType = "VolumeControl";
                VolumeDirection = a.Direction;
                VolumeStep = a.Step;
                break;
            case ClipboardPasteAction a:
                SelectedActionType = "ClipboardPaste";
                ClipboardText = a.Text;
                break;
            case ToggleInputModeAction:
                SelectedActionType = "ToggleInputMode";
                break;
            case ToggleFunctionLayerAction:
                SelectedActionType = "ToggleFunctionLayer";
                break;
            case AiAction a:
                SelectedActionType = "Ai";
                AiPrompt = a.Prompt;
                break;
            default:
                SelectedActionType = "SendKey";
                break;
        }
    }

    // ── English text ────────────────────────────────────────
    public KeyAction? BuildAction() => SelectedActionType switch
    {
        "SendKey"      => new SendKeyAction(SendKeyVk.Trim()),
        "SendCombo"    => new SendComboAction(
            SendComboKeysCollection.Where(s => !string.IsNullOrWhiteSpace(s.Value))
                .Select(s => s.Value).ToList()),
        "ToggleSticky" => new ToggleStickyAction(ToggleStickyVk.Trim()),
        "SwitchLayout" => new SwitchLayoutAction(SwitchLayoutName.Trim()),
        "RunApp"       => new RunAppAction(AppPath.Trim(), AppArgs.Trim()),
        "Boilerplate"  => new BoilerplateAction(BoilerplateText),
        "ShellCommand" => new ShellCommandAction(ShellCmd.Trim(), SelectedShell),
        "VolumeControl"=> new VolumeControlAction(VolumeDirection, VolumeStep),
        "ClipboardPaste"      => new ClipboardPasteAction(ClipboardText),
        "ToggleInputMode" => new ToggleInputModeAction(),
        "ToggleFunctionLayer" => new ToggleFunctionLayerAction(),
        "Ai"                  => new AiAction(AiPrompt),
        _                     => null
    };

    // ── RunApp English text ───────────────────────────────────────────────────
    [RelayCommand]
    private void BrowseApp()
    {
        var dlg = new WpfDialog.OpenFileDialog
        {
            Filter = "English text|*.exe|English text|*.*",
            Title  = "English text"
        };
        if (dlg.ShowDialog() == true)
            AppPath = dlg.FileName;
    }

    partial void OnSendKeyVkChanged(string value)
    {
        if (string.IsNullOrWhiteSpace(value) || value.StartsWith("VK_", StringComparison.OrdinalIgnoreCase))
            return;
        var (isCombo, keys) = KeyNotationParser.Parse(value);
        if (keys.Count > 0 && !isCombo && keys[0] != value)
            SendKeyVk = keys[0];
    }

    partial void OnToggleStickyVkChanged(string value)
    {
        if (string.IsNullOrWhiteSpace(value) || value.StartsWith("VK_", StringComparison.OrdinalIgnoreCase))
            return;
        var (isCombo, keys) = KeyNotationParser.Parse(value);
        if (keys.Count > 0 && !isCombo && keys[0] != value)
            ToggleStickyVk = keys[0];
    }

    [RelayCommand]
    private void AddComboKey()
    {
        SendComboKeysCollection.Add(new ObservableString("VK_A"));
    }

    [RelayCommand]
    private void RemoveComboKey(ObservableString key)
    {
        if (SendComboKeysCollection.Count > 1)
            SendComboKeysCollection.Remove(key);
    }

    [RelayCommand]
    private void ClearComboKeys()
    {
        SendComboKeysCollection.Clear();
        SendComboKeysCollection.Add(new ObservableString("VK_A"));
    }

    public bool CanRemoveComboKey => SendComboKeysCollection.Count > 1;
}
