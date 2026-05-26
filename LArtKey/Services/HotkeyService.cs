using System.Windows.Interop;
using LArtKey.Platform;

namespace LArtKey.Services;

/// T-5.7 / T-5.8: text
/// <summary>
/// [text] text.
/// [text] text "Ctrl+Alt+K" text.
/// </summary>
public class HotkeyService : IDisposable
{
    private const int HOTKEY_ID = 9001; // text.
    private HwndSource? _source;

    public event Action? HotkeyPressed;

    // ── text ────────────────────────────────────────────────────────────────

    public void Register(IntPtr hwnd, uint modifiers, uint vk)
    {
        _source ??= HwndSource.FromHwnd(hwnd);
        _source.AddHook(HwndHook);
        Win32.RegisterHotKey(hwnd, HOTKEY_ID, modifiers, vk);
    }

    public void Reregister(IntPtr hwnd, string hotkeyString)
    {
        if (_source is not null)
            Win32.UnregisterHotKey(_source.Handle, HOTKEY_ID);

        var (mods, vk) = ParseHotkey(hotkeyString);
        Register(hwnd, mods, vk);
    }

    // ── WndProc text ──────────────────────────────────────────────────────────

    private IntPtr HwndHook(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
    {
        if (msg == 0x0312 && wParam.ToInt32() == HOTKEY_ID) // WM_HOTKEY
        {
            HotkeyPressed?.Invoke();
            handled = true;
        }
        return IntPtr.Zero;
    }

    // ── T-5.8: "Ctrl+Alt+K" text ──────────────────────────────────────

    public static (uint modifiers, uint vk) ParseHotkey(string hotkey)
    {
        uint mods = 0;
        var parts = hotkey.Split('+').Select(s => s.Trim()).ToList();

        if (parts.Contains("Ctrl",  StringComparer.OrdinalIgnoreCase)) mods |= 0x0002; // MOD_CONTROL
        if (parts.Contains("Alt",   StringComparer.OrdinalIgnoreCase)) mods |= 0x0001; // MOD_ALT
        if (parts.Contains("Shift", StringComparer.OrdinalIgnoreCase)) mods |= 0x0004; // MOD_SHIFT
        if (parts.Contains("Win",   StringComparer.OrdinalIgnoreCase)) mods |= 0x0008; // MOD_WIN

        var keyStr = parts.Last();
        uint vk = keyStr.Length == 1 ? (uint)char.ToUpper(keyStr[0]) : 0;
        return (mods, vk);
    }

    // ── text ────────────────────────────────────────────────────────────────

    public void Dispose()
    {
        if (_source is not null)
        {
            Win32.UnregisterHotKey(_source.Handle, HOTKEY_ID);
            _source.RemoveHook(HwndHook);
        }
    }
}
