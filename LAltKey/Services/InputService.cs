using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Security.Principal;
using LAltKey.Models;
using LAltKey.Platform;
using WpfApp = System.Windows.Application;

namespace LAltKey.Services;

/// <summary>
/// Sends virtual keys or Unicode text to the target app and tracks transient keyboard state.
/// Sticky modifiers are handled here, and the Fn layer is also kept here so UI and execution share one source of truth.
/// </summary>
public enum InputMode
{
    Unicode,
    VirtualKey
}

public class InputService
{
    // text.
    private static readonly Dictionary<string, SystemShortcutCommand> SystemShortcutCommands = new()
    {
        [NormalizeShortcutKey([
            VirtualKeyCode.VK_CONTROL,
            VirtualKeyCode.VK_SHIFT,
            VirtualKeyCode.VK_ESCAPE
        ])] = SystemShortcutCommand.TaskManager,
    };

    private static readonly uint OwnProcessId = (uint)Environment.ProcessId;
    private static readonly IntPtr InputExtraInfoTag =
        unchecked((IntPtr)(long)Win32.INPUT_EXTRAINFO_LALTKEY);

    // Ctrl/Alt/Win can leave the desktop in a risky state if they stay pressed while the window hides.
    private static readonly HashSet<VirtualKeyCode> HighRiskModifiers =
    [
        VirtualKeyCode.VK_CONTROL,
        VirtualKeyCode.VK_LCONTROL,
        VirtualKeyCode.VK_RCONTROL,
        VirtualKeyCode.VK_MENU,
        VirtualKeyCode.VK_LMENU,
        VirtualKeyCode.VK_RMENU,
        VirtualKeyCode.VK_LWIN,
        VirtualKeyCode.VK_RWIN,
    ];

    private readonly bool _isElevated;
    private readonly HashSet<VirtualKeyCode> _stickyKeys = [];
    private readonly HashSet<VirtualKeyCode> _lockedKeys = [];
    private readonly HashSet<VirtualKeyCode> _heldKeys = [];
    private FunctionLayerState _functionLayerState;
    private VirtualKeyCode? _armedHeldKey;

    private enum SystemShortcutCommand
    {
        TaskManager
    }

    public InputMode Mode { get; private set; }
    public bool IsElevated => _isElevated;
    public int TrackedOnScreenLength { get; set; }
    public FunctionLayerState FunctionLayerState => _functionLayerState;
    public bool IsFunctionLayerActive => _functionLayerState != FunctionLayerState.Inactive;

    public IReadOnlySet<VirtualKeyCode> StickyKeys => _stickyKeys;
    public IReadOnlySet<VirtualKeyCode> LockedKeys => _lockedKeys;
    public IReadOnlySet<VirtualKeyCode> HeldKeys => _heldKeys;

    public event Action<InputMode>? ModeChanged;
    public event Action? InputModeGestureResetRequested;
    public event Action? StickyStateChanged;
    public event Action? ElevatedAppDetected;
    public event Action<KeyAction>? SpecialActionRequested;

    public InputService()
    {
        _isElevated = CheckElevated();
        Mode = _isElevated ? InputMode.VirtualKey : InputMode.Unicode;
    }

    public bool IsForegroundOwnWindow()
    {
        var hwnd = Win32.GetForegroundWindow();
        if (hwnd == IntPtr.Zero)
            return false;

        Win32.GetWindowThreadProcessId(hwnd, out var pid);
        return pid == OwnProcessId;
    }

    public bool TrySetMode(InputMode target)
    {
        if (_isElevated && target == InputMode.Unicode)
            return false;

        if (Mode == target)
            return true;

        Mode = target;
        ResetTransientInputStateForModeChange();
        ModeChanged?.Invoke(Mode);
        InputModeGestureResetRequested?.Invoke();
        return true;
    }

    public void ResetTrackedLength() => TrackedOnScreenLength = 0;
    public void NotifyElevatedApp() => ElevatedAppDetected?.Invoke();
    public bool IsCapsLockOn => (Win32.GetKeyState((int)VirtualKeyCode.VK_CAPITAL) & 0x0001) != 0;
    public bool HasActiveModifiers => _stickyKeys.Count > 0 || _lockedKeys.Count > 0;

    /// <summary>
    /// The Fn layer follows the same 3-step cycle users already know from sticky modifiers.
    /// First press arms one-shot, second press locks, third press clears.
    /// </summary>
    public void ToggleFunctionLayer()
    {
        _functionLayerState = _functionLayerState switch
        {
            FunctionLayerState.Inactive => FunctionLayerState.OneShot,
            FunctionLayerState.OneShot => FunctionLayerState.Locked,
            _ => FunctionLayerState.Inactive
        };

        StickyStateChanged?.Invoke();
    }

