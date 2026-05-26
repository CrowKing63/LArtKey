using System.Diagnostics;
using System.IO;

namespace LAltKey.Services;

/// <summary>T-9.5: text</summary>
public class InstallerService
{
    /// <summary>
    /// text.
    /// </summary>
    /// <param name="installerPath">text</param>
    /// <param name="autoRestart">text</param>
    /// <param name="requestElevation">runastext</param>
    /// <returns>text</returns>
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

        // text.
        if (requestElevation)
            psi.Verb = "runas";

        using var process = Process.Start(psi);
        if (process == null) return -1;

        await process.WaitForExitAsync();
        var exitCode = process.ExitCode;

        // text
        try { File.Delete(installerPath); }
        catch { /* text */ }

        return exitCode;
    }

    /// <summary>
    /// text)
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

        // text.
        if (requestElevation)
            psi.Verb = "runas";

        Process.Start(psi);
    }

    private string GetArguments(bool autoRestart)
    {
        // Inno Setup text
        return autoRestart
            ? "/VERYSILENT /SUPPRESSMSGBOXES /CLOSEAPPLICATIONS /AUTORESTART /LOG"
            : "/VERYSILENT /SUPPRESSMSGBOXES /NORESTART /CLOSEAPPLICATIONS";
    }
}
