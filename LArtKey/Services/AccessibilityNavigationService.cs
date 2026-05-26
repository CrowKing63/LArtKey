using System.Runtime.InteropServices;
using LArtKey.Models;
using LArtKey.Platform;
using LArtKey.ViewModels;
using WpfApp = System.Windows.Application;

namespace LArtKey.Services;

/// <summary>
/// [text] text.
/// [text] text.
/// </summary>
public sealed class AccessibilityNavigationService : IDisposable
{
    private readonly ConfigService _configService;
    private readonly MainViewModel _mainViewModel;
    private readonly Win32.LowLevelKeyboardProc _proc;
    private readonly HashSet<uint> _pressedKeys = [];

    private IntPtr _hookHandle = IntPtr.Zero;

    public AccessibilityNavigationService(
        ConfigService configService,
        MainViewModel mainViewModel)
    {
        _configService = configService;
        _mainViewModel = mainViewModel;
        _proc = HookProc;
    }

    public void Start()
    {
        if (_hookHandle != IntPtr.Zero) return;
        _hookHandle = Win32.SetWindowsHookEx(Win32.WH_KEYBOARD_LL, _proc, IntPtr.Zero, 0);
    }

    private IntPtr HookProc(int nCode, IntPtr wParam, IntPtr lParam)
    {
        if (nCode < 0)
            return Win32.CallNextHookEx(_hookHandle, nCode, wParam, lParam);

        int msg = wParam.ToInt32();
        bool isKeyDown = msg is Win32.WM_KEYDOWN or Win32.WM_SYSKEYDOWN;
        bool isKeyUp = msg is Win32.WM_KEYUP or Win32.WM_SYSKEYUP;
        if (!isKeyDown && !isKeyUp)
            return Win32.CallNextHookEx(_hookHandle, nCode, wParam, lParam);

        var info = Marshal.PtrToStructure<Win32.KBDLLHOOKSTRUCT>(lParam);
        ulong extraInfo = unchecked((ulong)info.dwExtraInfo.ToInt64());
        if (extraInfo == Win32.INPUT_EXTRAINFO_LARTKEY)
            return Win32.CallNextHookEx(_hookHandle, nCode, wParam, lParam);

        uint vk = info.vkCode;
        bool wasDown = _pressedKeys.Contains(vk);

        if (isKeyDown)
            _pressedKeys.Add(vk);
        else
            _pressedKeys.Remove(vk);

        if (!IsMainWindowVisible())
            return Win32.CallNextHookEx(_hookHandle, nCode, wParam, lParam);

        // L3: text.
        if (_configService.Current.SwitchScanEnabled)
        {
            if (TryMapSwitchScanAction(vk, out var action))
            {
                if (isKeyDown && !wasDown)
                {
                    WpfApp.Current.Dispatcher.Invoke(() =>
                    {
                        switch (action)
                        {
                            case SwitchScanAction.Next:
                                _mainViewModel.Keyboard.AdvanceScan();
                                break;
                            case SwitchScanAction.Previous:
                                _mainViewModel.Keyboard.ReverseScan();
                                break;
                            case SwitchScanAction.Select:
                                _mainViewModel.Keyboard.SelectScanTarget();
                                break;
                            case SwitchScanAction.Pause:
                                _mainViewModel.Keyboard.ToggleScanPaused();
                                break;
                        }
                    });
                }

                // text.
                return (IntPtr)1;
            }
        }

        if (!_configService.Current.KeyboardA11yNavigationEnabled)
            return Win32.CallNextHookEx(_hookHandle, nCode, wParam, lParam);

        if (IsExitKey(vk) && isKeyDown && !wasDown)
        {
            WpfApp.Current.Dispatcher.Invoke(() => _mainViewModel.Keyboard.ClearA11yFocus());
            return (IntPtr)1;
        }

        if (vk is (uint)VirtualKeyCode.VK_TAB
            or (uint)VirtualKeyCode.VK_RETURN
            or (uint)VirtualKeyCode.VK_SPACE)
        {
            if (isKeyDown)
            {
                bool isRepeat = wasDown;
                if (!isRepeat)
                {
                    WpfApp.Current.Dispatcher.Invoke(() =>
                    {
                        if (vk == (uint)VirtualKeyCode.VK_TAB)
                        {
                            _mainViewModel.Keyboard.MoveA11yFocus(IsShiftPressed());
                        }
                        else
                        {
                            _mainViewModel.Keyboard.ActivateA11yFocused();
                        }
                    });
                }
            }

            // text.
            return (IntPtr)1;
        }

        return Win32.CallNextHookEx(_hookHandle, nCode, wParam, lParam);
    }

    private bool IsMainWindowVisible()
    {
        return WpfApp.Current?.MainWindow?.IsVisible == true;
    }

    private bool IsShiftPressed()
    {
        return _pressedKeys.Contains((uint)VirtualKeyCode.VK_SHIFT)
            || _pressedKeys.Contains((uint)VirtualKeyCode.VK_LSHIFT)
            || _pressedKeys.Contains((uint)VirtualKeyCode.VK_RSHIFT);
    }

    private enum SwitchScanAction
    {
        Next,
        Previous,
        Select,
        Pause,
    }

    private bool TryMapSwitchScanAction(uint vk, out SwitchScanAction action)
    {
        action = default;
        var c = _configService.Current;

        if (MatchesConfiguredKey(c.SwitchScanNextKey, vk))
        {
            action = SwitchScanAction.Next;
            return true;
        }
        if (MatchesConfiguredKey(c.SwitchScanSelectKey, vk) || MatchesConfiguredKey(c.SwitchScanSecondarySelectKey, vk))
        {
            action = SwitchScanAction.Select;
            return true;
        }
        if (MatchesConfiguredKey(c.SwitchScanPreviousKey, vk))
        {
            action = SwitchScanAction.Previous;
            return true;
        }
        if (MatchesConfiguredKey(c.SwitchScanPauseKey, vk))
        {
            action = SwitchScanAction.Pause;
            return true;
        }
        return false;
    }

    // [text] text.
    private static bool MatchesConfiguredKey(string keyName, uint vk)
    {
        if (string.IsNullOrWhiteSpace(keyName))
            return false;
        if (!Enum.TryParse<VirtualKeyCode>(keyName.Trim(), ignoreCase: true, out var parsed))
            return false;
        return (uint)parsed == vk;
    }

    private bool IsExitKey(uint vk)
    {
        string configured = _configService.Current.KeyboardA11yExitKey;
        if (!Enum.TryParse<VirtualKeyCode>(configured, ignoreCase: true, out var exitVk))
            exitVk = VirtualKeyCode.VK_ESCAPE;
        return vk == (uint)exitVk;
    }

    public void Dispose()
    {
        if (_hookHandle != IntPtr.Zero)
        {
            Win32.UnhookWindowsHookEx(_hookHandle);
            _hookHandle = IntPtr.Zero;
        }
    }
}