    /// <summary>
    /// One-shot Fn should disappear after the next non-Fn key finishes.
    /// Locked Fn stays on until the user presses Fn again.
    /// </summary>
    public void ConsumeFunctionLayerAfterAction()
    {
        if (_functionLayerState != FunctionLayerState.OneShot)
            return;

        _functionLayerState = FunctionLayerState.Inactive;
        StickyStateChanged?.Invoke();
    }

    public void ResetFunctionLayer()
    {
        if (_functionLayerState == FunctionLayerState.Inactive)
            return;

        _functionLayerState = FunctionLayerState.Inactive;
        StickyStateChanged?.Invoke();
    }

    /// <summary>
    /// text.
    /// </summary>
    private void ResetTransientInputStateForModeChange()
    {
        ReleaseAllHeldKeys("mode-change");
        ReleaseTransientModifiers("mode-change");
        ResetTrackedLength();

        if (_functionLayerState == FunctionLayerState.OneShot)
        {
            _functionLayerState = FunctionLayerState.Inactive;
            StickyStateChanged?.Invoke();
        }
    }

    /// <summary>
    /// Shift is excluded because Unicode text input still allows Shift-only input through the composition path.
    /// </summary>
    public bool HasActiveModifiersExcludingShift
    {
        get
        {
            foreach (var vk in _stickyKeys)
            {
                if (vk is not VirtualKeyCode.VK_SHIFT and not VirtualKeyCode.VK_LSHIFT and not VirtualKeyCode.VK_RSHIFT)
                    return true;
            }

            foreach (var vk in _lockedKeys)
            {
                if (vk is not VirtualKeyCode.VK_SHIFT and not VirtualKeyCode.VK_LSHIFT and not VirtualKeyCode.VK_RSHIFT)
                    return true;
            }

            return false;
        }
    }

    public virtual void SendKeyPress(VirtualKeyCode vk)
    {
        var inputs = new Win32.INPUT[] { MakeSendKeyDown(vk), MakeSendKeyUp(vk) };
        DispatchInput(inputs);
    }

    public virtual void SendKeyDown(VirtualKeyCode vk)
    {
        DispatchInput([MakeSendKeyDown(vk)]);
    }

    public virtual void SendKeyUp(VirtualKeyCode vk)
    {
        DispatchInput([MakeSendKeyUp(vk)]);
    }

    /// <summary>
    /// text.
    /// </summary>
    protected virtual bool TryHandleSystemShortcut(IReadOnlyList<VirtualKeyCode> keys)
    {
        if (!SystemShortcutCommands.TryGetValue(NormalizeShortcutKey(keys), out var command))
            return false;

        switch (command)
        {
            case SystemShortcutCommand.TaskManager:
                StartProcess(CreateRunAppProcessStartInfo("taskmgr.exe", ""));
                return true;

            default:
                return false;
        }
    }

    /// <summary>
    /// text "text"text.
    /// </summary>
    public void ArmHeldKeyGesture(VirtualKeyCode vk)
    {
        _armedHeldKey = vk;
    }

    /// <summary>
    /// text.
    /// </summary>
    public void CancelHeldKeyGesture(VirtualKeyCode? vk = null)
    {
        if (vk is null || _armedHeldKey == vk)
            _armedHeldKey = null;
    }

    /// <summary>
    /// text.
    /// </summary>
    public virtual void BeginHeldKey(VirtualKeyCode vk)
    {
        if (!_heldKeys.Add(vk))
            return;

        SendKeyDown(vk);
    }

    /// <summary>
    /// BeginHeldKeytext.
    /// </summary>
    public virtual void EndHeldKey(VirtualKeyCode vk)
    {
        if (!_heldKeys.Remove(vk))
            return;

        SendKeyUp(vk);
    }

    /// <summary>
    /// text "text"text.
    /// </summary>
    public virtual void PulseHeldKey(VirtualKeyCode vk)
    {
        if (!_heldKeys.Contains(vk))
            return;

        SendKeyDown(vk);
    }

    /// <summary>
    /// text.
    /// </summary>
    public virtual void ReleaseAllHeldKeys(string reason = "manual")
    {
        CancelHeldKeyGesture();

        foreach (var vk in _heldKeys.ToList())
            SendKeyUp(vk);

        _heldKeys.Clear();
    }

