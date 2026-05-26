using System.Diagnostics;
using System.IO;

namespace LArtKey.Services;

/// <summary>T-9.5: English text</summary>
public class InstallerService
{
    /// <summary>
    /// English text.
    /// </summary>
    /// <param name="installerPath">English text</param>
    /// <param name="autoRestart">English text</param>
    /// <param name="requestElevation">runasEnglish text</param>
    /// <returns>English text</returns>
    public async Task<int> RunInstallerAsync(
        string installerPath,
        bool autoRestart = false,
        bool requestElevation = true)
    {
        if (!File.Exists(installerPath))
            throw new FileNotFoundException($"Installer not found: {installerPath}");

        var psi = new ProcessStartInfo
        {
            FileName = installerPath,
            Arguments = GetArguments(autoRestart),
            UseShellExecute = true
        };

        // English text.
        if (requestElevation)
            psi.Verb = "runas";

        using var process = Process.Start(psi);
        if (process == null) return -1;

        await process.WaitForExitAsync();
        var exitCode = process.ExitCode;

        // English text
        try { File.Delete(installerPath); }
        catch { /* English text */ }

        return exitCode;
    }

    /// <summary>
    /// English text)
    /// </summary>
    public void StartInstaller(
        string installerPath,
        bool autoRestart = true,
        bool requestElevation = true)
    {
        if (!File.Exists(installerPath))
            throw new FileNotFoundException($"Installer not found: {installerPath}");

        var psi = new ProcessStartInfo
        {
            FileName = installerPath,
            Arguments = GetArguments(autoRestart),
            UseShellExecute = true
        };

        // English text.
        if (requestElevation)
            psi.Verb = "runas";

        Process.Start(psi);
    }

    private string GetArguments(bool autoRestart)
    {
        // Inno Setup English text
        return autoRestart
            ? "/VERYSILENT /SUPPRESSMSGBOXES /CLOSEAPPLICATIONS /AUTORESTART /LOG"
            : "/VERYSILENT /SUPPRESSMSGBOXES /NORESTART /CLOSEAPPLICATIONS";
    }
}
