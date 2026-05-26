using System.Diagnostics;
using System.IO;

namespace LArtKey.Services;

/// <summary>
/// [text] Windows text.
/// [text] text.
/// </summary>
public class OskLauncherService
{
    /// <summary>
    /// OSK text.
    /// </summary>
    /// <returns>text false</returns>
    public virtual bool TryLaunch()
    {
        foreach (var candidate in EnumerateCandidates())
        {
            if (TryLaunchCandidate(candidate))
                return true;
        }

        return false;
    }

    /// <summary>
    /// OSK text.
    /// </summary>
    protected virtual IEnumerable<string> EnumerateCandidates()
    {
        var systemDirectory = Environment.SystemDirectory;
        if (!string.IsNullOrWhiteSpace(systemDirectory))
            yield return Path.Combine(systemDirectory, "osk.exe");

        var windowsDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Windows);
        if (!string.IsNullOrWhiteSpace(windowsDirectory))
            yield return Path.Combine(windowsDirectory, "System32", "osk.exe");

        // text.
        yield return "osk.exe";
    }

    /// <summary>
    /// text.
    /// </summary>
    protected virtual bool TryLaunchCandidate(string candidate)
    {
        try
        {
            Process.Start(new ProcessStartInfo(candidate)
            {
                UseShellExecute = true
            });
            return true;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[OskLauncher] Launch failed: {candidate} / {ex.Message}");
            return false;
        }
    }
}