    /// <summary>
    /// text.
    /// </summary>
    public bool IsHeldKey(VirtualKeyCode vk) => _heldKeys.Contains(vk);

    public void ToggleModifier(VirtualKeyCode vk)
    {
        if (_lockedKeys.Contains(vk))
        {
            _lockedKeys.Remove(vk);
            _stickyKeys.Remove(vk);
            SendKeyUp(vk);
        }
        else if (_stickyKeys.Contains(vk))
        {
            _lockedKeys.Add(vk);
        }
        else
        {
            _stickyKeys.Add(vk);
            SendKeyDown(vk);
        }

        StickyStateChanged?.Invoke();
    }

    internal void ReleaseTransientModifiers(string reason = "input-complete")
    {
        var transient = _stickyKeys.Except(_lockedKeys).ToList();
        foreach (var mod in transient)
        {
            SendKeyUp(mod);
            _stickyKeys.Remove(mod);
        }

        if (transient.Count > 0)
            StickyStateChanged?.Invoke();
    }

    public virtual void ReleaseAllModifiers(string reason = "manual")
    {
        var active = _stickyKeys.Union(_lockedKeys).Distinct().ToList();
        foreach (var mod in active)
            SendKeyUp(mod);

        _stickyKeys.Clear();
        _lockedKeys.Clear();
        StickyStateChanged?.Invoke();
    }

    public virtual void ReleaseHighRiskModifiers(string reason)
    {
        var released = _stickyKeys
            .Union(_lockedKeys)
            .Where(IsHighRiskModifier)
            .Distinct()
            .ToList();

        foreach (var mod in released)
        {
            SendKeyUp(mod);
            _stickyKeys.Remove(mod);
            _lockedKeys.Remove(mod);
        }

        if (released.Count > 0)
            StickyStateChanged?.Invoke();
    }

    /// <summary>
    /// text.
    /// </summary>
    protected virtual void StartProcess(ProcessStartInfo psi)
    {
        Process.Start(psi);
    }

    public void HandleAction(KeyAction action)
    {
        switch (action)
        {
            case SendKeyAction { Vk: var vkStr }:
                if (Enum.TryParse<VirtualKeyCode>(vkStr, out var vk))
                {
                    // text "text" text.
                    if (_armedHeldKey == vk)
                    {
                        BeginHeldKey(vk);
                        _armedHeldKey = null;
                    }
                    else
                    {
                        SendKeyPress(vk);
                    }

                    ReleaseTransientModifiers("SendKeyAction");
                }
                break;

            case SendComboAction { Keys: var keys }:
                var vkList = keys
                    .Select(k => Enum.TryParse<VirtualKeyCode>(k, out var v) ? (VirtualKeyCode?)v : null)
                    .Where(v => v.HasValue)
                    .Select(v => v!.Value)
                    .ToList();
                if (TryHandleSystemShortcut(vkList))
                {
                    ReleaseTransientModifiers("SystemShortcut");
                }
                else
                {
                    SendCombo(vkList);
                }
                break;

            case ToggleStickyAction { Vk: var vkStr2 }:
                if (Enum.TryParse<VirtualKeyCode>(vkStr2, out var modVk))
                    ToggleModifier(modVk);
                break;

            case ToggleFunctionLayerAction:
                ToggleFunctionLayer();
                break;

            case AiAction aiAction:
                SpecialActionRequested?.Invoke(aiAction);
                break;

            case RunAppAction { Path: var path, Args: var args }:
                try
                {
                    StartProcess(CreateRunAppProcessStartInfo(path, args));
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"[RunApp] failed: {path} / {ex.Message}");
                }
                break;

            case BoilerplateAction { Text: var bText }:
                SendUnicode(bText);
                break;

            case ShellCommandAction { Command: var cmd, Shell: var shell, Hidden: var hidden }:
                try
                {
                    StartProcess(CreateShellCommandProcessStartInfo(cmd, shell, hidden));
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"[ShellCommand] failed: {cmd} / {ex.Message}");
                }
                break;

            case VolumeControlAction { Direction: var dir, Step: var step }:
                HandleVolumeControl(dir, step);
                break;

