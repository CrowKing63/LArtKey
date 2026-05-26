using Microsoft.Win32;
using System.Diagnostics;

namespace LAltKey.Services;

/// <summary>
/// Manages the current-user Windows startup entry for LAltKey.
/// </summary>
public class StartupService
{
    private const string RegPath = @"Software\Microsoft\Windows\CurrentVersion\Run";
    private const string AppName = "LAltKey";

    public bool IsEnabled
    {
        get
        {
            using var key = Registry.CurrentUser.OpenSubKey(RegPath, writable: false);
            if (key?.GetValue(AppName) is not string rawValue) return false;
            // text("path")text
            var normalizedValue = rawValue.Trim('"');
            return normalizedValue.Equals(ExePath, StringComparison.OrdinalIgnoreCase);
        }
    }

    public void Enable()
    {
        using var key = Registry.CurrentUser.OpenSubKey(RegPath, writable: true)
            ?? throw new InvalidOperationException("Could not open the Windows startup registry key.");
        key.SetValue(AppName, $"\"{ExePath}\"");
    }

    public void Disable()
    {
        using var key = Registry.CurrentUser.OpenSubKey(RegPath, writable: true);
        key?.DeleteValue(AppName, throwOnMissingValue: false);
    }

    private static string ExePath
    {
        get
        {
            var processPath = Environment.ProcessPath;
            if (!string.IsNullOrEmpty(processPath))
                return processPath;

            try
            {
                return Process.GetCurrentProcess().MainModule?.FileName
                    ?? throw new InvalidOperationException("Could not resolve the LAltKey executable path.");
            }
            catch
            {
                throw new InvalidOperationException("Could not resolve the LAltKey executable path.");
            }
        }
    }
}
