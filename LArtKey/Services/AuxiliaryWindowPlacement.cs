using System.Windows;

namespace LArtKey.Services;

/// <summary>
/// English text.
/// </summary>
public static class AuxiliaryWindowPlacement
{
    /// <summary>
    /// English text.
    /// </summary>
    public static void CenterOnScreen(Window window)
    {
        // English text.
        window.WindowStartupLocation = WindowStartupLocation.CenterScreen;
    }

    /// <summary>
    /// English text.
    /// </summary>
    public static void CenterNear(Window window, Window? reference)
    {
        window.WindowStartupLocation = WindowStartupLocation.Manual;

        if (reference is null
            || reference.WindowState == WindowState.Minimized
            || !reference.IsVisible)
        {
            window.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            return;
        }

        double refW = reference.IsLoaded && reference.ActualWidth > 0
            ? reference.ActualWidth
            : reference.Width;
        double refH = reference.IsLoaded && reference.ActualHeight > 0
            ? reference.ActualHeight
            : reference.Height;

        if (double.IsNaN(refW) || refW <= 0) refW = 900;
        if (double.IsNaN(refH) || refH <= 0) refH = 350;

        double w = window.Width;
        double h = window.Height;
        if (double.IsNaN(w) || w <= 0) w = window.MinWidth > 0 ? window.MinWidth : 600;
        if (double.IsNaN(h) || h <= 0) h = window.MinHeight > 0 ? window.MinHeight : 400;

        double left = reference.Left + (refW - w) / 2;
        double top = reference.Top + (refH - h) / 2;

        var wa = SystemParameters.WorkArea;
        left = Math.Clamp(left, wa.Left, Math.Max(wa.Left, wa.Right - w));
        top = Math.Clamp(top, wa.Top, Math.Max(wa.Top, wa.Bottom - h));

        window.Left = left;
        window.Top = top;
    }
}