            case ClipboardPasteAction { Text: var pasteText }:
                WpfApp.Current.Dispatcher.Invoke(() => ClipboardHelper.SetTextWithRetry(pasteText));
                SendCombo([VirtualKeyCode.VK_CONTROL, VirtualKeyCode.VK_V]);
                break;
        }
    }

    public void SendCombo(List<VirtualKeyCode> keys)
    {
        foreach (var k in keys)
            SendKeyDown(k);
        foreach (var k in Enumerable.Reverse(keys))
            SendKeyUp(k);
        ReleaseTransientModifiers("SendCombo");
    }

    public virtual void SendUnicode(string text)
    {
        var inputs = new List<Win32.INPUT>();
        foreach (var ch in text)
        {
            inputs.Add(MakeUnicodeKeyDown(ch));
            inputs.Add(MakeUnicodeKeyUp(ch));
        }

        DispatchInput(inputs.ToArray());
        ReleaseTransientModifiers("SendUnicode");
    }

    public virtual void SendAtomicReplace(int prevLen, string newOutput)
    {
        var inputs = new List<Win32.INPUT>();
        for (int i = 0; i < prevLen; i++)
        {
            inputs.Add(MakeKeyDown((ushort)VirtualKeyCode.VK_BACK));
            inputs.Add(MakeKeyUp((ushort)VirtualKeyCode.VK_BACK));
        }

        foreach (var ch in newOutput)
        {
            inputs.Add(MakeUnicodeKeyDown(ch));
            inputs.Add(MakeUnicodeKeyUp(ch));
        }

        if (inputs.Count > 0)
            DispatchInput(inputs.ToArray());

        TrackedOnScreenLength = newOutput.Length;
        ReleaseTransientModifiers("SendAtomicReplace");
    }

    private static bool CheckElevated()
    {
        using var identity = WindowsIdentity.GetCurrent();
        var principal = new WindowsPrincipal(identity);
        return principal.IsInRole(WindowsBuiltInRole.Administrator);
    }

    /// <summary>
    /// text.
    /// </summary>
    private static ProcessStartInfo CreateRunAppProcessStartInfo(string path, string args) => new(path)
    {
        Arguments = args,
        UseShellExecute = true
    };

    /// <summary>
    /// text.
    /// </summary>
    private static ProcessStartInfo CreateShellCommandProcessStartInfo(string command, string shell, bool hidden)
    {
        var shellExe = shell == "powershell" ? "powershell.exe" : "cmd.exe";
        var psi = new ProcessStartInfo(shellExe)
        {
            UseShellExecute = false,
            CreateNoWindow = hidden
        };

        if (shell == "powershell")
        {
            psi.ArgumentList.Add("-NoProfile");
            psi.ArgumentList.Add("-Command");
            psi.ArgumentList.Add(command);
            return psi;
        }

        psi.ArgumentList.Add("/d");
        psi.ArgumentList.Add("/s");
        psi.ArgumentList.Add("/c");
        psi.ArgumentList.Add(command);
        return psi;
    }

    internal virtual void DispatchInput(Win32.INPUT[] inputs)
    {
        uint sent = Win32.SendInput((uint)inputs.Length, inputs, Marshal.SizeOf<Win32.INPUT>());
        if (sent == 0 && Marshal.GetLastWin32Error() == Win32.ERROR_ACCESS_DENIED)
            ElevatedAppDetected?.Invoke();
    }

    private void HandleVolumeControl(string direction, int step)
    {
        var vk = direction switch
        {
            "up" => (ushort)0xAF,
            "down" => (ushort)0xAE,
            "mute" => (ushort)0xAD,
            _ => (ushort)0
        };

        if (vk == 0)
            return;

        int repeat = Math.Max(1, step / 2);
        for (int i = 0; i < repeat; i++)
            SendKeyPress((VirtualKeyCode)vk);
    }

    private static bool IsHighRiskModifier(VirtualKeyCode vk) => HighRiskModifiers.Contains(vk);

    /// <summary>
    /// text.
    /// </summary>
    private static string NormalizeShortcutKey(IReadOnlyList<VirtualKeyCode> keys)
        => string.Join("+", keys
            .Select(NormalizeShortcutKeyPart)
            .Distinct()
            .OrderBy(vk => (int)vk)
            .Select(vk => vk.ToString()));

    private static VirtualKeyCode NormalizeShortcutKeyPart(VirtualKeyCode vk) => vk switch
    {
        VirtualKeyCode.VK_LCONTROL or VirtualKeyCode.VK_RCONTROL => VirtualKeyCode.VK_CONTROL,
        VirtualKeyCode.VK_LSHIFT or VirtualKeyCode.VK_RSHIFT => VirtualKeyCode.VK_SHIFT,
        VirtualKeyCode.VK_LMENU or VirtualKeyCode.VK_RMENU => VirtualKeyCode.VK_MENU,
        VirtualKeyCode.VK_RWIN => VirtualKeyCode.VK_LWIN,
        _ => vk
    };

    private Win32.INPUT MakeSendKeyDown(VirtualKeyCode vk)
        => TryMakeScanCodeInput(vk, keyUp: false, out var input)
            ? input
            : MakeKeyDown((ushort)vk);

    private Win32.INPUT MakeSendKeyUp(VirtualKeyCode vk)
        => TryMakeScanCodeInput(vk, keyUp: true, out var input)
            ? input
            : MakeKeyUp((ushort)vk);

    /// <summary>
    /// SendKey text VK_* text.
    /// </summary>
    private bool TryMakeScanCodeInput(VirtualKeyCode vk, bool keyUp, out Win32.INPUT input)
    {
        input = default;
        if (IsScanCodeFallbackKey(vk))
            return false;

        uint mapped = Win32.MapVirtualKey((uint)vk, Win32.MAPVK_VK_TO_VSC_EX);
        if (mapped == 0 || mapped > 0xFFFF)
            return false;

        ushort scanCode = (ushort)(mapped & 0x00FF);
        if (scanCode == 0)
            return false;

        uint flags = Win32.KEYEVENTF_SCANCODE;
        if (keyUp)
            flags |= Win32.KEYEVENTF_KEYUP;
        if ((mapped & 0xFF00) != 0 || IsExtendedScanCodeKey(vk))
            flags |= Win32.KEYEVENTF_EXTENDEDKEY;

        input = MakeScanCodeKey(scanCode, flags);
        return true;
    }

    private static bool IsScanCodeFallbackKey(VirtualKeyCode vk)
    {
        if (vk is
            // IME text.
            VirtualKeyCode.VK_HANGUL or VirtualKeyCode.VK_HANJA or
            // Pausetext.
            VirtualKeyCode.VK_PAUSE or VirtualKeyCode.VK_SNAPSHOT)
        {
            return true;
        }

        ushort rawVk = (ushort)vk;
        return rawVk is
            // text.
            0xAD or 0xAE or 0xAF or
            0xA6 or 0xA7 or 0xA8 or 0xA9 or 0xAA or 0xAB or 0xAC or
            0xB0 or 0xB1 or 0xB2 or 0xB3 or
            0xB4 or 0xB5 or 0xB6 or 0xB7;
    }

    private static bool IsExtendedScanCodeKey(VirtualKeyCode vk) => vk is
        VirtualKeyCode.VK_RCONTROL or VirtualKeyCode.VK_RMENU or
        VirtualKeyCode.VK_INSERT or VirtualKeyCode.VK_DELETE or
        VirtualKeyCode.VK_HOME or VirtualKeyCode.VK_END or
        VirtualKeyCode.VK_PRIOR or VirtualKeyCode.VK_NEXT or
        VirtualKeyCode.VK_LEFT or VirtualKeyCode.VK_UP or
        VirtualKeyCode.VK_RIGHT or VirtualKeyCode.VK_DOWN or
        VirtualKeyCode.VK_LWIN or VirtualKeyCode.VK_RWIN;

    private static Win32.INPUT MakeUnicodeKeyDown(char ch) => new()
    {
        Type = Win32.INPUT_KEYBOARD,
        U = new() { Ki = new() { WVk = 0, WScan = ch, DwFlags = Win32.KEYEVENTF_UNICODE, DwExtraInfo = InputExtraInfoTag } }
    };

    private static Win32.INPUT MakeUnicodeKeyUp(char ch) => new()
    {
        Type = Win32.INPUT_KEYBOARD,
        U = new() { Ki = new() { WVk = 0, WScan = ch, DwFlags = Win32.KEYEVENTF_UNICODE | Win32.KEYEVENTF_KEYUP, DwExtraInfo = InputExtraInfoTag } }
    };

    private static Win32.INPUT MakeKeyDown(ushort vk) => new()
    {
        Type = Win32.INPUT_KEYBOARD,
        U = new() { Ki = new() { WVk = vk, DwExtraInfo = InputExtraInfoTag } }
    };

    private static Win32.INPUT MakeKeyUp(ushort vk) => new()
    {
        Type = Win32.INPUT_KEYBOARD,
        U = new() { Ki = new() { WVk = vk, DwFlags = Win32.KEYEVENTF_KEYUP, DwExtraInfo = InputExtraInfoTag } }
    };

    private static Win32.INPUT MakeScanCodeKey(ushort scanCode, uint flags) => new()
    {
        Type = Win32.INPUT_KEYBOARD,
        U = new() { Ki = new() { WVk = 0, WScan = scanCode, DwFlags = flags, DwExtraInfo = InputExtraInfoTag } }
    };
}
