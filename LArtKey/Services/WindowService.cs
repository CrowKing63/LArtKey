using System.Windows;
using System.Windows.Media;
using static LArtKey.Platform.Win32;

namespace LArtKey.Services;

/// <summary>
/// [English text] English text.
/// [English text] English text.
/// </summary>
public class WindowService
{
    /// <summary>
    /// T-1.3: WS_EX_NOACTIVATE English text — English text.
    /// </summary>
    public void ApplyNoActivate(IntPtr hwnd)
    {
        int exStyle = GetWindowLong(hwnd, GWL_EXSTYLE);
        SetWindowLong(hwnd, GWL_EXSTYLE,
            exStyle | WS_EX_NOACTIVATE);
    }

    /// <summary>
    /// T-1.4: English text).
    /// </summary>
    public void ApplyBackground(Window window)
    {
        // English text.
        window.Background = System.Windows.Media.Brushes.Transparent;
    }

    /// <summary>
    /// T-1.8: English text — WPF Topmost + SetWindowPos English text.
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
