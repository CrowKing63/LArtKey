using Microsoft.Win32;
using System.Diagnostics;

namespace LArtKey.Services;

/// <summary>
/// [English text] English text.
/// [English text] English text.
/// </summary>
public class StartupService
{
    private const string RegPath = @"Software\Microsoft\Windows\CurrentVersion\Run"; // English text.
    private const string AppName = "LArtKey";

    public bool IsEnabled
    {
        get
        {
            using var key = Registry.CurrentUser.OpenSubKey(RegPath, writable: false);
            if (key?.GetValue(AppName) is not string rawValue) return false;
            // English text("path")English text
            var normalizedValue = rawValue.Trim('"');
            return normalizedValue.Equals(ExePath, StringComparison.OrdinalIgnoreCase);
        }
    }

    public void Enable()
    {
        using var key = Registry.CurrentUser.OpenSubKey(RegPath, writable: true)
            ?? throw new InvalidOperationException("English text.");
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
            // single-file publish English text
            var processPath = Environment.ProcessPath;
            if (!string.IsNullOrEmpty(processPath))
                return processPath;

            // English text
            try
            {
                return Process.GetCurrentProcess().MainModule?.FileName
                    ?? throw new InvalidOperationException("English text.");
            }
            catch
            {
                throw new InvalidOperationException("English text.");
            }
        }
    }
}
