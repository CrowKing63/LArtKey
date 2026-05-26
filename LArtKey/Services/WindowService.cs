using System.Windows;
using System.Windows.Media;
using static LArtKey.Platform.Win32;

namespace LArtKey.Services;

/// <summary>
/// [text] text.
/// [text] text.
/// </summary>
public class WindowService
{
    /// <summary>
    /// T-1.3: WS_EX_NOACTIVATE text — text.
    /// </summary>
    public void ApplyNoActivate(IntPtr hwnd)
    {
        int exStyle = GetWindowLong(hwnd, GWL_EXSTYLE);
        SetWindowLong(hwnd, GWL_EXSTYLE,
            exStyle | WS_EX_NOACTIVATE);
    }

    /// <summary>
    /// T-1.4: text).
    /// </summary>
    public void ApplyBackground(Window window)
    {
        // text.
        window.Background = System.Windows.Media.Brushes.Transparent;
    }

    /// <summary>
    /// T-1.8: text — WPF Topmost + SetWindowPos text.
    /// </summary>
    public void SetTopmost(Window window, bool topmost)
    {
        window.Topmost = topmost;

        var hwnd = new System.Windows.Interop.WindowInteropHelper(window).Handle;
        SetWindowPos(hwnd,
            topmost ? HWND_TOPMOST : HWND_NOTOPMOST,
            0, 0, 0, 0, SWP_NOMOVE | SWP_NOSIZE | SWP_NOACTIVATE);
    }
}
